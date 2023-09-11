using Microsoft.AspNetCore.Authentication;

namespace ColorCodeHackathon2023.Auth;

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; }
}
