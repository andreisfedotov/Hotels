public record UserDto(string UserName, string Password);

public record UserModel
{
    [Required]
    public string UserName { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}