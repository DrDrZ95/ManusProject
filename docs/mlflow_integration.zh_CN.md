# MLflow 集成指南

## 1. MLflow 简介

MLflow 是一个开源平台，旨在管理端到端机器学习 (ML) 生命周期。它由 Databricks 开发，解决了 ML 开发中的关键挑战，包括实验跟踪、可复现性和模型部署。MLflow 提供了一套工具，帮助数据科学家和机器学习工程师简化工作流程并更有效地协作。

**MLflow 的主要组件：**

*   **MLflow Tracking（跟踪）**：允许您记录和查询实验，包括代码、数据、配置和结果。它使您能够跟踪每次运行的参数、指标和工件（例如模型、图表）。
*   **MLflow Projects（项目）**：为以可重用和可复现的方式打包 ML 代码提供标准格式。这使得 ML 代码可以在不同平台之间共享，并确保一致的执行。
*   **MLflow Models（模型）**：提供了一种将 ML 模型打包成标准格式的约定，可供各种下游工具使用（例如，用于实时服务或批量推理）。它支持不同类型的模型（例如，scikit-learn、TensorFlow、PyTorch）。
*   **MLflow Model Registry（模型注册表）**：一个集中的模型存储库，允许您管理 MLflow 模型的生命周期，包括版本控制、阶段转换（例如，Staging、Production）和注释。

## 2. 基本安装过程

MLflow 可以使用 `pip` 轻松安装。在安装之前，建议创建一个虚拟环境来管理依赖项。

### 步骤 1：创建虚拟环境（推荐）

```bash
python3 -m venv mlflow-env
source mlflow-env/bin/activate  # 在 Linux/macOS 上
# 对于 Windows: .\mlflow-env\Scripts\activate
```

### 步骤 2：安装 MLflow

激活虚拟环境后，您可以使用 pip 安装 MLflow：

```bash
pip install mlflow
```

### 步骤 3：验证安装

要验证 MLflow 是否已正确安装，您可以运行以下命令：

```bash
mlflow --version
```

这将输出已安装的 MLflow 版本。

### 步骤 4：启动 MLflow UI（可选）

MLflow 带有一个基于 Web 的用户界面，允许您可视化、搜索和比较 ML 运行。要启动 UI，请导航到您的项目目录并运行：

```bash
mlflow ui
```

默认情况下，MLflow UI 将在 `http://localhost:5000` 上可访问。然后，您可以在 Web 浏览器中导航到此 URL 以查看您的实验。

### 步骤 5：基本使用示例 (Python)

这是一个简单的 Python 示例，演示如何使用 MLflow Tracking 记录参数和指标：

```python
import mlflow
import mlflow.sklearn
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score

# 为 scikit-learn 启用自动日志记录
mlflow.sklearn.autolog()

# 启动 MLflow 运行
with mlflow.start_run():
    # 记录参数
    mlflow.log_param("learning_rate", 0.01)
    mlflow.log_param("n_estimators", 100)

    # 训练一个简单的模型
    model = LogisticRegression(solver="liblinear", random_state=42)
    # 在实际场景中，您将在实际数据上进行训练
    # 为了演示，我们只模拟一个训练过程
    # model.fit(X_train, y_train)

    # 模拟预测和指标
    y_true = [0, 1, 0, 1, 0]
    y_pred = [0, 1, 1, 1, 0]
    accuracy = accuracy_score(y_true, y_pred)

    # 记录指标
    mlflow.log_metric("accuracy", accuracy)

    # 记录模型（可选，自动日志记录可能会处理此问题）
    # mlflow.sklearn.log_model(model, "logistic_regression_model")

    print(f"MLflow Run ID: {mlflow.active_run().info.run_id}")
    print(f"Accuracy: {accuracy}")
```

运行此脚本后，您可以打开 MLflow UI (`mlflow ui`) 查看已记录的实验运行，包括参数、指标以及可能的模型工件。

