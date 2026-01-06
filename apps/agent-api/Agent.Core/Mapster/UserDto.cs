namespace Agent.Core.Mapster;

/// <summary>
/// 用户数据传输对象 (User Data Transfer Object)
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
}