using Agent.Application.Services.Tokens;
using Agent.Application.Services.SemanticKernel;

namespace Agent.Application.Services.Prompts;

public class PromptComposer
{
    private readonly TokenCounterFactory _tokenCounterFactory;
    private readonly SemanticKernelOptions _options;

    public PromptComposer(TokenCounterFactory tokenCounterFactory, SemanticKernelOptions options)
    {
        _tokenCounterFactory = tokenCounterFactory;
        _options = options;
    }

    public string RenderComposite(
        CompositePromptRequest request,
        Func<string, PromptTemplate?> templateResolver)
    {
        var segments = new List<string>();

        if (!string.IsNullOrEmpty(request.Template.BaseTemplateId))
        {
            var baseTemplate = templateResolver(request.Template.BaseTemplateId);
            if (baseTemplate != null)
            {
                segments.Add(baseTemplate.Template);
            }
        }

        foreach (var includeId in request.Template.IncludeTemplateIds)
        {
            var includeTemplate = templateResolver(includeId);
            if (includeTemplate != null)
            {
                segments.Add(includeTemplate.Template);
            }
        }

        segments.Add(request.Template.Description);

        var builder = new StringBuilder();
        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                continue;
            }

            var rendered = ApplyVariables(segment, request.Variables);
            rendered = ApplyConditions(rendered, request.Variables);
            builder.AppendLine(rendered.Trim());
            builder.AppendLine();
        }

        if (!string.IsNullOrEmpty(request.Context))
        {
            var remainingTokens = request.MaxTokens - EstimateTokens(builder.ToString());
            if (remainingTokens > 0)
            {
                var trimmedContext = TrimToTokens(request.Context, remainingTokens);
                if (!string.IsNullOrWhiteSpace(trimmedContext))
                {
                    builder.AppendLine(trimmedContext.Trim());
                }
            }
        }

        return builder.ToString().Trim();
    }

    static string ApplyVariables(string template, Dictionary<string, object> variables)
    {
        var result = template;

        foreach (var kvp in variables)
        {
            var placeholder = "{" + kvp.Key + "}";
            var value = kvp.Value?.ToString() ?? string.Empty;
            result = result.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    static string ApplyConditions(string template, Dictionary<string, object> variables)
    {
        const string prefix = "[[if:";
        const string suffix = "]]";

        var result = new StringBuilder();
        var span = template.AsSpan();
        var index = 0;

        while (index < span.Length)
        {
            var start = template.IndexOf(prefix, index, StringComparison.Ordinal);
            if (start < 0)
            {
                result.Append(span[index..]);
                break;
            }

            result.Append(span[index..start]);

            var endHeader = template.IndexOf(suffix, start, StringComparison.Ordinal);
            if (endHeader < 0)
            {
                result.Append(span[start..]);
                break;
            }

            var conditionName = template[(start + prefix.Length)..endHeader];

            var endBlock = template.IndexOf("[[endif]]", endHeader, StringComparison.Ordinal);
            if (endBlock < 0)
            {
                result.Append(span[start..]);
                break;
            }

            var blockContent = template[(endHeader + suffix.Length)..endBlock];

            if (variables.TryGetValue(conditionName, out var value) && IsTruthy(value))
            {
                result.Append(blockContent);
            }

            index = endBlock + "[[endif]]".Length;
        }

        return result.ToString();
    }

    static bool IsTruthy(object? value)
    {
        if (value == null)
        {
            return false;
        }

        if (value is bool b)
        {
            return b;
        }

        if (value is string s)
        {
            return !string.IsNullOrWhiteSpace(s) &&
                   !string.Equals(s, "false", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(s, "0", StringComparison.OrdinalIgnoreCase);
        }

        if (value is int i)
        {
            return i != 0;
        }

        if (value is long l)
        {
            return l != 0;
        }

        if (value is double d)
        {
            return Math.Abs(d) > double.Epsilon;
        }

        return true;
    }

    private int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var counter = _tokenCounterFactory.GetCounter(_options.ChatModel);
        return counter.CountTokens(text, _options.ChatModel);
    }

    private string TrimToTokens(string text, int maxTokens)
    {
        if (maxTokens <= 0 || string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var estimatedTokens = EstimateTokens(text);
        if (estimatedTokens <= maxTokens)
        {
            return text;
        }

        var ratio = (double)maxTokens / estimatedTokens;
        var targetLength = (int)(text.Length * ratio);
        if (targetLength <= 0)
        {
            return string.Empty;
        }

        if (targetLength >= text.Length)
        {
            return text;
        }

        return text[..targetLength];
    }
}

