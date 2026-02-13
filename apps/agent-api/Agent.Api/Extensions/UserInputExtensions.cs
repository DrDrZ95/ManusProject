namespace Agent.Api.Extensions;

public static class UserInputExtensions
{
    public static IServiceCollection AddUserInputServices(this IServiceCollection services)
    {
        services.AddScoped<IUserInputService, UserInputService>();
        return services;
    }
}

