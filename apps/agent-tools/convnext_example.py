import torch
from PIL import Image
import requests
from transformers import ConvNextFeatureExtractor, ConvNextForImageClassification
import logging

# 配置日志
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("ConvNeXtApp")

def classify_image(image_source: str, is_url: bool = True):
    """
    使用 ConvNeXt 模型进行图像分类的交互示例
    
    :param image_source: 图像的 URL 或本地路径
    :param is_url: 是否为 URL，默认为 True
    """
    model_name = "facebook/convnext-base-224"
    
    try:
        # 1. 加载图像 (Load Image)
        logger.info(f"正在获取图像: {image_source}")
        if is_url:
            image = Image.open(requests.get(image_source, stream=True).raw)
        else:
            image = Image.open(image_source)
        
        # 2. 初始化模型和提取器 (Initialize Model and Extractor)
        # 第一次运行会自动从 Hugging Face 下载模型文件 (Approx. 350MB)
        logger.info("正在加载 ConvNeXt 模型和特征提取器...")
        feature_extractor = ConvNextFeatureExtractor.from_pretrained(model_name)
        model = ConvNextForImageClassification.from_pretrained(model_name)
        
        # 3. 预处理图像 (Preprocess Image)
        # 将 PIL 图像转换为模型需要的 PyTorch 张量格式
        inputs = feature_extractor(images=image, return_tensors="pt")
        
        # 4. 推理 (Inference)
        logger.info("正在进行图像识别...")
        with torch.no_grad():
            outputs = model(**inputs)
            logits = outputs.logits
            
        # 5. 解析结果 (Parse Results)
        # 获取概率最高的类别索引
        predicted_label_idx = logits.argmax(-1).item()
        # 从模型配置中获取人类可读的标签
        label_name = model.config.id2label[predicted_label_idx]
        
        print("-" * 30)
        print(f"识别成功！")
        print(f"预测类别索引: {predicted_label_idx}")
        print(f"预测类别名称: {label_name}")
        print("-" * 30)
        
        return label_name

    except Exception as e:
        logger.error(f"处理过程中出现错误: {str(e)}")
        return None

# --- 提示：此脚本可作为构建 Agent 工具的参考逻辑 ---
# 想法：你可以将此函数包装在 @tool 中，让 Agent 具备视觉分类能力。
# 例如：
# @tool("image_classifier")
# def image_classifier_tool(url: str) -> str:
#     \"\"\"识别给定 URL 图像中的物体内容\"\"\"
#     return classify_image(url)

if __name__ == "__main__":
    # 示例图像：经典的 COCO 数据集中的猫咪图片
    test_url = "http://images.cocodataset.org/val2017/000000039769.jpg"
    classify_image(test_url)
