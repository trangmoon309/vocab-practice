using Moq;
using VocaPlay.Application.Auth.Commands;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;
using Xunit;

namespace VocaPlay.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_users.Object, _hasher.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_ReturnsTokens_WhenCredentialsValid()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.com", DisplayName = "User", PasswordHash = "hashed" };
        _users.Setup(r => r.GetByEmailAsync("user@test.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("secret123", "hashed")).Returns(true);
        _jwt.Setup(j => j.GenerateAccessToken(user)).Returns("access-token");
        _jwt.Setup(j => j.GenerateRefreshToken(user)).Returns("refresh-token");

        var result = await _handler.Handle(new LoginCommand("user@test.com", "secret123"));

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserNotFound()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new LoginCommand("nobody@test.com", "secret123")));
    }

    [Fact]
    public async Task Handle_Throws_WhenPasswordWrong()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@test.com", PasswordHash = "hashed" };
        _users.Setup(r => r.GetByEmailAsync("user@test.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new LoginCommand("user@test.com", "wrong")));
    }
}
