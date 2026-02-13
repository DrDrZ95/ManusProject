global using Agent.Metering.eBPF;
global using Agent.Metering.Extensions;
global using Agent.Metering.Middleware;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using OpenTelemetry; // 引入OpenTelemetry
global using OpenTelemetry.Trace; // 引入OpenTelemetry追踪
global using System;
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Net;
global using System.Threading.Tasks;

