using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AgentWebApi.Services
{
    /// <summary>
    /// Service for OpenTelemetry integration.
    /// </summary>
    public class TelemetryService : ITelemetryService
    {
        private readonly ILogger<TelemetryService> _logger;
        private static readonly ActivitySource _activitySource = new ActivitySource("AgentWebApi");

        public TelemetryService(ILogger<TelemetryService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates a new activity for tracing.
        /// </summary>
        /// <param name="name">The name of the activity.</param>
        /// <returns>The created activity.</returns>
        public Activity StartActivity(string name)
        {
            _logger.LogInformation("Starting activity: {ActivityName}", name);
            return _activitySource.StartActivity(name);
        }

        /// <summary>
        /// Executes an action within a traced activity.
        /// </summary>
        /// <typeparam name="T">The return type of the action.</typeparam>
        /// <param name="name">The name of the activity.</param>
        /// <param name="action">The action to execute.</param>
        /// <param name="tags">Optional tags to add to the activity.</param>
        /// <returns>The result of the action.</returns>
        public T TraceActivity<T>(string name, Func<T> action, params (string Key, object Value)[] tags)
        {
            using var activity = StartActivity(name);
            
            if (activity != null)
            {
                foreach (var (key, value) in tags)
                {
                    activity.SetTag(key, value);
                }
            }

            try
            {
                var result = action();
                
                if (activity != null)
                {
                    activity.SetStatus(ActivityStatusCode.Ok);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                if (activity != null)
                {
                    activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity.RecordException(ex);
                }
                
                _logger.LogError(ex, "Error in activity {ActivityName}: {ErrorMessage}", name, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Executes an asynchronous action within a traced activity.
        /// </summary>
        /// <typeparam name="T">The return type of the action.</typeparam>
        /// <param name="name">The name of the activity.</param>
        /// <param name="action">The asynchronous action to execute.</param>
        /// <param name="tags">Optional tags to add to the activity.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<T> TraceActivityAsync<T>(string name, Func<Task<T>> action, params (string Key, object Value)[] tags)
        {
            using var activity = StartActivity(name);
            
            if (activity != null)
            {
                foreach (var (key, value) in tags)
                {
                    activity.SetTag(key, value);
                }
            }

            try
            {
                var result = await action();
                
                if (activity != null)
                {
                    activity.SetStatus(ActivityStatusCode.Ok);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                if (activity != null)
                {
                    activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity.RecordException(ex);
                }
                
                _logger.LogError(ex, "Error in activity {ActivityName}: {ErrorMessage}", name, ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Interface for telemetry service.
    /// </summary>
    public interface ITelemetryService
    {
        Activity StartActivity(string name);
        T TraceActivity<T>(string name, Func<T> action, params (string Key, object Value)[] tags);
        Task<T> TraceActivityAsync<T>(string name, Func<Task<T>> action, params (string Key, object Value)[] tags);
    }
}
