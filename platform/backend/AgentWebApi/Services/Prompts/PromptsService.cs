using System.Text.Json;
using System.Text.RegularExpressions;

namespace AgentWebApi.Services.Prompts;

/// <summary>
/// Professional prompt management service implementation
/// 专业提示词管理服务实现
/// 
/// 提供AI-Agent系统中各种专业工具的提示词模板管理
/// Provides prompt template management for various professional tools in AI-Agent system
/// </summary>
public class PromptsService : IPromptsService
{
    private readonly ILogger<PromptsService> _logger;
    private readonly Dictionary<string, Dictionary<string, PromptTemplate>> _prompts;
    private readonly Dictionary<string, List<ToolExample>> _toolExamples;

    public PromptsService(ILogger<PromptsService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _prompts = new Dictionary<string, Dictionary<string, PromptTemplate>>();
        _toolExamples = new Dictionary<string, List<ToolExample>>();
        
        // 初始化内置提示词和工具示例 - Initialize built-in prompts and tool examples
        InitializeBuiltInPrompts();
        InitializeToolExamples();
    }

    /// <summary>
    /// Get prompt template by category and name
    /// 根据类别和名称获取提示词模板
    /// </summary>
    public async Task<PromptTemplate?> GetPromptAsync(string category, string name)
    {
        try
        {
            _logger.LogDebug("Getting prompt: {Category}/{Name}", category, name);
            
            if (_prompts.TryGetValue(category, out var categoryPrompts) &&
                categoryPrompts.TryGetValue(name, out var prompt))
            {
                return await Task.FromResult(prompt);
            }

            _logger.LogWarning("Prompt not found: {Category}/{Name}", category, name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt: {Category}/{Name}", category, name);
            throw;
        }
    }

    /// <summary>
    /// Get all prompts in a category
    /// 获取某个类别下的所有提示词
    /// </summary>
    public async Task<List<PromptTemplate>> GetPromptsByCategoryAsync(string category)
    {
        try
        {
            _logger.LogDebug("Getting prompts for category: {Category}", category);
            
            if (_prompts.TryGetValue(category, out var categoryPrompts))
            {
                return await Task.FromResult(categoryPrompts.Values.ToList());
            }

            return new List<PromptTemplate>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompts for category: {Category}", category);
            throw;
        }
    }

    /// <summary>
    /// Get all available categories
    /// 获取所有可用的类别
    /// </summary>
    public async Task<List<string>> GetCategoriesAsync()
    {
        try
        {
            return await Task.FromResult(_prompts.Keys.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            throw;
        }
    }

    /// <summary>
    /// Render prompt with variables
    /// 使用变量渲染提示词
    /// </summary>
    public string RenderPrompt(PromptTemplate template, Dictionary<string, object> variables)
    {
        try
        {
            _logger.LogDebug("Rendering prompt: {PromptName}", template.Name);
            
            var rendered = template.Template;
            
            // 替换模板变量 - Replace template variables
            foreach (var variable in variables)
            {
                var placeholder = $"{{{variable.Key}}}";
                var value = variable.Value?.ToString() ?? string.Empty;
                rendered = rendered.Replace(placeholder, value);
            }

            // 检查是否有未替换的变量 - Check for unreplaced variables
            var unreplacedMatches = Regex.Matches(rendered, @"\{([^}]+)\}");
            if (unreplacedMatches.Count > 0)
            {
                var unreplacedVars = unreplacedMatches.Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .Distinct()
                    .ToList();
                
                _logger.LogWarning("Unreplaced variables in prompt {PromptName}: {Variables}", 
                    template.Name, string.Join(", ", unreplacedVars));
            }

            return rendered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering prompt: {PromptName}", template.Name);
            throw;
        }
    }

    /// <summary>
    /// Add or update prompt template
    /// 添加或更新提示词模板
    /// </summary>
    public async Task<bool> SavePromptAsync(PromptTemplate template)
    {
        try
        {
            _logger.LogDebug("Saving prompt: {Category}/{Name}", template.Category, template.Name);
            
            if (!_prompts.ContainsKey(template.Category))
            {
                _prompts[template.Category] = new Dictionary<string, PromptTemplate>();
            }

            template.UpdatedAt = DateTime.UtcNow;
            _prompts[template.Category][template.Name] = template;
            
            _logger.LogInformation("Successfully saved prompt: {Category}/{Name}", template.Category, template.Name);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving prompt: {Category}/{Name}", template.Category, template.Name);
            return false;
        }
    }

    /// <summary>
    /// Delete prompt template
    /// 删除提示词模板
    /// </summary>
    public async Task<bool> DeletePromptAsync(string category, string name)
    {
        try
        {
            _logger.LogDebug("Deleting prompt: {Category}/{Name}", category, name);
            
            if (_prompts.TryGetValue(category, out var categoryPrompts) &&
                categoryPrompts.Remove(name))
            {
                _logger.LogInformation("Successfully deleted prompt: {Category}/{Name}", category, name);
                return await Task.FromResult(true);
            }

            _logger.LogWarning("Prompt not found for deletion: {Category}/{Name}", category, name);
            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prompt: {Category}/{Name}", category, name);
            return false;
        }
    }

    /// <summary>
    /// Search prompts by keywords
    /// 根据关键词搜索提示词
    /// </summary>
    public async Task<List<PromptTemplate>> SearchPromptsAsync(string keywords)
    {
        try
        {
            _logger.LogDebug("Searching prompts with keywords: {Keywords}", keywords);
            
            var results = new List<PromptTemplate>();
            var searchTerms = keywords.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var categoryPrompts in _prompts.Values)
            {
                foreach (var prompt in categoryPrompts.Values)
                {
                    // 在标题、描述、标签中搜索 - Search in title, description, tags
                    var searchText = $"{prompt.Title} {prompt.Description} {string.Join(" ", prompt.Tags)}".ToLowerInvariant();
                    
                    if (searchTerms.Any(term => searchText.Contains(term)))
                    {
                        results.Add(prompt);
                    }
                }
            }

            _logger.LogDebug("Found {Count} prompts matching keywords: {Keywords}", results.Count, keywords);
            return await Task.FromResult(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching prompts with keywords: {Keywords}", keywords);
            throw;
        }
    }

    /// <summary>
    /// Get professional tool examples
    /// 获取专业工具示例
    /// </summary>
    public async Task<List<ToolExample>> GetToolExamplesAsync(string toolType)
    {
        try
        {
            _logger.LogDebug("Getting tool examples for type: {ToolType}", toolType);
            
            if (_toolExamples.TryGetValue(toolType, out var examples))
            {
                return await Task.FromResult(examples);
            }

            return new List<ToolExample>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tool examples for type: {ToolType}", toolType);
            throw;
        }
    }

    /// <summary>
    /// Get all available tool types
    /// 获取所有可用的工具类型
    /// </summary>
    public async Task<List<string>> GetToolTypesAsync()
    {
        try
        {
            return await Task.FromResult(_toolExamples.Keys.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tool types");
            throw;
        }
    }

    /// <summary>
    /// Initialize built-in prompt templates
    /// 初始化内置提示词模板
    /// </summary>
    private void InitializeBuiltInPrompts()
    {
        _logger.LogInformation("Initializing built-in prompt templates");

        // RAG相关提示词 - RAG related prompts
        var ragPrompts = new Dictionary<string, PromptTemplate>
        {
            ["document_analysis"] = new PromptTemplate
            {
                Id = "rag_document_analysis",
                Category = "rag",
                Name = "document_analysis",
                Title = "Document Analysis and Summarization",
                Description = "Analyze and summarize documents for RAG knowledge base",
                Template = """
                Please analyze the following document and provide a comprehensive summary:

                Document Title: {title}
                Document Type: {document_type}
                Content:
                {content}

                Please provide:
                1. A concise summary (2-3 sentences)
                2. Key topics and themes
                3. Important facts and figures
                4. Relevant keywords for search
                5. Potential questions this document could answer

                Format your response in a structured way that would be useful for a knowledge base.
                """,
                Variables = new List<PromptVariable>
                {
                    new() { Name = "title", Description = "Document title", Type = "string", Required = true },
                    new() { Name = "document_type", Description = "Type of document", Type = "string", DefaultValue = "general" },
                    new() { Name = "content", Description = "Document content", Type = "string", Required = true }
                },
                Tags = new List<string> { "rag", "analysis", "summarization" },
                Author = "AI-Agent"
            },

            ["query_enhancement"] = new PromptTemplate
            {
                Id = "rag_query_enhancement",
                Category = "rag",
                Name = "query_enhancement",
                Title = "Query Enhancement for Better Retrieval",
                Description = "Enhance user queries for better document retrieval",
                Template = """
                Original user query: {original_query}
                Context: {context}

                Please enhance this query to improve document retrieval by:
                1. Adding relevant synonyms and related terms
                2. Expanding abbreviations and technical terms
                3. Including alternative phrasings
                4. Suggesting related concepts

                Enhanced query should be optimized for semantic search while maintaining the original intent.
                Provide the enhanced query and explain the improvements made.
                """,
                Variables = new List<PromptVariable>
                {
                    new() { Name = "original_query", Description = "User's original query", Type = "string", Required = true },
                    new() { Name = "context", Description = "Additional context", Type = "string", DefaultValue = "" }
                },
                Tags = new List<string> { "rag", "query", "enhancement", "retrieval" },
                Author = "AI-Agent"
            }
        };

        // 工作流相关提示词 - Workflow related prompts
        var workflowPrompts = new Dictionary<string, PromptTemplate>
        {
            ["task_breakdown"] = new PromptTemplate
            {
                Id = "workflow_task_breakdown",
                Category = "workflow",
                Name = "task_breakdown",
                Title = "Task Breakdown and Planning",
                Description = "Break down complex tasks into manageable steps",
                Template = """
                Project: {project_name}
                Objective: {objective}
                Timeline: {timeline}
                Resources: {resources}

                Please break down this project into detailed, actionable steps:

                1. Analyze the objective and identify major phases
                2. Break each phase into specific tasks
                3. Estimate time requirements for each task
                4. Identify dependencies between tasks
                5. Suggest appropriate task types (e.g., [RESEARCH], [DESIGN], [CODE], [TEST])

                Format the output as a structured workflow plan that can be used by the AI-Agent system.
                """,
                Variables = new List<PromptVariable>
                {
                    new() { Name = "project_name", Description = "Name of the project", Type = "string", Required = true },
                    new() { Name = "objective", Description = "Project objective", Type = "string", Required = true },
                    new() { Name = "timeline", Description = "Project timeline", Type = "string", DefaultValue = "flexible" },
                    new() { Name = "resources", Description = "Available resources", Type = "string", DefaultValue = "standard" }
                },
                Tags = new List<string> { "workflow", "planning", "task", "breakdown" },
                Author = "AI-Agent"
            }
        };

        // 代码生成相关提示词 - Code generation related prompts
        var codePrompts = new Dictionary<string, PromptTemplate>
        {
            ["api_documentation"] = new PromptTemplate
            {
                Id = "code_api_documentation",
                Category = "code",
                Name = "api_documentation",
                Title = "API Documentation Generation",
                Description = "Generate comprehensive API documentation",
                Template = """
                API Endpoint: {endpoint}
                Method: {method}
                Description: {description}
                Parameters: {parameters}

                Please generate comprehensive API documentation including:

                1. Endpoint description and purpose
                2. Request/response examples
                3. Parameter descriptions with types and validation rules
                4. Error codes and handling
                5. Usage examples in multiple programming languages
                6. Best practices and common pitfalls

                Format the documentation in a clear, professional manner suitable for developers.
                """,
                Variables = new List<PromptVariable>
                {
                    new() { Name = "endpoint", Description = "API endpoint path", Type = "string", Required = true },
                    new() { Name = "method", Description = "HTTP method", Type = "string", Required = true },
                    new() { Name = "description", Description = "Endpoint description", Type = "string", Required = true },
                    new() { Name = "parameters", Description = "Endpoint parameters", Type = "string", DefaultValue = "none" }
                },
                Tags = new List<string> { "code", "api", "documentation", "development" },
                Author = "AI-Agent"
            }
        };

        // 系统管理相关提示词 - System administration related prompts
        var systemPrompts = new Dictionary<string, PromptTemplate>
        {
            ["kubernetes_deployment"] = new PromptTemplate
            {
                Id = "system_kubernetes_deployment",
                Category = "system",
                Name = "kubernetes_deployment",
                Title = "Kubernetes Deployment Configuration",
                Description = "Generate Kubernetes deployment YAML configurations",
                Template = """
                Application: {app_name}
                Image: {image_name}
                Replicas: {replicas}
                Port: {port}
                Environment: {environment}

                Please generate a comprehensive Kubernetes deployment configuration including:

                1. Deployment manifest with proper resource limits
                2. Service configuration for load balancing
                3. ConfigMap for environment variables
                4. Ingress configuration if needed
                5. Health checks and readiness probes
                6. Security context and best practices

                Ensure the configuration follows Kubernetes best practices for production deployment.
                """,
                Variables = new List<PromptVariable>
                {
                    new() { Name = "app_name", Description = "Application name", Type = "string", Required = true },
                    new() { Name = "image_name", Description = "Docker image name", Type = "string", Required = true },
                    new() { Name = "replicas", Description = "Number of replicas", Type = "number", DefaultValue = 3 },
                    new() { Name = "port", Description = "Application port", Type = "number", DefaultValue = 8080 },
                    new() { Name = "environment", Description = "Deployment environment", Type = "string", DefaultValue = "production" }
                },
                Tags = new List<string> { "kubernetes", "deployment", "devops", "infrastructure" },
                Author = "AI-Agent"
            },

            ["monitoring_setup"] = new PromptTemplate
            {
                Id = "system_monitoring_setup",
                Category = "system",
                Name = "monitoring_setup",
                Title = "System Monitoring Configuration",
                Description = "Configure comprehensive system monitoring with Prometheus and Grafana",
                Template = """
                System: {system_type}
                Components: {components}
                Metrics: {metrics_required}
                Alerting: {alerting_rules}

                Please provide a complete monitoring setup including:

                1. Prometheus configuration for metrics collection
                2. Grafana dashboard definitions
                3. Alert manager rules and notifications
                4. Service discovery configuration
                5. Custom metrics and exporters
                6. Performance optimization recommendations

                Include both infrastructure and application-level monitoring.
                """,
                Variables = new List<PromptVariable>
                {
                    new() { Name = "system_type", Description = "Type of system to monitor", Type = "string", Required = true },
                    new() { Name = "components", Description = "System components", Type = "string", Required = true },
                    new() { Name = "metrics_required", Description = "Required metrics", Type = "string", DefaultValue = "standard" },
                    new() { Name = "alerting_rules", Description = "Alerting requirements", Type = "string", DefaultValue = "basic" }
                },
                Tags = new List<string> { "monitoring", "prometheus", "grafana", "alerting" },
                Author = "AI-Agent"
            }
        };

        // 安全相关提示词 - Security related prompts
        var securityPrompts = new Dictionary<string, PromptTemplate>
        {
            ["security_audit"] = new PromptTemplate
            {
                Id = "security_audit_checklist",
                Category = "security",
                Name = "security_audit",
                Title = "Comprehensive Security Audit Checklist",
                Description = "Generate security audit checklist for applications and infrastructure",
                Template = """
                Target System: {target_system}
                System Type: {system_type}
                Compliance Requirements: {compliance}
                Risk Level: {risk_level}

                Please create a comprehensive security audit checklist covering:

                1. Authentication and authorization mechanisms
                2. Data encryption (at rest and in transit)
                3. Network security and firewall configurations
                4. Input validation and injection prevention
                5. Access control and privilege management
                6. Logging and monitoring for security events
                7. Vulnerability assessment procedures
                8. Incident response planning

                Include specific tools and techniques for each audit area.
                """,
                Variables = new List<PromptVariable>
                {
                    new() { Name = "target_system", Description = "System to audit", Type = "string", Required = true },
                    new() { Name = "system_type", Description = "Type of system", Type = "string", Required = true },
                    new() { Name = "compliance", Description = "Compliance requirements", Type = "string", DefaultValue = "general" },
                    new() { Name = "risk_level", Description = "Risk assessment level", Type = "string", DefaultValue = "medium" }
                },
                Tags = new List<string> { "security", "audit", "compliance", "vulnerability" },
                Author = "AI-Agent"
            }
        };

        // 数据科学相关提示词 - Data science related prompts
        var dataSciencePrompts = new Dictionary<string, PromptTemplate>
        {
            ["ml_model_evaluation"] = new PromptTemplate
            {
                Id = "ds_ml_model_evaluation",
                Category = "data_science",
                Name = "ml_model_evaluation",
                Title = "Machine Learning Model Evaluation Framework",
                Description = "Comprehensive ML model evaluation and validation framework",
                Template = """
                Model Type: {model_type}
                Dataset: {dataset_info}
                Problem Type: {problem_type}
                Evaluation Metrics: {metrics}

                Please provide a comprehensive model evaluation framework including:

                1. Data preprocessing and feature engineering steps
                2. Model training and hyperparameter tuning
                3. Cross-validation strategy
                4. Performance metrics calculation and interpretation
                5. Model comparison and selection criteria
                6. Bias and fairness assessment
                7. Model interpretability analysis
                8. Production deployment considerations

                Include code examples and best practices for each step.
                """,
                Variables = new List<PromptVariable>
                {
                    new() { Name = "model_type", Description = "Type of ML model", Type = "string", Required = true },
                    new() { Name = "dataset_info", Description = "Dataset description", Type = "string", Required = true },
                    new() { Name = "problem_type", Description = "ML problem type", Type = "string", Required = true },
                    new() { Name = "metrics", Description = "Evaluation metrics", Type = "string", DefaultValue = "standard" }
                },
                Tags = new List<string> { "machine_learning", "evaluation", "validation", "metrics" },
                Author = "AI-Agent"
            }
        };

        // 添加到主字典 - Add to main dictionary
        _prompts["rag"] = ragPrompts;
        _prompts["workflow"] = workflowPrompts;
        _prompts["code"] = codePrompts;
        _prompts["system"] = systemPrompts;
        _prompts["security"] = securityPrompts;
        _prompts["data_science"] = dataSciencePrompts;

        _logger.LogInformation("Initialized {Count} prompt categories with {Total} total prompts", 
            _prompts.Count, _prompts.Values.Sum(p => p.Count));
    }

    /// <summary>
    /// Initialize professional tool examples
    /// 初始化专业工具示例
    /// </summary>
    private void InitializeToolExamples()
    {
        _logger.LogInformation("Initializing professional tool examples");

        // 数据分析工具示例 - Data analysis tool examples
        var dataAnalysisTools = new List<ToolExample>
        {
            new ToolExample
            {
                ToolType = "data_analysis",
                Name = "Pandas Data Processing",
                Description = "Advanced data manipulation and analysis using Python Pandas library",
                Example = """
                import pandas as pd
                import numpy as np
                
                # 加载和清理数据 - Load and clean data
                df = pd.read_csv('data.csv')
                df = df.dropna()  # 移除空值 - Remove null values
                df = df.drop_duplicates()  # 移除重复项 - Remove duplicates
                
                # 数据分析 - Data analysis
                summary = df.describe()  # 统计摘要 - Statistical summary
                correlation = df.corr()  # 相关性分析 - Correlation analysis
                
                # 数据可视化 - Data visualization
                import matplotlib.pyplot as plt
                df.hist(bins=20, figsize=(12, 8))
                plt.show()
                """,
                Parameters = new Dictionary<string, object>
                {
                    { "data_source", "CSV file path or database connection" },
                    { "analysis_type", "descriptive, correlation, regression" },
                    { "output_format", "charts, tables, reports" }
                },
                ExpectedOutput = "Statistical analysis results, visualizations, and insights",
                UseCases = new List<string>
                {
                    "Business intelligence reporting",
                    "Scientific research data analysis",
                    "Financial market analysis",
                    "Customer behavior analysis"
                },
                BestPractices = new List<string>
                {
                    "Always validate data quality before analysis",
                    "Use appropriate statistical methods for data types",
                    "Document analysis methodology and assumptions",
                    "Visualize data to identify patterns and outliers"
                }
            },

            new ToolExample
            {
                ToolType = "data_analysis",
                Name = "SQL Query Optimization",
                Description = "Advanced SQL techniques for efficient data querying and analysis",
                Example = """
                -- 复杂数据分析查询 - Complex data analysis query
                WITH monthly_sales AS (
                    SELECT 
                        DATE_TRUNC('month', order_date) as month,
                        SUM(total_amount) as total_sales,
                        COUNT(*) as order_count,
                        AVG(total_amount) as avg_order_value
                    FROM orders 
                    WHERE order_date >= '2023-01-01'
                    GROUP BY DATE_TRUNC('month', order_date)
                ),
                growth_analysis AS (
                    SELECT 
                        month,
                        total_sales,
                        LAG(total_sales) OVER (ORDER BY month) as prev_month_sales,
                        (total_sales - LAG(total_sales) OVER (ORDER BY month)) / 
                        LAG(total_sales) OVER (ORDER BY month) * 100 as growth_rate
                    FROM monthly_sales
                )
                SELECT * FROM growth_analysis ORDER BY month;
                """,
                Parameters = new Dictionary<string, object>
                {
                    { "database_type", "PostgreSQL, MySQL, SQL Server" },
                    { "query_complexity", "simple, intermediate, advanced" },
                    { "performance_requirements", "response time, data volume" }
                },
                ExpectedOutput = "Optimized query results with performance metrics",
                UseCases = new List<string>
                {
                    "Business reporting and analytics",
                    "Data warehouse operations",
                    "Real-time dashboard queries",
                    "ETL process optimization"
                },
                BestPractices = new List<string>
                {
                    "Use appropriate indexes for query optimization",
                    "Avoid SELECT * in production queries",
                    "Use CTEs for complex logic readability",
                    "Monitor query execution plans"
                }
            }
        };

        // 机器学习工具示例 - Machine learning tool examples
        var mlTools = new List<ToolExample>
        {
            new ToolExample
            {
                ToolType = "machine_learning",
                Name = "Scikit-learn Classification",
                Description = "Building and evaluating classification models using scikit-learn",
                Example = """
                from sklearn.model_selection import train_test_split, cross_val_score
                from sklearn.ensemble import RandomForestClassifier
                from sklearn.metrics import classification_report, confusion_matrix
                from sklearn.preprocessing import StandardScaler
                
                # 数据预处理 - Data preprocessing
                X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
                
                scaler = StandardScaler()
                X_train_scaled = scaler.fit_transform(X_train)
                X_test_scaled = scaler.transform(X_test)
                
                # 模型训练 - Model training
                rf_model = RandomForestClassifier(n_estimators=100, random_state=42)
                rf_model.fit(X_train_scaled, y_train)
                
                # 模型评估 - Model evaluation
                y_pred = rf_model.predict(X_test_scaled)
                print(classification_report(y_test, y_pred))
                
                # 交叉验证 - Cross validation
                cv_scores = cross_val_score(rf_model, X_train_scaled, y_train, cv=5)
                print(f"CV Score: {cv_scores.mean():.3f} (+/- {cv_scores.std() * 2:.3f})")
                """,
                Parameters = new Dictionary<string, object>
                {
                    { "algorithm_type", "classification, regression, clustering" },
                    { "data_size", "small (<1K), medium (1K-100K), large (>100K)" },
                    { "feature_count", "number of input features" },
                    { "evaluation_metrics", "accuracy, precision, recall, F1-score" }
                },
                ExpectedOutput = "Trained model with performance metrics and predictions",
                UseCases = new List<string>
                {
                    "Customer churn prediction",
                    "Fraud detection systems",
                    "Medical diagnosis assistance",
                    "Image classification tasks"
                },
                BestPractices = new List<string>
                {
                    "Always split data into train/validation/test sets",
                    "Use cross-validation for robust model evaluation",
                    "Scale features for distance-based algorithms",
                    "Monitor for overfitting and underfitting"
                }
            }
        };

        // 网络安全工具示例 - Cybersecurity tool examples
        var securityTools = new List<ToolExample>
        {
            new ToolExample
            {
                ToolType = "cybersecurity",
                Name = "Network Security Scanning",
                Description = "Automated network vulnerability assessment and security scanning",
                Example = """
                import nmap
                import socket
                from datetime import datetime
                
                # 网络扫描配置 - Network scanning configuration
                def network_scan(target_range, scan_type='basic'):
                    nm = nmap.PortScanner()
                    
                    # 基础端口扫描 - Basic port scanning
                    if scan_type == 'basic':
                        result = nm.scan(target_range, '22-443')
                    # 详细服务扫描 - Detailed service scanning
                    elif scan_type == 'detailed':
                        result = nm.scan(target_range, arguments='-sV -sC')
                    
                    # 分析扫描结果 - Analyze scan results
                    for host in nm.all_hosts():
                        print(f'Host: {host} ({nm[host].hostname()})')
                        print(f'State: {nm[host].state()}')
                        
                        for protocol in nm[host].all_protocols():
                            ports = nm[host][protocol].keys()
                            for port in ports:
                                state = nm[host][protocol][port]['state']
                                service = nm[host][protocol][port].get('name', 'unknown')
                                print(f'Port {port}/{protocol}: {state} ({service})')
                
                # 安全评估报告 - Security assessment report
                def generate_security_report(scan_results):
                    # 生成详细的安全评估报告 - Generate detailed security assessment report
                    pass
                """,
                Parameters = new Dictionary<string, object>
                {
                    { "target_range", "IP address range or specific hosts" },
                    { "scan_type", "basic, detailed, stealth, aggressive" },
                    { "port_range", "specific ports or port ranges to scan" },
                    { "output_format", "console, JSON, XML, HTML report" }
                },
                ExpectedOutput = "Network security assessment report with vulnerability findings",
                UseCases = new List<string>
                {
                    "Network infrastructure security auditing",
                    "Penetration testing and vulnerability assessment",
                    "Compliance security scanning",
                    "Incident response and forensics"
                },
                BestPractices = new List<string>
                {
                    "Always obtain proper authorization before scanning",
                    "Use rate limiting to avoid network disruption",
                    "Document all findings with severity levels",
                    "Follow responsible disclosure practices"
                }
            }
        };

        // DevOps工具示例 - DevOps tool examples
        var devopsTools = new List<ToolExample>
        {
            new ToolExample
            {
                ToolType = "devops",
                Name = "Docker Container Management",
                Description = "Advanced Docker containerization and orchestration techniques",
                Example = """
                # 多阶段构建Dockerfile - Multi-stage build Dockerfile
                FROM node:18-alpine AS builder
                WORKDIR /app
                COPY package*.json ./
                RUN npm ci --only=production
                
                FROM node:18-alpine AS runtime
                # 创建非root用户 - Create non-root user
                RUN addgroup -g 1001 -S nodejs && adduser -S nextjs -u 1001
                
                WORKDIR /app
                COPY --from=builder /app/node_modules ./node_modules
                COPY . .
                
                # 设置安全配置 - Set security configuration
                USER nextjs
                EXPOSE 3000
                
                # 健康检查 - Health check
                HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
                    CMD curl -f http://localhost:3000/health || exit 1
                
                CMD ["npm", "start"]
                
                # Docker Compose配置 - Docker Compose configuration
                version: '3.8'
                services:
                  app:
                    build: .
                    ports:
                      - "3000:3000"
                    environment:
                      - NODE_ENV=production
                    volumes:
                      - app_data:/app/data
                    networks:
                      - app_network
                    restart: unless-stopped
                
                volumes:
                  app_data:
                
                networks:
                  app_network:
                    driver: bridge
                """,
                Parameters = new Dictionary<string, object>
                {
                    { "base_image", "operating system and runtime version" },
                    { "application_type", "web app, API, microservice, database" },
                    { "security_requirements", "user permissions, network isolation" },
                    { "scaling_needs", "horizontal scaling, load balancing" }
                },
                ExpectedOutput = "Production-ready containerized application with proper configuration",
                UseCases = new List<string>
                {
                    "Microservices deployment and orchestration",
                    "CI/CD pipeline integration",
                    "Development environment standardization",
                    "Cloud-native application deployment"
                },
                BestPractices = new List<string>
                {
                    "Use multi-stage builds to reduce image size",
                    "Run containers as non-root users",
                    "Implement proper health checks",
                    "Use specific image tags instead of 'latest'"
                }
            }
        };

        // 添加到工具示例字典 - Add to tool examples dictionary
        _toolExamples["data_analysis"] = dataAnalysisTools;
        _toolExamples["machine_learning"] = mlTools;
        _toolExamples["cybersecurity"] = securityTools;
        _toolExamples["devops"] = devopsTools;

        _logger.LogInformation("Initialized {Count} tool categories with {Total} total examples", 
            _toolExamples.Count, _toolExamples.Values.Sum(t => t.Count));
    }
}

