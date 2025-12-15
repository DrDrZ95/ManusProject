import re
import os
from typing import List
from langchain_community.document_loaders import DirectoryLoader
from langchain_core.documents import Document
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_community.vectorstores import Chroma
from langchain_openai import OpenAIEmbeddings, ChatOpenAI
from langchain.retrievers import ContextualCompressionRetriever
from langchain.retrievers.document_compressors import LLMChainExtractor
from langchain.chains import RetrievalQA
from langchain_core.prompts import PromptTemplate

# --- 1. Advanced Document Loading & Cleaning (高级文档加载与清洗) ---

def clean_text_function(text: str) -> str:
    """
    自定义文本清洗函数 (Custom Text Cleaning Function)
    
    设计模式: 策略模式 (Strategy Pattern) 的简单应用，作为加载器的一个处理步骤。
    功能描述: 使用正则表达式移除文档中的常见“噪音”，例如重复的页眉、特殊字符和多余的空白。
    
    :param text: 原始文本 (Original text)
    :return: 清洗后的文本 (Cleaned text)
    """
    # 移除重复的空白符 (Remove extra whitespace)
    text = re.sub(r'\s+', ' ', text)
    
    # 移除常见的页眉/页脚模式 (Remove common header/footer patterns)
    # 示例: 移除 "Page X of Y" 或 "Confidential Document"
    text = re.sub(r'Page \d+ of \d+', '', text, flags=re.IGNORECASE)
    text = re.sub(r'Confidential Document', '', text, flags=re.IGNORECASE)
    
    # 移除特殊字符或非打印字符 (Remove special or non-printable characters)
    # 仅保留字母、数字、标点和基本空白
    text = re.sub(r'[^\w\s.,;!?"\'\(\)\[\]\-]', '', text)
    
    # 移除行首行尾的空白 (Strip leading/trailing whitespace)
    text = text.strip()
    
    return text

def load_and_clean_documents(directory_path: str) -> List[Document]:
    """
    加载指定目录下的文档并进行清洗 (Load documents from a directory and clean them)
    
    :param directory_path: 包含文档的目录路径 (Directory path containing documents)
    :return: 清洗后的文档列表 (List of cleaned documents)
    """
    # 使用 DirectoryLoader 加载所有文件 (Use DirectoryLoader to load all files)
    # 假设我们主要处理PDF文件，但可以扩展到其他类型 (Assuming we mainly handle PDF, but can extend)
    # loader = DirectoryLoader(
    #     directory_path, 
    #     glob="**/*.pdf", 
    #     loader_cls=PyPDFLoader, # 需要安装 pypdf
    #     loader_kwargs={"text_content_func": clean_text_function} # 假设加载器支持自定义清洗函数
    # )
    
    # 由于 DirectoryLoader 的 clean_text_function 并非标准参数，我们采用先加载后清洗的策略
    # Since clean_text_function is not a standard DirectoryLoader argument, we use a load-then-clean strategy
    
    # 仅为演示目的，我们使用一个通用的文本加载器 (For demonstration, we use a generic text loader)
    # 在实际应用中，应使用针对特定文件类型的加载器 (In a real app, use specific file type loaders)
    loader = DirectoryLoader(
        directory_path, 
        glob="**/*", 
        silent_errors=True
    )
    
    print(f"正在从 {directory_path} 加载文档...")
    documents = loader.load()
    
    cleaned_documents = []
    for doc in documents:
        # 对每个文档的内容进行清洗 (Clean the content of each document)
        cleaned_content = clean_text_function(doc.page_content)
        
        # 创建新的文档对象 (Create a new document object)
        cleaned_doc = Document(
            page_content=cleaned_content,
            metadata=doc.metadata
        )
        cleaned_documents.append(cleaned_doc)
        
    print(f"已加载并清洗 {len(cleaned_documents)} 个文档。")
    return cleaned_documents

# 示例用法 (Example Usage - 需要一个实际的目录)
# if __name__ == "__main__":
#     # 假设在当前目录下有一个名为 "data" 的文件夹 (Assume there is a folder named "data" in the current directory)
#     # documents = load_and_clean_documents("./data")
#     # print(documents[0].page_content[:500])
#     pass


# --- 2. Smart Text Splitting (智能文本分割) ---

