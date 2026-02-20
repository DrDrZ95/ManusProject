
namespace Agent.Metering.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEbpfMetering(this IServiceCollection services)
    {
        services.AddSingleton<IEbpfService, EbpfService>();
        return services;
    }

    public static void AddCoreServices(this IServiceCollection services)
    {
        // 注册插件组件注册表，用于根据名称解析 Receiver/Processor/Exporter
        services.AddSingleton<IMeteringComponentRegistry, MeteringComponentRegistry>();

        // 注册内置日志采集与处理插件
        // File tail receiver：从文件系统读取日志行
        services.AddSingleton<IMeteringReceiver>(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<FileTailReceiverOptions>>();
            var logger = sp.GetRequiredService<ILogger<FileTailReceiver>>();
            return new FileTailReceiver("file-tail", optionsMonitor, logger);
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

        // 注册流水线宿主服务与健康/场景管理接口
        services.AddSingleton<MeteringPipelineHostedService>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<MeteringPipelineHostedService>());
        services.AddSingleton<IMeteringHealthSnapshotProvider>(sp => sp.GetRequiredService<MeteringPipelineHostedService>());
        services.AddSingleton<IScenarioManager>(sp => sp.GetRequiredService<MeteringPipelineHostedService>());
    }
}


