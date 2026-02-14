import re
import os
import logging
from typing import List, Optional, Any, Dict, Union
from langchain_community.document_loaders import DirectoryLoader
from langchain_core.documents import Document
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_community.vectorstores import Chroma
from langchain_openai import OpenAIEmbeddings, ChatOpenAI
from langchain.retrievers import ContextualCompressionRetriever
from langchain.retrievers.document_compressors import LLMChainExtractor
from langchain.chains import RetrievalQA
from langchain_core.prompts import PromptTemplate
from langchain.tools import tool
from langchain_core.messages import ToolMessage
from langchain.agents.output_parsers import OpenAIFunctionsAgentOutputParser
from langchain_huggingface import HuggingFaceEmbeddings

# 配置日志 (Configure Logging)
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger("AgentTools")

# --- 自定义 Runtime 类 (Custom Runtime Class) ---
class ToolRuntime:
    """
    用于访问会话信息和状态的运行时环境 (Runtime environment for accessing session info and state)
    """
    def __init__(self, state: Dict[str, Any]):
        self.state = state

# --- 1. Advanced Document Loading & Cleaning (高级文档加载与清洗) ---

def clean_text_function(text: str) -> str:
    """
    自定义文本清洗函数 (Custom Text Cleaning Function)
    
    功能描述: 
    1. 移除重复空白、页眉页脚模式。
    2. 新增: 去除 HTML 标记和乱码字符。
    3. 新增: 统一处理不同语言（中英文）的标点符号。
    
    :param text: 原始文本 (Original text)
    :return: 清洗后的文本 (Cleaned text)
    """
    # 1. 移除 HTML 标记 (Remove HTML tags)
    text = re.sub(r'<[^>]+>', '', text)
    
    # 2. 移除乱码字符 (Remove garbled characters)
    # 移除不可见字符和非法 Unicode (Remove non-printable and illegal Unicode)
    text = "".join(ch for ch in text if ch.isprintable())
    
    # 3. 统一标点处理 (Unify punctuation for different languages)
    # 将中文标点转换为英文标点或统一格式 (Convert Chinese punctuation to English or unified format)
    punctuation_map = {
        '，': ',', '。': '.', '！': '!', '？': '?', '：': ':', '；': ';',
        '“': '"', '”': '"', '‘': "'", '’': "'", '（': '(', '）': ')',
        '【': '[', '】': ']', '—': '-'
    }
    for zh_punc, en_punc in punctuation_map.items():
        text = text.replace(zh_punc, en_punc)

    # 4. 移除重复的空白符 (Remove extra whitespace)
    text = re.sub(r'\s+', ' ', text)
    
    # 5. 移除常见的页眉/页脚模式 (Remove common header/footer patterns)
    text = re.sub(r'Page \d+ of \d+', '', text, flags=re.IGNORECASE)
    text = re.sub(r'Confidential Document', '', text, flags=re.IGNORECASE)
    
    # 6. 移除特殊字符 (仅保留字母、数字、常用标点和基本空白)
    text = re.sub(r'[^\w\s.,;!?"\'\(\)\[\]\-]', '', text)
    
    return text.strip()

def load_and_clean_documents(directory_path: str) -> List[Document]:
    """
    加载并清洗文档 (Load and clean documents)
    """
    logger.info(f"正在从 {directory_path} 加载文档...")
    loader = DirectoryLoader(directory_path, glob="**/*", silent_errors=True)
    documents = loader.load()
    
    cleaned_documents = []
    for doc in documents:
        cleaned_content = clean_text_function(doc.page_content)
        cleaned_documents.append(Document(page_content=cleaned_content, metadata=doc.metadata))
        
    logger.info(f"已加载并清洗 {len(cleaned_documents)} 个文档。")
    return cleaned_documents

# --- 2. Smart Text Splitting (智能文本分割) ---

