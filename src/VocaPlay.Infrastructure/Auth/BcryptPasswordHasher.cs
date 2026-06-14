// VocaPlay.Infrastructure/Auth/BcryptPasswordHasher.cs
using VocaPlay.Application.Common.Interfaces;

namespace VocaPlay.Infrastructure.Auth;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
