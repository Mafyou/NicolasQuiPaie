namespace NicolasQuiPaie.IntegrationTests.Fixtures;

/// <summary>
/// Bearer tokens the API accepts: same key, issuer and audience as its configuration, with the roles of the caller.
/// </summary>
public static class TestTokens
{
    private const string Key = "MySecretKeyForNicolasQuiPaie2025!";
    private const string Issuer = "NicolasQuiPaieAPI";
    private const string Audience = "NicolasQuiPaieClient";

    public static string For(string userId, params string[] roles)
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, userId),
            new("DisplayName", $"Display {userId}"),
            .. roles.Select(role => new Claim(ClaimTypes.Role, role))
        ];

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)), SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    extension(HttpRequestMessage request)
    {
        /// <summary>Sends the request as this user, with these roles.</summary>
        public HttpRequestMessage Authorized(string userId, params string[] roles)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", For(userId, roles));
            return request;
        }
    }
}