def split_documents(
    documents: List[Document], 
    chunk_size: int = 1500, 
    chunk_overlap: int = 150,
    separator_priority: Optional[List[str]] = None
) -> List[Document]:
    """
    智能文本分割函数 (Smart Text Splitting Function)
    
    :param documents: 清洗后的文档列表 (List of cleaned documents)
    :param chunk_size: 每个块的最大字符数 (Max chars per chunk)
    :param chunk_overlap: 块之间的重叠 (Overlap)
    :param separator_priority: 自定义分割优先级列表 (Custom separator priority list). 
                               若为 None，则使用默认的递归分割逻辑 ["\n\n", "\n", " ", ""].
    :return: 分割后的文档块 (Split document chunks)
    """
    separators = separator_priority if separator_priority else ["\n\n", "\n", " ", ""]
    
    text_splitter = RecursiveCharacterTextSplitter(
        chunk_size=chunk_size,
        chunk_overlap=chunk_overlap,
        length_function=len,
        separators=separators
    )
    
    logger.info(f"正在使用优先级 {separators} 分割文档 (Size: {chunk_size})...")
    chunks = text_splitter.split_documents(documents)
    logger.info(f"分割完成，共生成 {len(chunks)} 个文本块。")
    
    return chunks

# --- 3. Vector Store & Compression (向量存储与压缩) ---

def setup_vector_store(
    chunks: List[Document], 
    embedding_name: str = "openai",
    persist_directory: Optional[str] = None,
    collection_name: str = "agent_knowledge_base"
) -> Chroma:
    """
    设置向量存储 (Setup Vector Store)
    
    :param chunks: 文档块 (Document chunks)
    :param embedding_name: 嵌入模型名称 (Embedding model name). 支持 "openai", "huggingface" 或自定义路径。
    :param persist_directory: 持久化目录 (Persistence directory).
    :param collection_name: 集合名称 (Collection name).
    :return: Chroma 实例 (Chroma instance)
    """
    logger.info(f"正在初始化嵌入模型: {embedding_name}")
    
    if embedding_name.lower() == "openai":
        embeddings = OpenAIEmbeddings()
    elif embedding_name.lower() == "huggingface":
        embeddings = HuggingFaceEmbeddings(model_name="all-MiniLM-L6-v2")
    else:
        # 尝试作为本地路径加载 (Try loading as local path)
        embeddings = HuggingFaceEmbeddings(model_name=embedding_name)
    
    vector_store = Chroma.from_documents(
        documents=chunks,
        embedding=embeddings,
        collection_name=collection_name,
        persist_directory=persist_directory
    )
    
    logger.info(f"向量存储设置完成 (持久化: {persist_directory})。")
    return vector_store

def setup_compression_retriever(
    vector_store: Chroma, 
    llm: Any, 
    compressor_type: str = "LLMChainExtractor",
    k: int = 5, 
    fetch_k: int = 20
) -> ContextualCompressionRetriever:
    """
    设置压缩检索器 (Setup Compression Retriever)
    
    :param vector_store: 向量存储 (Vector store)
    :param llm: 用于压缩的 LLM (LLM for compression)
    :param compressor_type: 压缩器类型 (Compressor type). 
                            - "LLMChainExtractor": 效果好但慢 (High quality, slow)
                            - "FlashRank": 速度快适合大规模 (Fast, suitable for large scale)
                            - "BGE-Reranker": 精度极高 (Very high precision)
    :param k: 最终返回结果数量 (Final return count). 控制最终展示给 LLM 的上下文数量。
    :param fetch_k: 初始检索数量 (Initial fetch count). 控制从向量库初筛的候选数量。
    :return: ContextualCompressionRetriever 实例
    """
    base_retriever = vector_store.as_retriever(search_kwargs={"k": fetch_k})
    
    logger.info(f"正在配置压缩器: {compressor_type} (fetch_k={fetch_k}, k={k})")
    
    if compressor_type == "LLMChainExtractor":
        compressor = LLMChainExtractor.from_llm(llm)
    elif compressor_type == "FlashRank":
        # 需要安装 flashrank (Requires flashrank)
        from langchain.retrievers.document_compressors import FlashrankRerank
        compressor = FlashrankRerank(top_n=k)
    elif compressor_type == "BGE-Reranker":
        # 示例：使用自定义 BGE 压缩器 (Example: using custom BGE compressor)
        # 这里仅作示意，实际需根据具体库实现 (Placeholder for actual BGE implementation)
        compressor = LLMChainExtractor.from_llm(llm) 
    else:
        compressor = LLMChainExtractor.from_llm(llm)

    return ContextualCompressionRetriever(
        base_compressor=compressor, 
        base_retriever=base_retriever
    )