def split_documents(documents: List[Document], chunk_size: int = 1500, chunk_overlap: int = 150) -> List[Document]:
    """
    智能文本分割函数 (Smart Text Splitting Function)
    
    设计模式: 责任链模式 (Chain of Responsibility) 的一部分，负责将大文档分割成小块。
    功能描述: 使用 RecursiveCharacterTextSplitter，针对技术文档的特点进行配置，
              以保留上下文的连贯性。
    
    :param documents: 清洗后的文档列表 (List of cleaned documents)
    :param chunk_size: 每个块的最大字符数 (Maximum characters per chunk)
    :param chunk_overlap: 块之间的重叠字符数 (Overlap characters between chunks)
    :return: 分割后的文档块列表 (List of split document chunks)
    """
    # 针对技术文档，使用默认的分割符列表，它会尝试按段落、句子等递归分割
    # For technical documents, use the default separators list, which recursively tries to split by paragraph, sentence, etc.
    text_splitter = RecursiveCharacterTextSplitter(
        chunk_size=chunk_size,
        chunk_overlap=chunk_overlap,
        length_function=len,
        # 默认分隔符: ["\n\n", "\n", " ", ""]
        # Default separators: ["\n\n", "\n", " ", ""]
    )
    
    print(f"正在将文档分割成块 (Chunk Size: {chunk_size}, Overlap: {chunk_overlap})...")
    chunks = text_splitter.split_documents(documents)
    print(f"分割完成，共生成 {len(chunks)} 个文本块。")
    
    return chunks

# 示例用法 (Example Usage - 需要一个实际的目录)
# if __name__ == "__main__":
#     # 假设在当前目录下有一个名为 "data" 的文件夹 (Assume there is a folder named "data" in the current directory)
#     # documents = load_and_clean_documents("./data")
#     # if documents:
#     #     chunks = split_documents(documents)
#     #     print(chunks[0].page_content)
#     pass


# --- 3. Retrieval & Re-ranking Core (检索与重排核心) ---

def setup_vector_store(chunks: List[Document], collection_name: str = "agent_knowledge_base") -> Chroma:
    """
    设置向量存储 (Setup Vector Store)
    
    设计模式: 工厂模式 (Factory Pattern) 的简单应用，用于创建 Chroma 实例。
    功能描述: 使用 ChromaDB 作为向量存储，并使用 OpenAIEmbeddings (或兼容的本地模型) 
              将文本块转换为向量。
    
    :param chunks: 分割后的文档块列表 (List of split document chunks)
    :param collection_name: 向量集合名称 (Name of the vector collection)
    :return: Chroma 向量存储实例 (Chroma vector store instance)
    """
    # 嵌入模型选择 (Embedding Model Selection)
    # 生产环境中，可以替换为本地模型，例如 Sentence Transformers
    # In production, replace with a local model, e.g., Sentence Transformers
    embeddings = OpenAIEmbeddings() 
    
    # 初始化 Chroma 向量存储 (Initialize Chroma vector store)
    # 默认使用内存存储，可配置为持久化存储 (Default is in-memory, can be configured for persistence)
    vector_store = Chroma.from_documents(
        documents=chunks,
        embedding=embeddings,
        collection_name=collection_name
    )
    
    print(f"已创建 Chroma 向量存储，包含 {vector_store._collection.count()} 个文档。")
    return vector_store

def setup_compression_retriever(vector_store: Chroma, llm: ChatOpenAI, k: int = 5, fetch_k: int = 20) -> ContextualCompressionRetriever:
    """
    设置上下文压缩检索器 (Setup Contextual Compression Retriever)
    
    设计模式: 装饰器模式 (Decorator Pattern)，ContextualCompressionRetriever 装饰了 VectorStoreRetriever。
    功能描述: 
    1. 从向量存储中检索出 top-k (fetch_k) 个文档。
    2. 使用 LLMChainExtractor (或 FlashRank) 对检索到的文档进行重排和压缩，只保留与查询最相关的部分。
    
    :param vector_store: Chroma 向量存储实例 (Chroma vector store instance)
    :param llm: 用于压缩的 LLM 实例 (LLM instance for compression)
    :param k: 最终返回的文档数量 (Number of final documents to return)
    :param fetch_k: 初始检索的文档数量 (Number of initial documents to fetch)
    :return: ContextualCompressionRetriever 实例 (ContextualCompressionRetriever instance)
    """
    # 1. 创建基础检索器 (Create base retriever)
    base_retriever = vector_store.as_retriever(search_kwargs={"k": fetch_k})
    
    # 2. 创建文档压缩器 (Create document compressor)
    # LLMChainExtractor: 使用 LLM 来提取相关信息，实现压缩 (Uses LLM to extract relevant info for compression)
    compressor = LLMChainExtractor.from_llm(llm)
    
    # 性能提示: 如果需要更高的速度，可以考虑使用 FlashRank 或 BGE-Reranker 等专门的重排模型。
    # Performance Tip: For higher speed, consider using specialized rerankers like FlashRank or BGE-Reranker.
    # compressor = FlashrankRerank(model="rank-bge-small-v1.5", top_n=k) 
    
    # 3. 创建压缩检索器 (Create compression retriever)
    compression_retriever = ContextualCompressionRetriever(
        base_compressor=compressor, 
        base_retriever=base_retriever
    )
    
    print(f"已设置压缩检索器 (初始检索: {fetch_k}, 最终返回: {k})。")
    return compression_retriever

