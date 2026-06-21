using Moq;
using VocaPlay.Application.Auth.Commands;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;
using Xunit;

namespace VocaPlay.Application.Tests.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(_users.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_IssuesNewTokenPair_WhenRefreshTokenValid()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "user@test.com" };
        _jwt.Setup(j => j.ValidateRefreshToken("valid-token")).Returns(userId);
        _users.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateAccessToken(user)).Returns("new-access");
        _jwt.Setup(j => j.GenerateRefreshToken(user)).Returns("new-refresh");

        var result = await _handler.Handle(new RefreshTokenCommand("valid-token"));

        Assert.Equal("new-access", result.AccessToken);
        Assert.Equal("new-refresh", result.RefreshToken);
    }

    [Fact]
    public async Task Handle_Throws_WhenTokenInvalid()
    {
        _jwt.Setup(j => j.ValidateRefreshToken("bad-token")).Returns((Guid?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new RefreshTokenCommand("bad-token")));
    }

    [Fact]
    public async Task Handle_Throws_WhenUserNoLongerExists()
    {
        var userId = Guid.NewGuid();
        _jwt.Setup(j => j.ValidateRefreshToken("valid-token")).Returns(userId);
        _users.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new RefreshTokenCommand("valid-token")));
    }
}