def create_rag_chain(retriever: ContextualCompressionRetriever, llm: Any) -> RetrievalQA:
    """
    创建 RAG 链 (Create RAG Chain)
    """
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
    
    return RetrievalQA.from_chain_type(
        llm=llm,
        chain_type="stuff",
        retriever=retriever,
        return_source_documents=True,
        chain_type_kwargs={"prompt": QA_CHAIN_PROMPT}
    )

# --- 4. Tool Definitions (工具定义) ---

@tool("search_docs")
def search_documents(query: str, limit: int = 5) -> str:
    """
    在内部文档库中搜索匹配的记录 (Search for matching records in the internal document library).
    
    :param query: 搜索查询语句 (Search query string)
    :param limit: 返回结果的最大数量 (Maximum number of results to return)
    :return: 搜索状态描述 (Search status description)
    """
    logger.info(f"调用 search_documents: query='{query}', limit={limit}")
    return f"Found {limit} results for '{query}'"

@tool("ask_with_rag")
def ask_with_rag(question: str, directory_path: str, runtime: Optional[ToolRuntime] = None) -> str:
    """
    使用检索增强生成 (RAG) 回答问题 (Answer questions using Retrieval-Augmented Generation).
    
    :param question: 用户问题 (User question)
    :param directory_path: 文档所在目录 (Directory path of documents)
    :param runtime: 运行时环境，用于获取会话状态 (Runtime environment for session state)
    :return: 包含答案和引用数量的字符串 (Answer with source count)
    """
    logger.info(f"开始 RAG 流程: 问题='{question}', 目录='{directory_path}'")
    
    try:
        # 1. 加载与清洗 (Load & Clean)
        docs = load_and_clean_documents(directory_path)
        if not docs:
            return "目录中未发现有效文档，请检查路径。"

        # 2. 分割 (Split)
        chunks = split_documents(docs)

        # 3. 向量存储与检索 (Vector Store & Retriever)
        llm = ChatOpenAI(temperature=0, model="gpt-3.5-turbo")
        vector_store = setup_vector_store(chunks)
        retriever = setup_compression_retriever(vector_store, llm)

        # 4. 执行链 (Execute Chain)
        rag_chain = create_rag_chain(retriever, llm)
        logger.info("正在执行 RAG 链查询...")
        result = rag_chain({"query": question})
        
        answer = result["result"]
        source_count = len(result["source_documents"])
        
        # 示例：使用 runtime 访问状态 (Example: accessing state via runtime)
        if runtime and runtime.state:
            session_id = runtime.state.get("session_id", "unknown")
            logger.info(f"会话 {session_id} 的 RAG 查询完成。")

        return f"回答: {answer}\n(引用文档数量: {source_count})"

    except Exception as e:
        logger.error(f"RAG 流程出错: {str(e)}")
        raise e

# --- 5. Error Handling & Middleware (错误处理与中间件) ---

def handle_tool_errors(error: Exception) -> str:
    """
    统一的工具错误处理回调 (Unified tool error handling callback)
    """
    if isinstance(error, (ValueError, TypeError)):
        return f"工具调用参数错误: {str(error)}。请检查输入格式。"
    return "工具调用失败，请检查输入或稍后重试。"

# 模拟 @wrap_tool_call 中间件逻辑 (Simulated @wrap_tool_call logic)
# 在实际 LangGraph 或 LangChain 应用中，这通常通过装饰器或 ToolNode 配置实现
def wrap_tool_call(tool_func):
    """
    包装工具调用，捕获异常并返回 ToolMessage (Wrap tool call to catch exceptions and return ToolMessage)
    """
    def wrapper(*args, **kwargs):
        try:
            return tool_func(*args, **kwargs)
        except Exception as e:
            # 这里的 tool_call_id 需要从上下文中获取 (In practice, tool_call_id is needed)
            return ToolMessage(
                content=handle_tool_errors(e),
                tool_call_id="temp_id" 
            )
    return wrapper

# 注意：根据需求 1，ToolNode 的 handle_tool_errors 参数应在创建 ToolNode 时设置。
# 例如: tool_node = ToolNode(tools, handle_tool_errors=handle_tool_errors)
