namespace Agent.Core.Mapster;

/// <summary>
/// Mapster 使用示例
/// Mapster Usage Example
/// </summary>
public class MapsterUsageExample
{
    public static void RunExample()
    {
        // 注册映射配置
        // Register mapping configurations
        MapsterConfig.RegisterMappings();

        // 创建源对象
        // Create source object
        var user = new User { Id = 1, FirstName = "John", LastName = "Doe" };
        Console.WriteLine($"Source User: Id={user.Id}, FirstName={user.FirstName}, LastName={user.LastName}");

        // 使用 Mapster 进行映射
        // Use Mapster for mapping
        var userDto = user.Adapt<UserDto>();
        Console.WriteLine($"Mapped UserDto: Id={userDto.Id}, FirstName={userDto.FirstName}, LastName={userDto.LastName}, FullName={userDto.FullName}");

        // 验证映射结果
        // Verify mapping result
        if (userDto.FullName == "John Doe")
        {
            Console.WriteLine("Mapster mapping successful!");
        }
        else
        {
            Console.WriteLine("Mapster mapping failed.");
        }
    }
}
