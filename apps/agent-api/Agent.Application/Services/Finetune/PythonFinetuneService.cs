using Python.Runtime;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Agent.Core.Data.Entities;
using Agent.Core.Data.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace Agent.Application.Services.Finetune;

/// <summary>
/// Python.NET finetune service implementation
/// Python.NET微调服务实现
/// 
/// 使用Python.NET与AI-Agent项目中的微调脚本进行交互
/// Uses Python.NET to interact with fine-tuning scripts in the AI-Agent project
/// </summary>
public class PythonFinetuneService : IPythonFinetuneService, IDisposable
{
    private readonly IRepository<FinetuneRecordEntity, string> _repository;
    private readonly ILogger<PythonFinetuneService> _logger;
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, Process> _runningProcesses = new();
    private readonly object _lockObject = new();
    private bool _pythonInitialized = false;

    public PythonFinetuneService(
        IRepository<FinetuneRecordEntity, string> repository,
        ILogger<PythonFinetuneService> logger,
        IDistributedCache cache,
        IConfiguration configuration)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Initialize Python.NET environment
    /// 初始化Python.NET环境
    /// </summary>
    private async Task InitializePythonAsync()
    {
        if (_pythonInitialized) return;

        try
        {
            // 设置Python路径 - Set Python path
            var pythonPath = _configuration["Python:ExecutablePath"] ?? "python";
            var pythonHome = _configuration["Python:Home"];
            
            if (!string.IsNullOrEmpty(pythonHome))
            {
                PythonEngine.PythonHome = pythonHome;
            }

            // 初始化Python引擎 - Initialize Python engine
            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                _logger.LogInformation("Python.NET engine initialized successfully - Python.NET引擎初始化成功");
            }

            _pythonInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Python.NET - Python.NET初始化失败");
            throw new InvalidOperationException("Python.NET initialization failed", ex);
        }
    }

    /// <summary>
    /// Start a new fine-tuning job
    /// 启动新的微调任务
    /// </summary>
    public async Task<string> StartFinetuningAsync(FinetuneRequest request)
    {
        await InitializePythonAsync();

        var jobId = Guid.NewGuid().ToString();
        
        try
        {
            _logger.LogInformation("Starting fine-tuning job: {JobName} - 启动微调任务: {JobName}", request.JobName);

            // 创建微调记录 - Create finetune record
            var record = new FinetuneRecordEntity
            {
                Id = jobId,
                JobName = request.JobName,
                BaseModel = request.BaseModel,
                DatasetPath = request.DatasetPath,
                ConfigJson = JsonSerializer.Serialize(request),
                Status = FinetuneStatus.Queued,
                TotalEpochs = request.Epochs,
                LearningRate = request.LearningRate,
                BatchSize = request.BatchSize,
                OutputPath = request.OutputDir,
                GpuDevices = request.GpuDevices,
                CreatedBy = request.CreatedBy,
                Tags = string.Join(",", request.Tags ?? new List<string>()),
                Priority = request.Priority,
                IsCancellable = true,
                SaveCheckpoints = request.SaveCheckpoints,
                CheckpointInterval = request.SaveSteps
            };

            await _repository.AddAsync(record);
            _logger.LogInformation("Finetune record created with ID: {JobId} - 创建微调记录，ID: {JobId}", jobId);

            // 异步启动Python微调脚本 - Start Python fine-tuning script asynchronously
            _ = Task.Run(async () => await ExecuteFinetuningAsync(jobId, request));

            return jobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start fine-tuning job: {JobName} - 启动微调任务失败: {JobName}", request.JobName);
            
            // 更新记录状态为失败 - Update record status to failed
            var record = await _repository.GetByIdAsync(jobId);
            if (record != null)
            {
                record.Status = FinetuneStatus.Failed;
                record.ErrorMessage = ex.Message;
                await _repository.UpdateAsync(record);
            }
            
            throw;
        }
    }

    /// <summary>
    /// Execute fine-tuning using Python script
    /// 使用Python脚本执行微调
    /// </summary>
    private async Task ExecuteFinetuningAsync(string jobId, FinetuneRequest request)
    {
        Process? process = null;
        
        try
        {
            // 更新状态为运行中 - Update status to running
            var record = await _repository.GetByIdAsync(jobId);
            if (record == null) return;

            record.Status = FinetuneStatus.Running;
            record.StartedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(record);

            // 构建Python脚本命令 - Build Python script command
            var scriptPath = Path.Combine(
                _configuration["AI-Agent:ProjectPath"] ?? "/home/ubuntu/ai-agent",
                "finetune", "examples", "simple_finetune.py"
            );

            var arguments = BuildPythonArguments(request);
            
            // 启动Python进程 - Start Python process
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _configuration["Python:ExecutablePath"] ?? "python",
                Arguments = $"\"{scriptPath}\" {arguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath)
            };

            // 设置环境变量 - Set environment variables
            if (!string.IsNullOrEmpty(request.GpuDevices))
            {
                processStartInfo.EnvironmentVariables["CUDA_VISIBLE_DEVICES"] = request.GpuDevices;
            }

            process = new Process { StartInfo = processStartInfo };
            
            // 注册进程 - Register process
            lock (_lockObject)
            {
                _runningProcesses[jobId] = process;
            }

            // 设置输出处理 - Set up output handling
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += async (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                    await ProcessTrainingOutput(jobId, e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            // 启动进程 - Start process
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            _logger.LogInformation("Python fine-tuning process started for job: {JobId} - Python微调进程已启动，任务: {JobId}", jobId);

            // 等待进程完成 - Wait for process completion
            await process.WaitForExitAsync();

            // 更新最终状态 - Update final status
            record = await _repository.GetByIdAsync(jobId);
            if (record != null)
            {
                record.CompletedAt = DateTime.UtcNow;
                record.TotalTrainingTime = (int)(record.CompletedAt.Value - record.StartedAt!.Value).TotalSeconds;
                record.Logs = outputBuilder.ToString();
                
                if (process.ExitCode == 0)
                {
                    record.Status = FinetuneStatus.Completed;
                    record.Progress = 100;
                    _logger.LogInformation("Fine-tuning job completed successfully: {JobId} - 微调任务成功完成: {JobId}", jobId);
                }
                else
                {
                    record.Status = FinetuneStatus.Failed;
                    record.ErrorMessage = errorBuilder.ToString();
                    _logger.LogError("Fine-tuning job failed: {JobId}, Exit code: {ExitCode} - 微调任务失败: {JobId}, 退出代码: {ExitCode}", 
                        jobId, process.ExitCode);
                }

                await _repository.UpdateAsync(record);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during fine-tuning execution: {JobId} - 微调执行过程中出错: {JobId}", jobId);
            
            // 更新错误状态 - Update error status
            var record = await _repository.GetByIdAsync(jobId);
            if (record != null)
            {
                record.Status = FinetuneStatus.Failed;
                record.ErrorMessage = ex.Message;
                record.CompletedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(record);
            }
        }
        finally
        {
            // 清理进程 - Clean up process
            lock (_lockObject)
            {
                _runningProcesses.Remove(jobId);
            }
            
            process?.Dispose();
        }
    }

    /// <summary>
    /// Build Python script arguments
    /// 构建Python脚本参数
    /// </summary>
    private string BuildPythonArguments(FinetuneRequest request)
    {
        var args = new List<string>
        {
            $"--model_name {request.BaseModel}",
            $"--dataset_path \"{request.DatasetPath}\"",
            $"--output_dir \"{request.OutputDir}\"",
            $"--num_train_epochs {request.Epochs}",
            $"--learning_rate {request.LearningRate}",
            $"--per_device_train_batch_size {request.BatchSize}",
            $"--max_length {request.MaxLength}",
            $"--save_steps {request.SaveSteps}",
            $"--eval_steps {request.EvalSteps}",
            $"--logging_steps {request.LoggingSteps}",
            $"--warmup_steps {request.WarmupSteps}",
            $"--weight_decay {request.WeightDecay}"
        };

        if (request.SaveCheckpoints)
        {
            args.Add("--save_strategy steps");
        }

        return string.Join(" ", args);
    }

    /// <summary>
    /// Process training output for progress tracking
    /// 处理训练输出以跟踪进度
    /// </summary>
    private async Task ProcessTrainingOutput(string jobId, string output)
    {
        try
        {
            // 解析训练进度 - Parse training progress
            var epochMatch = Regex.Match(output, @"Epoch (\d+)/(\d+)");
            var lossMatch = Regex.Match(output, @"loss: ([\d.]+)");
            var progressMatch = Regex.Match(output, @"(\d+)%");

            if (epochMatch.Success || lossMatch.Success || progressMatch.Success)
            {
                var record = await _repository.GetByIdAsync(jobId);
                if (record != null)
                {
                    if (epochMatch.Success)
                    {
                        record.CurrentEpoch = int.Parse(epochMatch.Groups[1].Value);
                        record.TotalEpochs = int.Parse(epochMatch.Groups[2].Value);
                    }

                    if (lossMatch.Success)
                    {
                        var loss = double.Parse(lossMatch.Groups[1].Value);
                        record.CurrentLoss = loss;
                        
                        if (record.BestLoss == null || loss < record.BestLoss)
                        {
                            record.BestLoss = loss;
                        }
                    }

                    if (progressMatch.Success)
                    {
                        record.Progress = int.Parse(progressMatch.Groups[1].Value);
                    }

                    record.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(record);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse training output: {Output} - 解析训练输出失败: {Output}", output);
        }
    }

    /// <summary>
    /// Get fine-tuning job status
    /// 获取微调任务状态
    /// </summary>
    public async Task<FinetuneRecordEntity?> GetJobStatusAsync(string jobId)
    {
        try
        {
            return await _repository.GetByIdAsync(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get job status: {JobId} - 获取任务状态失败: {JobId}", jobId);
            return null;
        }
    }

    /// <summary>
    /// Cancel a running fine-tuning job
    /// 取消正在运行的微调任务
    /// </summary>
    public async Task<bool> CancelJobAsync(string jobId)
    {
        try
        {
            _logger.LogInformation("Cancelling fine-tuning job: {JobId} - 取消微调任务: {JobId}", jobId);

            // 终止进程 - Terminate process
            lock (_lockObject)
            {
                if (_runningProcesses.TryGetValue(jobId, out var process))
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(true); // Kill process tree
                        }
                        _runningProcesses.Remove(jobId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to kill process for job: {JobId} - 终止任务进程失败: {JobId}", jobId);
                    }
                }
            }

            // 更新数据库状态 - Update database status
            var record = await _repository.GetByIdAsync(jobId);
            if (record != null && record.Status.IsActive())
            {
                record.Status = FinetuneStatus.Cancelled;
                record.CompletedAt = DateTime.UtcNow;
                record.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(record);
                
                _logger.LogInformation("Fine-tuning job cancelled: {JobId} - 微调任务已取消: {JobId}", jobId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel job: {JobId} - 取消任务失败: {JobId}", jobId);
            return false;
        }
    }

    /// <summary>
    /// Get all fine-tuning jobs
    /// 获取所有微调任务
    /// </summary>
    public async Task<List<FinetuneRecordEntity>> GetJobsAsync(FinetuneStatus? status = null, int limit = 100)
    {
        try
        {
            var query = await _repository.GetAllAsync();
            
            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            return query.OrderByDescending(x => x.CreatedAt)
                       .Take(limit)
                       .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get jobs - 获取任务列表失败");
            return new List<FinetuneRecordEntity>();
        }
    }

    /// <summary>
    /// Get job logs
    /// 获取任务日志
    /// </summary>
    public async Task<string?> GetJobLogsAsync(string jobId)
    {
        try
        {
            var record = await _repository.GetByIdAsync(jobId);
            return record?.Logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get job logs: {JobId} - 获取任务日志失败: {JobId}", jobId);
            return null;
        }
    }

    /// <summary>
    /// Update job progress
    /// 更新任务进度
    /// </summary>
    public async Task<bool> UpdateJobProgressAsync(string jobId, FinetuneProgress progress)
    {
        try
        {
            var record = await _repository.GetByIdAsync(jobId);
            if (record == null) return false;

            record.CurrentEpoch = progress.CurrentEpoch;
            record.TotalEpochs = progress.TotalEpochs;
            record.Progress = progress.Progress;
            record.CurrentLoss = progress.CurrentLoss;
            record.BestLoss = progress.BestLoss;
            record.EstimatedTimeRemaining = progress.EstimatedTimeRemaining;
            record.MemoryUsageMb = progress.MemoryUsageMb;
            record.GpuMemoryUsageMb = progress.GpuMemoryUsageMb;
            record.UpdatedAt = DateTime.UtcNow;

            if (progress.Metrics != null)
            {
                record.MetricsJson = JsonSerializer.Serialize(progress.Metrics);
            }

            await _repository.UpdateAsync(record);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update job progress: {JobId} - 更新任务进度失败: {JobId}", jobId);
            return false;
        }
    }

    /// <summary>
    /// Validate Python environment and dependencies
    /// 验证Python环境和依赖项
    /// </summary>
    public async Task<PythonEnvironmentInfo> ValidatePythonEnvironmentAsync()
    {
        var info = new PythonEnvironmentInfo();
        
        try
        {
            await InitializePythonAsync();

            using (Py.GIL())
            {
                // 检查Python版本 - Check Python version
                dynamic sys = Py.Import("sys");
                info.PythonVersion = sys.version.ToString();
                info.PythonPath = sys.executable.ToString();

                // 检查必需的包 - Check required packages
                var requiredPackages = new[]
                {
                    "torch", "transformers", "datasets", "accelerate", 
                    "peft", "bitsandbytes", "numpy", "pandas"
                };

                foreach (var package in requiredPackages)
                {
                    try
                    {
                        dynamic pkg = Py.Import(package);
                        var version = pkg.__version__?.ToString() ?? "unknown";
                        info.InstalledPackages[package] = version;
                    }
                    catch
                    {
                        info.MissingPackages.Add(package);
                    }
                }

                // 检查GPU可用性 - Check GPU availability
                try
                {
                    dynamic torch = Py.Import("torch");
                    if (torch.cuda.is_available())
                    {
                        int deviceCount = torch.cuda.device_count();
                        for (int i = 0; i < deviceCount; i++)
                        {
                            var gpuInfo = new GpuInfo
                            {
                                DeviceId = i,
                                Name = torch.cuda.get_device_name(i).ToString(),
                                TotalMemoryMb = (long)(torch.cuda.get_device_properties(i).total_memory / (1024 * 1024))
                            };
                            info.AvailableGpus.Add(gpuInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    info.ErrorMessages.Add($"GPU检查失败: {ex.Message}");
                }

                info.IsValid = info.MissingPackages.Count == 0;
            }
        }
        catch (Exception ex)
        {
            info.IsValid = false;
            info.ErrorMessages.Add($"Python环境验证失败: {ex.Message}");
            _logger.LogError(ex, "Python environment validation failed - Python环境验证失败");
        }

        return info;
    }

    /// <summary>
    /// Get available models for fine-tuning
    /// 获取可用于微调的模型
    /// </summary>
    public async Task<List<string>> GetAvailableModelsAsync()
    {
        // 从缓存获取 - Get from cache
        var cachedModelsJson = await _cache.GetStringAsync("available_models");
        if (!string.IsNullOrEmpty(cachedModelsJson))
        {
            return JsonSerializer.Deserialize<List<string>>(cachedModelsJson) ?? new List<string>();
        }

        var models = new List<string>
        {
            "meta-llama/Llama-3.1-8B-Instruct",
            "meta-llama/Llama-2-7b-chat-hf", 
            "meta-llama/Llama-2-13b-chat-hf",
            "microsoft/DialoGPT-medium",
            "microsoft/DialoGPT-large",
            "facebook/blenderbot-400M-distill",
            "facebook/blenderbot-1B-distill"
        };

        // 缓存1小时 - Cache for 1 hour
        await _cache.SetStringAsync("available_models", JsonSerializer.Serialize(models), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
        
        return models;
    }

    /// <summary>
    /// Estimate training time and resources
    /// 估算训练时间和资源
    /// </summary>
    public async Task<ResourceEstimation> EstimateResourcesAsync(FinetuneRequest request)
    {
        var estimation = new ResourceEstimation();

        try
        {
            // 基于模型大小和数据集估算 - Estimate based on model size and dataset
            var modelSizeGb = GetModelSizeEstimate(request.BaseModel);
            var datasetSizeMb = await GetDatasetSizeEstimate(request.DatasetPath);

            // 估算GPU内存需求 - Estimate GPU memory requirements
            estimation.RequiredGpuMemoryMb = (long)(modelSizeGb * 1024 * 4); // 4x model size for training
            estimation.RequiredSystemMemoryMb = Math.Max(8192, datasetSizeMb * 2); // At least 8GB
            estimation.RequiredDiskSpaceMb = (long)(modelSizeGb * 1024 * 2 + datasetSizeMb * 3); // 2x model + 3x dataset

            // 估算训练时间 - Estimate training time
            var samplesPerEpoch = Math.Max(1000, datasetSizeMb / 10); // Rough estimate
            var stepsPerEpoch = samplesPerEpoch / request.BatchSize;
            var totalSteps = stepsPerEpoch * request.Epochs;
            var secondsPerStep = GetSecondsPerStepEstimate(request.BaseModel);
            
            estimation.EstimatedTrainingHours = (totalSteps * secondsPerStep) / 3600.0;

            // GPU推荐 - GPU recommendations
            estimation.RecommendedGpuCount = estimation.RequiredGpuMemoryMb > 24576 ? 2 : 1; // >24GB needs 2 GPUs

            // 检查资源充足性 - Check resource sufficiency
            var envInfo = await ValidatePythonEnvironmentAsync();
            var availableGpuMemory = envInfo.AvailableGpus.Sum(g => g.TotalMemoryMb);
            
            estimation.ResourcesSufficient = availableGpuMemory >= estimation.RequiredGpuMemoryMb;

            if (!estimation.ResourcesSufficient)
            {
                estimation.Warnings.Add($"GPU内存不足: 需要 {estimation.RequiredGpuMemoryMb}MB, 可用 {availableGpuMemory}MB");
                estimation.Suggestions.Add("考虑减少批次大小或使用梯度累积");
                estimation.Suggestions.Add("使用LoRA或QLoRA进行参数高效微调");
            }

            if (estimation.EstimatedTrainingHours > 24)
            {
                estimation.Warnings.Add($"训练时间较长: 预计 {estimation.EstimatedTrainingHours:F1} 小时");
                estimation.Suggestions.Add("考虑减少训练轮次或增加批次大小");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to estimate resources - 资源估算失败");
            estimation.Warnings.Add($"资源估算失败: {ex.Message}");
        }

        return estimation;
    }

    /// <summary>
    /// Get model size estimate in GB
    /// 获取模型大小估算（GB）
    /// </summary>
    private double GetModelSizeEstimate(string modelName)
    {
        return modelName.ToLower() switch
        {
            var name when name.Contains("4b") => 8.0,
            var name when name.Contains("7b") => 14.0,
            var name when name.Contains("14b") => 28.0,
            var name when name.Contains("400m") => 1.6,
            var name when name.Contains("1b") => 4.0,
            var name when name.Contains("medium") => 2.0,
            var name when name.Contains("large") => 6.0,
            _ => 8.0 // Default estimate
        };
    }

    /// <summary>
    /// Get dataset size estimate in MB
    /// 获取数据集大小估算（MB）
    /// </summary>
    private async Task<long> GetDatasetSizeEstimate(string datasetPath)
    {
        try
        {
            if (File.Exists(datasetPath))
            {
                var fileInfo = new FileInfo(datasetPath);
                return fileInfo.Length / (1024 * 1024);
            }
            else if (Directory.Exists(datasetPath))
            {
                var dirInfo = new DirectoryInfo(datasetPath);
                var totalSize = dirInfo.GetFiles("*", SearchOption.AllDirectories)
                                      .Sum(f => f.Length);
                return totalSize / (1024 * 1024);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to estimate dataset size: {DatasetPath} - 数据集大小估算失败: {DatasetPath}", datasetPath);
        }

        return 100; // Default 100MB estimate
    }

    /// <summary>
    /// Get seconds per training step estimate
    /// 获取每训练步骤的秒数估算
    /// </summary>
    private double GetSecondsPerStepEstimate(string modelName)
    {
        return modelName.ToLower() switch
        {
            var name when name.Contains("4b") => 0.5,
            var name when name.Contains("7b") => 1.0,
            var name when name.Contains("14b") => 2.0,
            var name when name.Contains("400m") => 0.1,
            var name when name.Contains("1b") => 0.2,
            _ => 0.5 // Default estimate
        };
    }

    /// <summary>
    /// Dispose resources
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        // 终止所有运行中的进程 - Terminate all running processes
        lock (_lockObject)
        {
            foreach (var process in _runningProcesses.Values)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(true);
                    }
                    process.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to dispose process - 进程释放失败");
                }
            }
            _runningProcesses.Clear();
        }

        // 关闭Python引擎 - Shutdown Python engine
        if (_pythonInitialized && PythonEngine.IsInitialized)
        {
            try
            {
                PythonEngine.Shutdown();
                _logger.LogInformation("Python.NET engine shutdown - Python.NET引擎已关闭");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to shutdown Python.NET engine - Python.NET引擎关闭失败");
            }
        }
    }
}

