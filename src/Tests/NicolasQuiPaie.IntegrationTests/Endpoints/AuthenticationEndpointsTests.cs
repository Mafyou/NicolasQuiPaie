namespace NicolasQuiPaie.IntegrationTests.Endpoints;

/// <summary>Registration, sign-in, token refresh and password reset, through the real Identity stores.</summary>
public class AuthenticationEndpointsTests(NicolasQuiPaieApiFactory factory) : IClassFixture<NicolasQuiPaieApiFactory>, IDisposable
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Register_ShouldCreateAUserWithTheUserRole_AndReturnAWorkingToken()
    {
        // Arrange
        var request = new RegisterRequestDto { Email = "new.nicolas@test.com", Password = "Nicolas123", DisplayName = "Nouveau Nicolas", Bio = "Contribuable" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel);
        auth.ShouldNotBeNull();
        auth.Success.ShouldBeTrue();
        auth.Token.ShouldNotBeNullOrEmpty();
        auth.User.Email.ShouldBe(request.Email);
        auth.User.DisplayName.ShouldBe(request.DisplayName);
        auth.User.ContributionLevel.ShouldBe(NicolasQuiPaieData.DTOs.ContributionLevel.PetitNicolas);

        var saved = await factory.WithContextAsync(context => context.Users.SingleAsync(user => user.Email == request.Email, Cancel));
        saved.Bio.ShouldBe(request.Bio);

        var role = await CheckRoleAsync(auth.Token);
        role.IsAuthenticated.ShouldBeTrue();
        role.Roles.ShouldContain(NicolasRoles.User);
        role.Email.ShouldBe(request.Email);
    }

    [Fact]
    public async Task Register_ShouldRefuse_AnEmailAlreadyTaken()
    {
        // Arrange
        var request = new RegisterRequestDto { Email = NicolasQuiPaieApiFactory.TestUserEmail, Password = "Nicolas123", DisplayName = "Doublon" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel);
        auth.ShouldNotBeNull().Errors.ShouldContain("Email already exists");
    }

    [Fact]
    public async Task Register_ShouldAccept_ASixCharacterPasswordWithADigitAndBothCases()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestDto { Email = $"rules-{Guid.NewGuid():N}@test.com", Password = "Abcde1", DisplayName = "Règles" }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("abc1A")]      // too short
    [InlineData("nouppercase1")]
    [InlineData("NOLOWERCASE1")]
    [InlineData("NoDigitHere")]
    public async Task Register_ShouldRefuse_AWeakPassword(string password)
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestDto { Email = $"weak-{Guid.NewGuid():N}@test.com", Password = password, DisplayName = "Faible" }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel);
        auth.ShouldNotBeNull().Success.ShouldBeFalse();
        auth.Errors.ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("", "Nicolas123", "Email is required")]
    [InlineData("someone@test.com", "", "Password is required")]
    public async Task Register_ShouldRefuse_AMissingField(string email, string password, string expectedError)
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestDto { Email = email, Password = password, DisplayName = "Incomplet" }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel)).ShouldNotBeNull().Errors.ShouldContain(expectedError);
    }

    [Fact]
    public async Task Login_ShouldReturnAToken_ForTheRightPassword_AndRecordTheSignIn()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto { Email = NicolasQuiPaieApiFactory.TestUserEmail, Password = NicolasQuiPaieApiFactory.TestUserPassword }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel);
        auth.ShouldNotBeNull().Success.ShouldBeTrue();
        auth.User.Id.ShouldBe(NicolasQuiPaieApiFactory.TestUserId);
        auth.RefreshToken.ShouldNotBeNullOrEmpty();

        var user = await factory.WithContextAsync(context => context.Users.SingleAsync(user => user.Id == NicolasQuiPaieApiFactory.TestUserId, Cancel));
        user.LastLoginAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(NicolasQuiPaieApiFactory.TestUserEmail, "WrongPassword1")]
    [InlineData("nobody@test.com", "Whatever123")]
    public async Task Login_ShouldGiveTheSameAnswer_ForAWrongPasswordAndAnUnknownEmail(string email, string password)
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = password }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel)).ShouldNotBeNull().Errors.ShouldBe(["Invalid email or password"]);
    }

    [Fact]
    public async Task CheckRole_ShouldReportAnonymous_WithoutAToken()
    {
        // Act
        var role = await CheckRoleAsync(token: null);

        // Assert
        role.IsAuthenticated.ShouldBeFalse();
        role.HighestRole.ShouldBe("Anonymous");
    }

    [Fact]
    public async Task CheckRole_ShouldReportAnInvalidToken()
    {
        // Act
        var role = await CheckRoleAsync("not-a-token");

        // Assert
        role.IsAuthenticated.ShouldBeFalse();
        role.Message.ShouldBe("Invalid or expired token");
    }

    [Fact]
    public async Task Refresh_ShouldIssueANewToken_ForAKnownUser()
    {
        // Arrange
        var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequestDto { RefreshToken = refreshToken, UserId = NicolasQuiPaieApiFactory.TestUserId }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel);
        auth.ShouldNotBeNull().Success.ShouldBeTrue();
        (await CheckRoleAsync(auth.Token)).UserId.ShouldBe(NicolasQuiPaieApiFactory.TestUserId);
    }

    [Theory]
    [InlineData("not base64!", NicolasQuiPaieApiFactory.TestUserId)]
    [InlineData("", NicolasQuiPaieApiFactory.TestUserId)]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAA==", "")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAA==", "no-such-user")]
    public async Task Refresh_ShouldRefuse_ABadRequest(string refreshToken, string userId)
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequestDto { RefreshToken = refreshToken, UserId = userId }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PasswordReset_ShouldLetTheUserSignInWithTheNewPassword()
    {
        // Arrange: a user of this test only, and the link the mail sender is handed
        const string email = "forgetful@test.com";
        (await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestDto { Email = email, Password = "OldPassword1", DisplayName = "Distrait" }, Cancel)).EnsureSuccessStatusCode();
        string? resetLink = null;
        factory.Emails.Setup(sender => sender.SendPasswordResetEmailAsync(email, It.IsAny<string>()))
            .Callback<string, string>((_, link) => resetLink = link)
            .Returns(Task.CompletedTask);

        // Act
        var forgot = await _client.PostAsJsonAsync("/api/auth/forgot-password", new PasswordResetRequestDto { Email = email }, Cancel);
        var token = System.Web.HttpUtility.ParseQueryString(new Uri(resetLink.ShouldNotBeNull()).Query)["token"];
        var reset = await _client.PostAsJsonAsync("/api/auth/reset-password",
            new PasswordResetConfirmDto { Email = email, Token = token.ShouldNotBeNull(), NewPassword = "NewPassword1" }, Cancel);

        // Assert
        forgot.StatusCode.ShouldBe(HttpStatusCode.OK);
        reset.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = "OldPassword1" }, Cancel))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = "NewPassword1" }, Cancel))
            .StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ForgotPassword_ShouldNotTellWhetherTheEmailExists()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new PasswordResetRequestDto { Email = "ghost@test.com" }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        factory.Emails.Verify(sender => sender.SendPasswordResetEmailAsync("ghost@test.com", It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_ShouldRefuse_AForgedToken()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password",
            new PasswordResetConfirmDto { Email = NicolasQuiPaieApiFactory.TestUserEmail, Token = "forged", NewPassword = "Hacked123" }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto { Email = NicolasQuiPaieApiFactory.TestUserEmail, Password = NicolasQuiPaieApiFactory.TestUserPassword }, Cancel))
            .StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private async Task<UserRoleCheckDto> CheckRoleAsync(string? token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/check-role");
        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await _client.SendAsync(request, Cancel);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<UserRoleCheckDto>(Cancel)).ShouldNotBeNull();
    }
}
