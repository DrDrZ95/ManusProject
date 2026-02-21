namespace Agent.Metering.Finetuning;

public sealed class LoraFinetuneOptions
{
    // 0.这步是基础模型相关配置，新手可以先使用默认的 DeepSeek Coder
    public string BaseModelName { get; set; } = "deepseek-coder";

    // 0.1 这步是备用模型名称，比如想切换到 Llama 时可以修改这里
    public string? AlternateBaseModelName { get; set; } = "llama-3";

    // 0.2 这步是训练数据集的路径，建议使用绝对路径方便排查问题
    public string DatasetPath { get; set; } = string.Empty;

    // 0.3 这步是输出目录，微调后的权重和日志都会写到这里
    public string OutputDirectory { get; set; } = string.Empty;

    // 0.4 这步是最大训练步数，用于控制训练时间与成本
    public int MaxSteps { get; set; } = 1000;

    // 0.5 这步是 batch size，显存不够时可以调小
    public int BatchSize { get; set; } = 4;

    // 0.6 这步是学习率，数值越大收敛越快但不稳定，新手可以先用默认值
    public double LearningRate { get; set; } = 1e-4;

    // 0.7 这步是 LoRA 的 rank，控制可训练参数量，越高表示表达能力越强
    public string LoraRank { get; set; } = "16";

    // 0.8 这步是 LoRA 的 alpha，通常与 rank 搭配使用
    public string LoraAlpha { get; set; } = "32";

    // 0.9 这步表示是否使用 4bit 量化，打开后能显著降低显存占用
    public bool Use4BitQuantization { get; set; } = true;

    // 0.10 这步是 Python 可执行文件名称或路径，默认使用环境中的 python
    public string PythonExecutable { get; set; } = "python";

    // 0.11 这步是训练脚本名称，默认假设为当前工作目录下的 train_lora_unsloth.py
    public string TrainingScript { get; set; } = "train_lora_unsloth.py";

    // 0.12 这步提供一个完整的入门示例配置，开发者可以直接拷贝修改
    public static LoraFinetuneOptions CreateSample()
    {
        return new LoraFinetuneOptions
        {
            BaseModelName = "deepseek-coder",
            AlternateBaseModelName = "meta-llama/Meta-Llama-3-8B",
            DatasetPath = "/data/finetune/deepseek_train.jsonl",
            OutputDirectory = "/models/deepseek_lora_adapter",
            MaxSteps = 800,
            BatchSize = 4,
            LearningRate = 1e-4,
            LoraRank = "16",
            LoraAlpha = "32",
            Use4BitQuantization = true,
            PythonExecutable = "python",
            TrainingScript = "train_lora_unsloth.py"
        };
    }
}

public sealed class LoraFinetuneProcessRunner
{
    private readonly ILogger<LoraFinetuneProcessRunner> _logger;

    public LoraFinetuneProcessRunner(ILogger<LoraFinetuneProcessRunner> logger)
    {
        _logger = logger;
    }

    // 1.这步是对外暴露的主入口，用于启动一次 LoRA 微调流程
    //   - options: 微调需要的所有参数
    //   - progress: 用于向调用方报告当前执行到哪一步（可选）
    //   - cancellationToken: 用于在外部需要时取消训练进程
    public async Task<int> RunAsync(LoraFinetuneOptions options, IProgress<string>? progress, CancellationToken cancellationToken)
    {
        progress ??= new Progress<string>(_ => { });

        // 1.1 这步实现了基础参数检查与准备，确保路径与超参数在合理范围内
        ValidateOptions(options);
        progress.Report("1. 已完成基础参数检查与准备。");

        // 2.这步实现了微调数据集的前置说明与建议，帮助新手理解数据准备流程
        await PrepareDatasetAsync(options, progress, cancellationToken);
        progress.Report("2. 已完成数据集预处理与切分建议输出。");

        // 3.这步实现了 LoRA + unsloth 环境准备说明，涵盖依赖安装与显存需求
        await DescribeEnvironmentAsync(options, progress, cancellationToken);
        progress.Report("3. 已完成 LoRA + unsloth 训练环境配置步骤说明。");

        // 4.这步实现了实际要执行的 Python 命令拼装，并通过 Process 启动训练
        var arguments = BuildTrainingArguments(options);
        progress.Report("4. 已生成 LoRA+unsloth 微调命令行参数。");

        // 4.1 这步创建了 ProcessStartInfo，用于以受控方式启动 Python 训练脚本
        var startInfo = new ProcessStartInfo
        {
            FileName = options.PythonExecutable,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        // 4.2 这步为新手展示如何订阅标准输出，将训练日志实时打印出来
        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                progress.Report($"[stdout] {e.Data}");
            }
        };

