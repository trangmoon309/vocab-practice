// VocaPlay.Application/Common/Interfaces/IPasswordHasher.cs
namespace VocaPlay.Application.Common.Interfaces;

/// <summary>Abstraction over bcrypt password hashing — keeps Application free of BCrypt NuGet.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
