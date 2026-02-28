using Agent.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Agent.Application.Services.Tokens;

public class TokenCounterFactory
{
    private readonly IEnumerable<ITokenCounter> _counters;

    public TokenCounterFactory(IEnumerable<ITokenCounter> counters)
    {
        _counters = counters;
    }

    public ITokenCounter GetCounter(string modelId)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            return _counters.OfType<FallbackTokenCounter>().FirstOrDefault() ?? new FallbackTokenCounter();
        }

        string modelLower = modelId.ToLowerInvariant();

        if (modelLower.StartsWith("gpt-"))
        {
            return _counters.OfType<GptTokenCounter>().FirstOrDefault() ?? new GptTokenCounter();
        }
        else if (modelLower.StartsWith("deepseek-"))
        {
            return _counters.OfType<DeepSeekTokenCounter>().FirstOrDefault() ?? new DeepSeekTokenCounter();
        }
        else if (modelLower.StartsWith("moonshot-") || modelLower.Contains("kimi"))
        {
            return _counters.OfType<KimiTokenCounter>().FirstOrDefault() ?? new KimiTokenCounter();
        }

        return _counters.OfType<FallbackTokenCounter>().FirstOrDefault() ?? new FallbackTokenCounter();
    }
}
