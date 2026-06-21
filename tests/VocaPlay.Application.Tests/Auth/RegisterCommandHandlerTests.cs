using Moq;
using VocaPlay.Application.Auth.Commands;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;
using Xunit;

namespace VocaPlay.Application.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_users.Object, _hasher.Object, _jwt.Object);
    }

    [Fact]
    public async Task Handle_CreatesUser_WhenEmailNotTaken()
    {
        _users.Setup(r => r.GetByEmailAsync("new@test.com", default)).ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash("secret123")).Returns("hashed");
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        _jwt.Setup(j => j.GenerateRefreshToken(It.IsAny<User>())).Returns("refresh-token");

        var result = await _handler.Handle(new RegisterCommand("new@test.com", "New User", "secret123"));

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal("new@test.com", result.User.Email);
        Assert.Equal("New User", result.User.DisplayName);
        _users.Verify(r => r.AddAsync(It.Is<User>(u => u.PasswordHash == "hashed"), default), Times.Once);
    }

    [Fact]
    public async Task Handle_NormalizesEmailAndTrimsDisplayName()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("a");
        _jwt.Setup(j => j.GenerateRefreshToken(It.IsAny<User>())).Returns("r");

        User? captured = null;
        _users.Setup(r => r.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((u, _) => captured = u)
            .Returns(Task.CompletedTask);

        await _handler.Handle(new RegisterCommand("  Mixed@Test.com  ", "  Display Name  ", "secret123"));

        Assert.Equal("mixed@test.com", captured!.Email);
        Assert.Equal("Display Name", captured.DisplayName);
    }

    [Fact]
    public async Task Handle_Throws_WhenEmailAlreadyInUse()
    {
        _users.Setup(r => r.GetByEmailAsync("taken@test.com", default))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "taken@test.com" });

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new RegisterCommand("taken@test.com", "Someone", "secret123")));

        _users.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Never);
    }
}
