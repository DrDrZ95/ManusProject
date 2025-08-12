using Agent.Core.Services.UserInput;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserInputExtensions
    {
        public static IServiceCollection AddUserInputServices(this IServiceCollection services)
        {
            services.AddScoped<IUserInputService, UserInputService>();
            return services;
        }
    }
}

