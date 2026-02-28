using Agent.Core.Interfaces;
using System.Collections.Generic;

namespace Agent.Application.Services.Tokens;

public class KimiTokenCounter : GptTokenCounter
{
    // Moonshot (Kimi) is mostly cl100k_base compatible.
}
