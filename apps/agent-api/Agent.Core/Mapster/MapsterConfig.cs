namespace Agent.Core.Mapster;

/// <summary>
/// Mapster 映射配置
/// Mapster mapping configuration
/// </summary>
public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        // 配置 User 到 UserDto 的映射
        // Configure mapping from User to UserDto
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(dest => dest.FullName, src => src.FirstName + " " + src.LastName);

        // 也可以配置反向映射
        // Can also configure reverse mapping
        // TypeAdapterConfig<UserDto, User>.NewConfig()
        //    .Map(dest => dest.FirstName, src => src.FullName.Split(' ')[0])
        //    .Map(dest => dest.LastName, src => src.FullName.Split(' ')[1]);
    }
}