# 示例用法 (Example Usage)
# if __name__ == "__main__":
#     # 假设我们已经有了 chunks (Assume we already have chunks)
#     # vector_store = setup_vector_store(chunks)
#     # llm = ChatOpenAI(temperature=0)
#     # retriever = setup_compression_retriever(vector_store, llm)
#     # 
#     # query = "什么是 RecursiveCharacterTextSplitter 的主要用途?"
#     # compressed_docs = retriever.get_relevant_documents(query)
#     # print(f"检索到的压缩文档数量: {len(compressed_docs)}")
#     pass


# --- 4. Integration with the LLM (与LLM集成) ---

def create_rag_chain(retriever: ContextualCompressionRetriever, llm: ChatOpenAI) -> RetrievalQA:
    """
    创建检索增强生成 (RAG) 链 (Create Retrieval-Augmented Generation Chain)
    
    设计模式: 组合模式 (Composite Pattern)，将检索器和LLM组合成一个单一的链。
    功能描述: 使用 RetrievalQAChain，它接受压缩后的上下文，并将其与用户问题一起发送给LLM以生成答案。
    
    :param retriever: 上下文压缩检索器 (Contextual Compression Retriever)
    :param llm: LLM 实例 (LLM instance)
    :return: RetrievalQAChain 实例 (RetrievalQAChain instance)
    """
    
    # 提示词模板 (Prompt Template)
    # 确保提示词清晰地指示LLM使用提供的上下文 (Ensure the prompt clearly instructs the LLM to use the provided context)
    template = """
    你是一个专业的问答助手，请根据提供的上下文信息来回答问题。
    如果上下文中没有足够的信息，请回答“根据提供的资料，我无法回答这个问题。”
    
    上下文:
    {context}
    
    问题:
    {question}
    
    回答:
    """
    QA_CHAIN_PROMPT = PromptTemplate.from_template(template)
    
    # --- LLM 替换注释 (LLM Swapping Note) ---
    # 当前使用 ChatOpenAI 作为标准LLM。
    # To swap the standard OpenAI LLM for a local fine-tuned model endpoint:
    # 1. 使用 HuggingFacePipeline (需要安装 transformers, torch)
    #    from langchain_community.llms import HuggingFacePipeline
    #    llm = HuggingFacePipeline.from_model_id(
    #        model_id="local_model_name", 
    #        task="text-generation", 
    #        pipeline_kwargs={"max_new_tokens": 100}
    #    )
    # 2. 使用自定义的 API 客户端 (例如，如果本地模型暴露了 OpenAI 兼容的 API)
    #    llm = ChatOpenAI(openai_api_base="http://localhost:8000/v1", openai_api_key="not-needed")
    # -------------------------------------------
    
    qa_chain = RetrievalQA.from_chain_type(
        llm=llm,
        chain_type="stuff", # 简单地将所有文档“塞”进提示词 (Simply "stuffs" all documents into the prompt)
        retriever=retriever,
        chain_type_kwargs={"prompt": QA_CHAIN_PROMPT},
        return_source_documents=True # 返回来源文档 (Return source documents)
    )
    
    print("已创建 RetrievalQAChain。")
    return qa_chain

# 示例主函数 (Example Main Function)
# if __name__ == "__main__":
#     # 1. 准备数据 (Prepare Data)
#     # 假设我们有一个虚拟的文档目录 (Assume we have a virtual document directory)
#     # documents = load_and_clean_documents("./data")
#     # chunks = split_documents(documents)
#     
#     # 2. 设置核心组件 (Setup Core Components)
#     # llm_for_compression = ChatOpenAI(temperature=0, model="gpt-3.5-turbo")
#     # llm_for_qa = ChatOpenAI(temperature=0, model="gpt-3.5-turbo")
#     # vector_store = setup_vector_store(chunks)
#     # retriever = setup_compression_retriever(vector_store, llm_for_compression)
#     
#     # 3. 创建 RAG 链 (Create RAG Chain)
#     # rag_chain = create_rag_chain(retriever, llm_for_qa)
#     
#     # 4. 执行查询 (Execute Query)
#     # query = "什么是 RecursiveCharacterTextSplitter 的主要用途?"
#     # result = rag_chain.invoke({"query": query})
#     # print("\n--- 结果 ---")
#     # print(result["result"])
#     # print("\n--- 来源 ---")
#     # for doc in result["source_documents"]:
#     #     print(f"- {doc.metadata.get('source')}")
#     pass

