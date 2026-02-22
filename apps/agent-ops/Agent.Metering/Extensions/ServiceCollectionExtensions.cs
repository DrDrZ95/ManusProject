
namespace Agent.Metering.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEbpfMetering(this IServiceCollection services)
    {
        services.AddSingleton<IEbpfService, EbpfService>();
        services.AddSingleton<IeBPFDetectiveService, eBPFDetectiveService>();
        return services;
    }

    public static void AddCoreServices(this IServiceCollection services)
    {
        // 注册插件组件注册表，用于根据名称解析 Receiver/Processor/Exporter
        services.AddSingleton<IMeteringComponentRegistry, MeteringComponentRegistry>();

        // 注册内置日志采集与处理插件
        // File tail receiver：从文件系统读取日志行（可用于容器/Docker 日志）
        services.AddSingleton<IMeteringReceiver>(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<FileTailReceiverOptions>>();
            var logger = sp.GetRequiredService<ILogger<FileTailReceiver>>();
            return new FileTailReceiver("file-tail", optionsMonitor, logger);
        });

        // Dapr Prometheus metrics receiver：抓取 Dapr sidecar 的 /metrics 端点
        services.AddSingleton<IMeteringReceiver>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DaprPrometheusReceiver>>();
            var options = new DaprPrometheusOptions();
            return new DaprPrometheusReceiver("dapr-prometheus", options, logger);
        });

        // 多行聚合处理器
        services.AddSingleton<IMeteringProcessor>(sp =>
        {
            var options = new LogMultilineOptions
            {
                Name = "log-multiline"
            };
            return new LogMultilineProcessor("log-multiline", options);
        });

        // 字段提取与脱敏处理器
        services.AddSingleton<IMeteringProcessor>(sp =>
        {
            var options = new LogTransformOptions
            {
                Name = "log-transform"
            };
            return new LogTransformProcessor("log-transform", options);
        });

        // Kubernetes metadata enricher
        services.AddSingleton<IMeteringProcessor>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<KubernetesMetadataProcessor>>();
            var options = new KubernetesMetadataOptions
            {
                PodNameEnv = "POD_NAME",
                NamespaceEnv = "POD_NAMESPACE",
                NodeNameEnv = "NODE_NAME"
            };
            return new KubernetesMetadataProcessor("k8s-metadata", options, logger);
        });

        // OTLP 风格日志导出器（依赖外部 OTLP 日志管道配置）
        services.AddSingleton<IMeteringExporter>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<OtlpLogExporter>>();
            var options = new OtlpLogExporterOptions
            {
                Name = "otlp-logs"
            };
            return new OtlpLogExporter("otlp-logs", options, logger);
        });

        // OTLP/HTTP 通用导出器（可用于对接 OTLP/HTTP Collector 或 Datadog OTLP ingest）
        services.AddSingleton<IMeteringExporter>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<OtlpHttpExporter>>();
            var options = new OtlpExporterOptions
            {
                Protocol = OtlpExporterProtocol.Http,
                Endpoint = "http://127.0.0.1:4318/v1/metrics"
            };
            return new OtlpHttpExporter("otlp-http", options, logger);
        });

        // 本地调试日志导出器
        services.AddSingleton<IMeteringExporter>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DebugFileLogExporter>>();
            var options = new DebugFileLogExporterOptions
            {
                Name = "file-debug"
            };
            return new DebugFileLogExporter("file-debug", options, logger);
        });

        // 注册通用 LoRA 微调流程运行器（用于在控制平面触发与追踪微调流程）
        services.AddSingleton<LoraFinetuneProcessRunner>();

        // 注册流水线宿主服务与健康/场景管理接口
        services.AddSingleton<MeteringPipelineHostedService>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<MeteringPipelineHostedService>());
        services.AddSingleton<IMeteringHealthSnapshotProvider>(sp => sp.GetRequiredService<MeteringPipelineHostedService>());
        services.AddSingleton<IScenarioManager>(sp => sp.GetRequiredService<MeteringPipelineHostedService>());
    }
}
