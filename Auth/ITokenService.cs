public interface ITokenService
{
    string BuildToken(string key, string issuer, UserDto user);
}