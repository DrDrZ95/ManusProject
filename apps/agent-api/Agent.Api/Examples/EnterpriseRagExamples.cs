namespace Agent.Api.Examples;

/// <summary>
/// Enterprise RAG scenario examples and use cases
/// 企业RAG场景示例和用例
/// </summary>
public class EnterpriseRagExamples
{
    private readonly IRagService _ragService;
    private readonly ILogger<EnterpriseRagExamples> _logger;

    public EnterpriseRagExamples(IRagService ragService, ILogger<EnterpriseRagExamples> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    /// <summary>
    /// Example: HR Policy Q&A System
    /// 示例：人力资源政策问答系统
    /// </summary>
    public async Task<RagResponse> HRPolicyQAExample(string question)
    {
        try
        {
            _logger.LogInformation("Processing HR policy question: {Question}", question);

            // 1. Create HR knowledge base if not exists - 如果不存在，创建HR知识库
            var hrKnowledgeBase = "hr_policies";

            try
            {
                await _ragService.CreateKnowledgeBaseAsync(hrKnowledgeBase, new RagCollectionConfig
                {
                    Description = "Human Resources policies and procedures",
                    ChunkSize = 800,
                    ChunkOverlap = 150,
                    EmbeddingModel = "text-embedding-ada-002",
                    EnableKeywordIndex = true,
                    EnableSemanticIndex = true,
                    Metadata = new Dictionary<string, object>
                    {
                        ["domain"] = "HR",
                        ["language"] = "zh-CN",
                        ["department"] = "Human Resources"
                    }
                });
            }
            catch
            {
                // Knowledge base might already exist - 知识库可能已经存在
            }

            // 2. Add sample HR documents - 添加示例HR文档
            await AddSampleHRDocuments(hrKnowledgeBase);

            // 3. Configure enterprise Q&A options - 配置企业问答选项
            var ragOptions = new RagOptions
            {
                EnableCitation = true,
                MaxContextLength = 3000,
                Language = "zh-CN",
                DomainFilters = new Dictionary<string, object>
                {
                    ["domain"] = "HR",
                    ["status"] = "active"
                }
            };

            // 4. Process the question - 处理问题
            var response = await _ragService.EnterpriseQAAsync(hrKnowledgeBase, question, ragOptions);

            _logger.LogInformation("HR policy question processed successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process HR policy question");
            throw;
        }
    }

    /// <summary>
    /// Example: Technical Documentation Search
    /// 示例：技术文档搜索
    /// </summary>
    public async Task<RagResponse> TechnicalDocumentationExample(string technicalQuery)
    {
        try
        {
            _logger.LogInformation("Processing technical documentation query: {Query}", technicalQuery);

            var techDocsKB = "technical_documentation";

            // 1. Create technical documentation knowledge base - 创建技术文档知识库
            try
            {
                await _ragService.CreateKnowledgeBaseAsync(techDocsKB, new RagCollectionConfig
                {
                    Description = "Technical documentation and API references",
                    ChunkSize = 1200,
                    ChunkOverlap = 300,
                    EmbeddingModel = "text-embedding-ada-002",
                    EnableKeywordIndex = true,
                    EnableSemanticIndex = true,
                    Metadata = new Dictionary<string, object>
                    {
                        ["domain"] = "Technical",
                        ["type"] = "Documentation",
                        ["audience"] = "Developers"
                    }
                });
            }
            catch { }

            // 2. Add sample technical documents - 添加示例技术文档
            await AddSampleTechnicalDocuments(techDocsKB);

            // 3. Configure technical search with hybrid retrieval - 配置带混合检索的技术搜索
            var generationRequest = new RagGenerationRequest
            {
                Query = technicalQuery,
                SystemPrompt = @"
你是一个专业的技术文档助手。请基于提供的技术文档回答用户的技术问题。

回答要求：
1. 技术准确性：确保所有技术信息准确无误
2. 代码示例：如果相关，提供代码示例
3. 最佳实践：包含相关的最佳实践建议
4. 版本信息：注明相关的版本或兼容性信息
5. 引用来源：标注信息来源以便用户查证

技术文档内容：
{CONTEXT}
",
                RetrievalOptions = new RagQuery
                {
                    Text = technicalQuery,
                    Strategy = RetrievalStrategy.Hybrid,
                    TopK = 8,
                    MinSimilarity = 0.75f,
                    Weights = new HybridRetrievalWeights
                    {
                        VectorWeight = 0.5f,
                        KeywordWeight = 0.4f,
                        SemanticWeight = 0.1f
                    },
                    ReRanking = new ReRankingOptions
                    {
                        Enabled = true,
                        MaxResults = 20,
                        Threshold = 0.6f
                    }
                },
                GenerationOptions = new RagGenerationOptions
                {
                    MaxTokens = 1500,
                    Temperature = 0.3, // Lower temperature for technical accuracy
                    Format = ResponseFormat.Markdown
                },
                IncludeSources = true
            };

            var response = await _ragService.GenerateResponseAsync(techDocsKB, generationRequest);

            _logger.LogInformation("Technical documentation query processed successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process technical documentation query");
            throw;
        }
    }

    /// <summary>
    /// Example: Legal Document Analysis
    /// 示例：法律文档分析
    /// </summary>
    public async Task<RagResponse> LegalDocumentAnalysisExample(List<string> contractIds, string analysisQuery)
    {
        try
        {
            _logger.LogInformation("Analyzing {ContractCount} legal contracts", contractIds.Count);

            var legalKB = "legal_contracts";

            // 1. Create legal knowledge base - 创建法律知识库
            try
            {
                await _ragService.CreateKnowledgeBaseAsync(legalKB, new RagCollectionConfig
                {
                    Description = "Legal contracts and compliance documents",
                    ChunkSize = 1500,
                    ChunkOverlap = 400,
                    EmbeddingModel = "text-embedding-ada-002",
                    EnableKeywordIndex = true,
                    EnableSemanticIndex = true,
                    Metadata = new Dictionary<string, object>
                    {
                        ["domain"] = "Legal",
                        ["confidentiality"] = "high",
                        ["retention_period"] = "7_years"
                    }
                });
            }
            catch { }

            // 2. Add sample legal documents - 添加示例法律文档
            await AddSampleLegalDocuments(legalKB);

            // 3. Perform multi-document analysis - 执行多文档分析
            var response = await _ragService.MultiDocumentAnalysisAsync(legalKB, contractIds, analysisQuery);

            _logger.LogInformation("Legal document analysis completed successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze legal documents");
            throw;
        }
    }

    /// <summary>
    /// Example: Customer Support Knowledge Base
    /// 示例：客户支持知识库
    /// </summary>
    public async Task<RagResponse> CustomerSupportExample(string customerQuery)
    {
        try
        {
            _logger.LogInformation("Processing customer support query: {Query}", customerQuery);

            var supportKB = "customer_support";

            // 1. Create customer support knowledge base - 创建客户支持知识库
            try
            {
                await _ragService.CreateKnowledgeBaseAsync(supportKB, new RagCollectionConfig
                {
                    Description = "Customer support FAQs and troubleshooting guides",
                    ChunkSize = 600,
                    ChunkOverlap = 100,
                    EmbeddingModel = "text-embedding-ada-002",
                    EnableKeywordIndex = true,
                    EnableSemanticIndex = true,
                    Metadata = new Dictionary<string, object>
                    {
                        ["domain"] = "CustomerSupport",
                        ["priority"] = "high",
                        ["update_frequency"] = "weekly"
                    }
                });
            }
            catch { }

            // 2. Add sample support documents - 添加示例支持文档
            await AddSampleSupportDocuments(supportKB);

            // 3. Configure customer-friendly response generation - 配置客户友好的响应生成
            var generationRequest = new RagGenerationRequest
            {
                Query = customerQuery,
                SystemPrompt = @"
你是一个专业的客户服务助手。请基于知识库内容为客户提供帮助。

回答要求：
1. 友好态度：使用友好、耐心的语调
2. 清晰步骤：如果是操作指导，提供清晰的步骤
3. 替代方案：如果可能，提供多种解决方案
4. 联系方式：如果问题复杂，提供进一步联系方式
5. 简洁明了：避免过于技术性的术语

客户问题：{QUERY}

知识库内容：
{CONTEXT}
",
                RetrievalOptions = new RagQuery
                {
                    Text = customerQuery,
                    Strategy = RetrievalStrategy.Hybrid,
                    TopK = 5,
                    MinSimilarity = 0.7f,
                    Weights = new HybridRetrievalWeights
                    {
                        VectorWeight = 0.4f,
                        KeywordWeight = 0.5f, // Higher weight for keyword matching in support
                        SemanticWeight = 0.1f
                    }
                },
                GenerationOptions = new RagGenerationOptions
                {
                    MaxTokens = 800,
                    Temperature = 0.6,
                    Format = ResponseFormat.Text
                },
                IncludeSources = false // Don't show technical sources to customers
            };

            var response = await _ragService.GenerateResponseAsync(supportKB, generationRequest);

            _logger.LogInformation("Customer support query processed successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process customer support query");
            throw;
        }
    }

    /// <summary>
    /// Example: Research Paper Summarization
    /// 示例：研究论文摘要
    /// </summary>
    public async Task<RagResponse> ResearchPaperSummarizationExample(string paperId)
    {
        try
        {
            _logger.LogInformation("Summarizing research paper: {PaperId}", paperId);

            var researchKB = "research_papers";

            // 1. Create research knowledge base - 创建研究知识库
            try
            {
                await _ragService.CreateKnowledgeBaseAsync(researchKB, new RagCollectionConfig
                {
                    Description = "Academic research papers and publications",
                    ChunkSize = 2000,
                    ChunkOverlap = 500,
                    EmbeddingModel = "text-embedding-ada-002",
                    EnableKeywordIndex = true,
                    EnableSemanticIndex = true,
                    Metadata = new Dictionary<string, object>
                    {
                        ["domain"] = "Research",
                        ["type"] = "Academic",
                        ["peer_reviewed"] = true
                    }
                });
            }
            catch { }

            // 2. Add sample research papers - 添加示例研究论文
            await AddSampleResearchPapers(researchKB);

            // 3. Generate academic summary - 生成学术摘要
            var summaryOptions = new RagSummaryOptions
            {
                Length = SummaryLength.Detailed,
                Style = SummaryStyle.Technical,
                IncludeKeyPoints = true,
                TargetAudience = "研究人员和学者"
            };

            var response = await _ragService.DocumentSummarizationAsync(researchKB, paperId, summaryOptions);

            _logger.LogInformation("Research paper summarization completed successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to summarize research paper");
            throw;
        }
    }

    #region Private Helper Methods - 私有辅助方法

    /// <summary>
    /// Add sample HR documents to knowledge base
    /// 向知识库添加示例HR文档
    /// </summary>
    private async Task AddSampleHRDocuments(string knowledgeBase)
    {
        var hrDocuments = new List<RagDocument>
        {
            new RagDocument
            {
                Id = "hr_001",
                Title = "员工休假政策",
                Content = @"
                员工休假政策

                1. 年假政策
                - 工作满一年的员工享有10天年假
                - 工作满三年的员工享有15天年假
                - 工作满五年的员工享有20天年假
                - 年假必须在当年使用，不可跨年累积

                2. 病假政策
                - 员工因病需要休假时，应提前通知直属主管
                - 病假超过3天需要提供医生证明
                - 病假期间工资按基本工资的80%发放

                3. 事假政策
                - 事假需要提前3天申请
                - 事假期间不发放工资
                - 每年事假不得超过10天

                4. 产假政策
                - 女性员工享有98天产假
                - 男性员工享有15天陪产假
                - 产假期间工资按国家规定发放
                ",
                Summary = "详细说明了公司的各种休假政策，包括年假、病假、事假和产假的具体规定。",
                Metadata = new Dictionary<string, object>
                {
                    ["department"] = "HR",
                    ["category"] = "policy",
                    ["status"] = "active",
                    ["last_updated"] = "2024-01-01"
                }
            },
            new RagDocument
            {
                Id = "hr_002",
                Title = "薪酬福利制度",
                Content = @"
                薪酬福利制度
                
                1. 薪酬结构
                - 基本工资：根据岗位和能力确定
                - 绩效奖金：根据个人和团队绩效发放
                - 年终奖：根据公司业绩和个人表现发放
                
                2. 福利待遇
                - 五险一金：按国家标准缴纳
                - 商业保险：公司为员工购买补充医疗保险
                - 餐补：每月500元餐补
                - 交通补贴：每月300元交通补贴
                
                3. 调薪机制
                - 每年进行一次薪酬调整
                - 调薪幅度根据市场水平和个人表现确定
                - 晋升时薪酬相应调整
                
                4. 福利假期
                - 生日假：员工生日当天可享受1天带薪假期
                - 结婚假：3天带薪婚假
                - 丧假：直系亲属去世可享受3天带薪丧假
                ",
                Summary = "公司薪酬福利制度的详细说明，包括薪酬结构、福利待遇、调薪机制等。",
                Metadata = new Dictionary<string, object>
                {
                    ["department"] = "HR",
                    ["category"] = "compensation",
                    ["status"] = "active",
                    ["confidentiality"] = "internal"
                }
            }
        };

        foreach (var doc in hrDocuments)
        {
            try
            {
                await _ragService.AddDocumentAsync(knowledgeBase, doc);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add HR document {DocumentId}", doc.Id);
            }
        }
    }

    /// <summary>
    /// Add sample technical documents to knowledge base
    /// 向知识库添加示例技术文档
    /// </summary>
    private async Task AddSampleTechnicalDocuments(string knowledgeBase)
    {
        var techDocuments = new List<RagDocument>
        {
            new RagDocument
            {
                Id = "tech_001",
                Title = "API 认证指南",
                Content = @"
                API 认证指南
                
                1. 认证方式
                我们的API支持多种认证方式：
                - API Key认证
                - OAuth 2.0认证
                - JWT Token认证
                
                2. API Key认证
                最简单的认证方式，适用于服务器到服务器的调用。
                
                使用方法：
                ```
                curl -H ""Authorization: Bearer YOUR_API_KEY"" \
                     https://api.example.com/v1/users
                ```
                
                3. OAuth 2.0认证
                适用于需要用户授权的场景。
                
                授权流程：
                1. 重定向用户到授权页面
                2. 用户授权后获取授权码
                3. 使用授权码换取访问令牌
                4. 使用访问令牌调用API
                
                示例代码：
                ```javascript
                const response = await fetch('https://api.example.com/oauth/token', {
                  method: 'POST',
                  headers: {
                    'Content-Type': 'application/json',
                  },
                  body: JSON.stringify({
                    grant_type: 'authorization_code',
                    code: authorizationCode,
                    client_id: 'your_client_id',
                    client_secret: 'your_client_secret'
                  })
                });
                ```
                
                4. JWT Token认证
                适用于无状态的认证场景。
                
                Token格式：
                - Header: 算法和类型信息
                - Payload: 用户信息和过期时间
                - Signature: 签名验证
                
                使用示例：
                ```
                Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
                ```
                ",
                Summary = "详细介绍了API的三种认证方式：API Key、OAuth 2.0和JWT Token，包含使用方法和代码示例。",
                Metadata = new Dictionary<string, object>
                {
                    ["category"] = "authentication",
                    ["type"] = "api_guide",
                    ["version"] = "v1.0",
                    ["audience"] = "developers"
                }
            },
            new RagDocument
            {
                Id = "tech_002",
                Title = "错误处理最佳实践",
                Content = @"
                错误处理最佳实践
                
                1. HTTP状态码
                正确使用HTTP状态码来表示请求结果：
                - 200: 成功
                - 400: 客户端错误（请求参数错误）
                - 401: 未认证
                - 403: 无权限
                - 404: 资源不存在
                - 500: 服务器内部错误
                
                2. 错误响应格式
                统一的错误响应格式：
                ```json
                {
                  ""error"": {
                    ""code"": ""INVALID_PARAMETER"",
                    ""message"": ""参数 'email' 格式不正确"",
                    ""details"": {
                      ""field"": ""email"",
                      ""value"": ""invalid-email""
                    },
                    ""timestamp"": ""2024-01-01T12:00:00Z"",
                    ""request_id"": ""req_123456""
                  }
                }
                ```
                
                3. 客户端错误处理
                ```javascript
                try {
                  const response = await fetch('/api/users', {
                    method: 'POST',
                    body: JSON.stringify(userData)
                  });
                  
                  if (!response.ok) {
                    const error = await response.json();
                    throw new Error(error.error.message);
                  }
                  
                  const data = await response.json();
                  return data;
                } catch (error) {
                  console.error('API调用失败:', error.message);
                  // 显示用户友好的错误信息
                  showErrorMessage('操作失败，请稍后重试');
                }
                ```
                
                4. 重试机制
                对于临时性错误，实现指数退避重试：
                ```javascript
                async function apiCallWithRetry(url, options, maxRetries = 3) {
                  for (let i = 0; i < maxRetries; i++) {
                    try {
                      const response = await fetch(url, options);
                      if (response.ok) {
                        return response;
                      }
                      
                      // 如果是服务器错误且不是最后一次重试
                      if (response.status >= 500 && i < maxRetries - 1) {
                        await new Promise(resolve => 
                          setTimeout(resolve, Math.pow(2, i) * 1000)
                        );
                        continue;
                      }
                      
                      throw new Error(`HTTP ${response.status}`);
                    } catch (error) {
                      if (i === maxRetries - 1) throw error;
                    }
                  }
                }
                ```
                ",
                Summary = "API错误处理的最佳实践，包括HTTP状态码使用、错误响应格式、客户端错误处理和重试机制。",
                Metadata = new Dictionary<string, object>
                {
                    ["category"] = "error_handling",
                    ["type"] = "best_practices",
                    ["language"] = "javascript",
                    ["difficulty"] = "intermediate"
                }
            }
        };

        foreach (var doc in techDocuments)
        {
            try
            {
                await _ragService.AddDocumentAsync(knowledgeBase, doc);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add technical document {DocumentId}", doc.Id);
            }
        }
    }

    /// <summary>
    /// Add sample legal documents to knowledge base
    /// 向知识库添加示例法律文档
    /// </summary>
    private async Task AddSampleLegalDocuments(string knowledgeBase)
    {
        var legalDocuments = new List<RagDocument>
        {
            new RagDocument
            {
                Id = "legal_001",
                Title = "软件许可协议",
                Content = @"
软件许可协议

第一条 许可范围
1. 许可方授予被许可方在协议期限内使用本软件的非排他性许可。
2. 被许可方可以在其内部业务中使用本软件。
3. 被许可方不得将软件转让、出租或再许可给第三方。

第二条 知识产权
1. 软件的所有知识产权归许可方所有。
2. 被许可方不得对软件进行反向工程、反编译或反汇编。
3. 被许可方不得删除或修改软件中的版权声明。

第三条 保密条款
1. 双方应对在合作过程中获得的对方商业秘密承担保密义务。
2. 保密期限为协议终止后三年。
3. 违反保密义务的一方应承担相应的法律责任。

第四条 责任限制
1. 许可方对软件的适用性不作任何明示或暗示的保证。
2. 在任何情况下，许可方的赔偿责任不超过被许可方支付的许可费用。
3. 许可方不对间接损失、利润损失等承担责任。

第五条 协议终止
1. 协议期限为一年，到期自动续约。
2. 任何一方可提前30天书面通知终止协议。
3. 协议终止后，被许可方应停止使用软件并删除所有副本。
",
                Summary = "软件许可协议的主要条款，包括许可范围、知识产权、保密条款、责任限制和协议终止等内容。",
                Metadata = new Dictionary<string, object>
                {
                    ["type"] = "license_agreement",
                    ["category"] = "software",
                    ["status"] = "active",
                    ["effective_date"] = "2024-01-01"
                }
            },
            new RagDocument
            {
                Id = "legal_002",
                Title = "数据处理协议",
                Content = @"
数据处理协议

第一条 数据处理目的
1. 数据控制者委托数据处理者处理个人数据，仅用于提供约定的服务。
2. 数据处理者不得将个人数据用于其他目的。
3. 数据处理的法律依据为合同履行和合法利益。

第二条 数据安全措施
1. 数据处理者应采取适当的技术和组织措施保护个人数据。
2. 包括但不限于：数据加密、访问控制、日志记录、定期备份。
3. 发生数据泄露时，应在24小时内通知数据控制者。

第三条 数据主体权利
1. 数据主体有权访问、更正、删除其个人数据。
2. 数据主体有权限制处理或反对处理其个人数据。
3. 数据处理者应协助数据控制者响应数据主体的权利请求。

第四条 跨境传输
1. 个人数据原则上不得传输至第三国。
2. 如需跨境传输，应确保目标国家具有充分的数据保护水平。
3. 或者采用标准合同条款、约束性公司规则等适当保障措施。

第五条 审计和监督
1. 数据控制者有权对数据处理活动进行审计。
2. 数据处理者应配合审计并提供必要的信息。
3. 审计发现的问题应及时整改。

第六条 协议终止
1. 协议终止后，数据处理者应删除或返还所有个人数据。
2. 除非法律要求保留，否则不得继续保存个人数据。
3. 数据删除应有完整的记录和证明。
",
                Summary = "数据处理协议的核心条款，涵盖数据处理目的、安全措施、数据主体权利、跨境传输、审计监督等方面。",
                Metadata = new Dictionary<string, object>
                {
                    ["type"] = "data_processing_agreement",
                    ["category"] = "privacy",
                    ["compliance"] = "GDPR",
                    ["jurisdiction"] = "EU"
                }
            }
        };

        foreach (var doc in legalDocuments)
        {
            try
            {
                await _ragService.AddDocumentAsync(knowledgeBase, doc);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add legal document {DocumentId}", doc.Id);
            }
        }
    }

    /// <summary>
    /// Add sample customer support documents to knowledge base
    /// 向知识库添加示例客户支持文档
    /// </summary>
    private async Task AddSampleSupportDocuments(string knowledgeBase)
    {
        string content = $@"""
            账户登录问题解决方案

            常见登录问题及解决方法：

            1. 忘记密码
            解决步骤：
            - 点击登录页面的“忘记密码”链接
                        - 输入您的注册邮箱地址
                        - 检查邮箱中的重置密码邮件
                        - 点击邮件中的链接设置新密码
                        - 使用新密码登录

                    注意事项：
                    - 重置链接有效期为24小时
                        - 如果没有收到邮件，请检查垃圾邮件文件夹
                        - 确保输入的邮箱地址正确

                    2. 账户被锁定
                    可能原因：
                    - 连续多次输入错误密码
                        - 账户存在安全风险
                        - 长时间未使用账户

                    解决方法：
                    - 等待30分钟后重试
                        - 联系客服解锁账户
                        - 提供身份验证信息

                    3. 验证码问题
                    常见情况：
                    - 验证码过期：重新获取验证码
                        - 收不到验证码：检查手机号是否正确，网络是否正常
                        - 验证码错误：确保输入完整的验证码

        """;
        var supportDocuments = new List<RagDocument>
                    {
                        new RagDocument
                        {
                            Id = "support_001",
                            Title = "账户登录问题解决方案",
                            Content = content,
                            Summary = "详细介绍了账户登录相关问题的解决方案，包括忘记密码、账户锁定、验证码问题等。",
                            Metadata = new Dictionary<string, object>
                            {
                                ["category"] = "account",
                                ["type"] = "troubleshooting",
                                ["priority"] = "high",
                                ["update_frequency"] = "monthly"
                            }
                        },
                        new RagDocument
                        {
                            Id = "support_002",
                            Title = "支付问题常见解答",
                            Content = """"
            支付问题常见解答

            1. 支付失败
            可能原因：
            - 银行卡余额不足
            - 银行卡已过期或被冻结
            - 网络连接不稳定
            - 支付限额超限

            解决方法：
            - 检查银行卡状态和余额
            - 尝试使用其他支付方式
            - 联系银行确认支付限额
            - 稍后重试支付

            2. 重复扣款
            如果发生重复扣款：
            - 保留支付凭证
            - 联系客服申请退款
            - 通常3-5个工作日内处理完成
            - 退款将原路返回到支付账户

            3. 支付方式
            我们支持以下支付方式：
            - 银行卡支付（借记卡、信用卡）
            - 支付宝
            - 微信支付
            - Apple Pay
            - Google Pay

            4. 发票申请
            发票申请流程：
            - 登录账户进入"发票管理"
            - 填写发票信息（个人或企业）
            - 选择需要开票的订单
            - 提交申请
            - 电子发票将发送到您的邮箱

            发票类型：
            - 个人：普通发票
            - 企业：增值税普通发票、专用发票

            5. 退款政策
            退款条件：
            - 服务未开始使用
            - 服务质量问题
            - 技术故障导致无法使用

            退款时效：
            - 申请提交后1-3个工作日审核
            - 审核通过后3-7个工作日到账
            - 具体到账时间取决于银行处理速度

            如需帮助，请联系客服：
            - 客服热线：400-123-4567
            - 在线客服：7×24小时服务
            - 邮箱：billing@example.com
            """",
                            Summary = "支付相关问题的详细解答，包括支付失败、重复扣款、支付方式、发票申请和退款政策。",
                            Metadata = new Dictionary<string, object>
                            {
                                ["category"] = "payment",
                                ["type"] = "faq",
                                ["priority"] = "high",
                                ["last_updated"] = "2024-01-15"
                            }
                        }
                    };

        foreach (var doc in supportDocuments)
        {
            try
            {
                await _ragService.AddDocumentAsync(knowledgeBase, doc);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add support document {DocumentId}", doc.Id);
            }
        }
    }

    /// <summary>
    /// Add sample research papers to knowledge base
    /// 向知识库添加示例研究论文
    /// </summary>
    private async Task AddSampleResearchPapers(string knowledgeBase)
    {
        var researchPapers = new List<RagDocument>
                    {
                        new RagDocument
                        {
                            Id = "research_001",
                            Title = "基于深度学习的自然语言处理技术研究",
                            Content = $@"""
            基于深度学习的自然语言处理技术研究

            摘要
            本文综述了深度学习在自然语言处理领域的最新进展，重点分析了Transformer架构、预训练语言模型和多模态学习等关键技术。研究表明，深度学习技术显著提升了NLP任务的性能，但仍面临可解释性、数据偏见和计算资源等挑战。

            1. 引言
            自然语言处理（NLP）是人工智能的重要分支，旨在让计算机理解和生成人类语言。近年来，深度学习技术的发展为NLP带来了革命性的变化。从早期的循环神经网络（RNN）到现在的Transformer架构，NLP模型的性能不断提升。

            2. 相关工作
            2.1 传统方法
            传统的NLP方法主要基于统计学习和规则系统，包括：
            - 词袋模型（Bag of Words）
            - TF-IDF特征提取
            - 支持向量机（SVM）
            - 条件随机场（CRF）

            这些方法在特定任务上表现良好，但难以捕捉语言的复杂语义关系。

            2.2 深度学习方法
            深度学习方法通过多层神经网络学习语言的分布式表示：
            - 词嵌入（Word Embeddings）：Word2Vec、GloVe
            - 循环神经网络：LSTM、GRU
            - 卷积神经网络：TextCNN
            - 注意力机制：Attention Mechanism

            3. Transformer架构
            Transformer是当前NLP领域最重要的架构创新，其核心特点包括：

            3.1 自注意力机制
            自注意力机制允许模型直接建模序列中任意两个位置之间的关系：
            Attention(Q,K,V) = softmax(QK^T/√d_k)V

            3.2 位置编码
            由于Transformer没有循环结构，需要位置编码来表示序列顺序：
            PE(pos,2i) = sin(pos/10000^(2i/d_model))
            PE(pos,2i+1) = cos(pos/10000^(2i/d_model))

            3.3 多头注意力
            多头注意力允许模型关注不同的表示子空间：
            MultiHead(Q,K,V) = Concat(head_1,...,head_h)W^O

            4. 预训练语言模型
            预训练语言模型通过大规模无监督学习获得通用语言表示：

            4.1 BERT
            BERT（Bidirectional Encoder Representations from Transformers）采用双向编码器：
            - 掩码语言模型（MLM）
            - 下一句预测（NSP）
            - 在多个NLP任务上取得突破性性能

            4.2 GPT系列
            GPT（Generative Pre-trained Transformer）采用自回归生成方式：
            - GPT-1：1.17亿参数
            - GPT-2：15亿参数
            - GPT-3：1750亿参数
            - GPT-4：参数量未公开，性能显著提升

            4.3 T5
            T5（Text-to-Text Transfer Transformer）将所有NLP任务统一为文本生成：
            - 统一的输入输出格式
            - 大规模预训练数据
            - 强大的迁移学习能力

            5. 多模态学习
            多模态学习结合文本、图像、音频等多种模态：

            5.1 视觉-语言模型
            - CLIP：对比学习图像和文本表示
            - DALL-E：文本到图像生成
            - Flamingo：少样本学习的视觉-语言模型

            5.2 语音-文本模型
            - Wav2Vec：自监督语音表示学习
            - Whisper：多语言语音识别
            - SpeechT5：统一的语音-文本预训练

            6. 挑战与未来方向
            6.1 可解释性
            深度学习模型的黑盒特性限制了其在关键应用中的使用。需要开发更好的解释方法。

            6.2 数据偏见
            训练数据中的偏见会被模型学习并放大，需要公平性和去偏见技术。

            6.3 计算资源
            大型语言模型需要巨大的计算资源，限制了其普及应用。

            6.4 多语言支持
            现有模型主要针对英语优化，需要更好的多语言和跨语言能力。

            7. 结论
            深度学习技术极大地推动了NLP的发展，Transformer架构和预训练模型成为主流范式。未来的研究方向包括提高模型的可解释性、减少计算资源需求、增强多语言能力等。

            参考文献
            [1] Vaswani, A., et al. Attention is all you need. NIPS 2017.
            [2] Devlin, J., et al. BERT: Pre-training of Deep Bidirectional Transformers. NAACL 2019.
            [3] Brown, T., et al. Language Models are Few-Shot Learners. NeurIPS 2020.
            [4] Raffel, C., et al. Exploring the Limits of Transfer Learning. JMLR 2020.
            """,
                            Summary = "本文综述了深度学习在自然语言处理领域的发展，重点介绍了Transformer架构、预训练语言模型和多模态学习技术。",
                            Metadata = new Dictionary<string, object>
                            {
                                ["authors"] = "张三, 李四, 王五",
                                ["journal"] = "计算机学报",
                                ["year"] = 2024,
                                ["keywords"] = "深度学习, 自然语言处理, Transformer, 预训练模型",
                                ["peer_reviewed"] = true
                            }
                        }
                    };

        foreach (var doc in researchPapers)
        {
            try
            {
                await _ragService.AddDocumentAsync(knowledgeBase, doc);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add research paper {DocumentId}", doc.Id);
            }
        }
    }

    #endregion
}