        // 4.3 这步用于捕获标准错误输出，把潜在的错误信息暴露给调用方
        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                progress.Report($"[stderr] {e.Data}");
            }
        };

        // 4.4 这步在取消信号触发时尝试安全终止训练进程，避免僵尸进程
        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch
            {
            }
        });

        progress.Report("4. 即将启动 Python 训练进程。");

        if (!process.Start())
        {
            throw new InvalidOperationException("无法启动微调训练进程，请检查 Python 与脚本配置。");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // 4.5 这步等待训练进程结束，并返回退出码，0 通常表示执行成功
        await process.WaitForExitAsync(cancellationToken);
        var exitCode = process.ExitCode;
        progress.Report($"4. 训练进程已结束，退出码为 {exitCode}。");

        // 5.这步实现了对训练过程监控与中间结果的说明，帮助理解如何评估效果
        await DescribeMonitoringAsync(options, $"{options.PythonExecutable} {arguments}", progress, cancellationToken);
        progress.Report("5. 已输出训练过程监控与中间结果检查建议。");

        // 6.这步实现了微调产物整理与导出的一般建议，方便模型落盘与部署
        await DescribeArtifactsAsync(options, progress, cancellationToken);
        progress.Report("6. 已说明微调产物的导出与落盘结构。");

        // 7.这步实现了与观测系统和推理服务的集成建议，帮助串联完整链路
        await DescribeIntegrationAsync(options, progress, cancellationToken);
        progress.Report("7. 已输出与 Agent.Metering 以及下游观测系统集成的建议。");

        return exitCode;
    }

    private static void ValidateOptions(LoraFinetuneOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.DatasetPath))
        {
            throw new ArgumentException("DatasetPath 不能为空。", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
        {
            throw new ArgumentException("OutputDirectory 不能为空。", nameof(options));
        }

        if (options.MaxSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxSteps), "MaxSteps 必须大于 0。");
        }

        if (options.BatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.BatchSize), "BatchSize 必须大于 0。");
        }

        if (options.LearningRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.LearningRate), "LearningRate 必须大于 0。");
        }
    }

    private Task PrepareDatasetAsync(LoraFinetuneOptions options, IProgress<string> progress, CancellationToken cancellationToken)
    {
        // 2.1 这步实现了对原始数据集路径的说明与检查（如 JSONL/Parquet）
        progress.Report($"2.1 将从数据集路径 {options.DatasetPath} 读取原始训练样本。");

        // 2.2 这步实现了样本格式转换的提示，例如转换为 unsloth 期望的指令/对话格式
        progress.Report("2.2 建议将原始样本转换为指令/对话格式，例如包含 'instruction'、'input'、'output' 字段。");

        // 2.3 这步实现了数据清洗的提示，比如过滤过长样本与含敏感信息的样本
        progress.Report("2.3 建议在预处理阶段过滤过长样本、空样本以及疑似包含敏感信息的记录。");

        // 2.4 这步实现了训练/验证集的切分建议，以保证评估稳定性
        progress.Report("2.4 建议将数据集按 8:2 或 9:1 切分为训练集与验证集，以便评估泛化表现。");

        return Task.CompletedTask;
    }

    private Task DescribeEnvironmentAsync(LoraFinetuneOptions options, IProgress<string> progress, CancellationToken cancellationToken)
    {
        // 3.1 这步实现了安装 unsloth 与所需依赖库的建议命令行说明
        progress.Report("3.1 建议在 Python 环境中安装 unsloth 与 transformers、accelerate、bitsandbytes 等依赖。");

        // 3.2 这步实现了设备环境检查的说明，例如 GPU/显存与多卡并行要求
        progress.Report("3.2 建议在启动训练前检查 GPU 型号与显存大小，必要时使用多卡并行或梯度累积。");

        // 3.3 这步实现了 DeepSeek 与 Llama 模型权重下载与本地缓存的建议
        progress.Report($"3.3 建议提前通过 huggingface-cli 或其他方式缓存基础模型 {options.BaseModelName} 与备用模型 {options.AlternateBaseModelName}。");

        // 3.4 这步实现了 LoRA 配置与 4bit 量化策略的说明，减小显存占用
        progress.Report("3.4 建议结合 4bit 量化与 LoRA 低秩适配，降低显存占用并加速微调。");

        return Task.CompletedTask;
    }

    private string BuildTrainingArguments(LoraFinetuneOptions options)
    {
        var builder = new StringBuilder();

        // 4.1 这步实现了基础模型名称的参数拼装，支持 DeepSeek 与 Llama 等模型
        builder.Append(" ");
        builder.Append("--base_model ");
        builder.Append(EscapeShell(options.BaseModelName));

        // 4.2 这步实现了数据集路径参数的拼装，指向预处理后的训练数据
        builder.Append(" ");
        builder.Append("--dataset_path ");
        builder.Append(EscapeShell(options.DatasetPath));

        // 4.3 这步实现了输出目录参数的拼装，用于保存 LoRA 适配器与配置
        builder.Append(" ");
        builder.Append("--output_dir ");
        builder.Append(EscapeShell(options.OutputDirectory));

        // 4.4 这步实现了训练超参数（步数、batch size、学习率）的拼装
        builder.Append(" ");
        builder.Append("--max_steps ");
        builder.Append(options.MaxSteps.ToString(CultureInfo.InvariantCulture));

        builder.Append(" ");
        builder.Append("--batch_size ");
        builder.Append(options.BatchSize.ToString(CultureInfo.InvariantCulture));

        builder.Append(" ");
        builder.Append("--learning_rate ");
        builder.Append(options.LearningRate.ToString(CultureInfo.InvariantCulture));

        // 4.5 这步实现了 LoRA 相关参数的拼装，例如 rank 与 alpha
        builder.Append(" ");
        builder.Append("--lora_rank ");
        builder.Append(EscapeShell(options.LoraRank));

        builder.Append(" ");
        builder.Append("--lora_alpha ");
        builder.Append(EscapeShell(options.LoraAlpha));

        // 4.6 这步实现了 4bit 量化开关参数的拼装，结合 unsloth 的高效加载能力
        if (options.Use4BitQuantization)
        {
            builder.Append(" --use_4bit");
        }

        return $"{EscapeShell(options.TrainingScript)} {builder}";
    }

    private Task DescribeMonitoringAsync(LoraFinetuneOptions options, string commandLine, IProgress<string> progress, CancellationToken cancellationToken)
    {
        // 5.1 这步实现了通过日志流监控训练进度与损失变化的建议
        progress.Report("5.1 建议在训练脚本中定期输出 step/loss/learning_rate 等指标到标准输出与日志文件。");

        // 5.2 这步实现了将训练日志接入 Agent.Metering 的 file-tail 采集管道的提示
        progress.Report("5.2 建议将训练日志输出到固定文件路径，并通过 Agent.Metering 的 file-tail receiver 进行采集与聚合。");

        // 5.3 这步实现了对中间 checkpoint 与验证集指标的检查说明
        progress.Report("5.3 建议定期在验证集上评估微调效果，并保存若干中间 checkpoint 便于回滚与对比。");

        // 5.4 这步实现了对训练命令行的总结，方便在实际环境中复制执行
        progress.Report($"5.4 训练命令行示例: {commandLine}");

        return Task.CompletedTask;
    }

    private Task DescribeArtifactsAsync(LoraFinetuneOptions options, IProgress<string> progress, CancellationToken cancellationToken)
    {
        // 6.1 这步实现了微调后 LoRA 适配器权重输出位置的说明
        progress.Report($"6.1 建议将 LoRA 适配器权重保存在输出目录 {options.OutputDirectory}/lora_adapter 中。");

        // 6.2 这步实现了 tokenizer 与配置文件的导出建议，便于部署时加载
        progress.Report("6.2 建议同时导出 tokenizer 配置与模型配置 JSON，以便部署时一并加载。");

        // 6.3 这步实现了训练日志与指标文件归档的建议，便于后续合规与性能审计
        progress.Report("6.3 建议将训练日志与评估指标文件归档到持久化存储，用于后续审计与对比实验。");

        // 6.4 这步实现了对最终模型卡（model card）编写的建议，用于记录数据与训练设定
        progress.Report("6.4 建议编写 model card，记录数据来源、训练超参数与已知限制，便于合规评审。");

        return Task.CompletedTask;
    }

    private Task DescribeIntegrationAsync(LoraFinetuneOptions options, IProgress<string> progress, CancellationToken cancellationToken)
    {
        // 7.1 这步实现了将训练过程指标暴露为 OTLP metrics 的建议
        progress.Report("7.1 建议在训练脚本中通过 OTLP 或 Prometheus 将关键指标暴露给 Agent.Metering。");

        // 7.2 这步实现了通过 Agent.Metering 管道统一采集训练日志与运行时指标的说明
        progress.Report("7.2 通过 Agent.Metering 的 pipeline，将训练日志与 GPU/CPU 利用率等指标汇总到统一观测后端。");

        // 7.3 这步实现了将微调模型元数据写入配置中心或模型登记系统的建议
        progress.Report("7.3 建议在微调完成后，将模型版本、时间戳与校验信息写入模型登记系统。");

        // 7.4 这步实现了与推理服务集成的高层建议，例如通过配置切换新模型
        progress.Report("7.4 建议在推理服务中通过配置或特性标记的方式平滑切换到新微调模型，并保留回退路径。");

        return Task.CompletedTask;
    }

    private static string EscapeShell(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        if (value.Contains(' '))
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        return value;
    }
}
