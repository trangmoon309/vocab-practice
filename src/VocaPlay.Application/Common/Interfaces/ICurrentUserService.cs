// VocaPlay.Application/Common/Interfaces/ICurrentUserService.cs
namespace VocaPlay.Application.Common.Interfaces;

/// <summary>Provides the authenticated user's identity to application handlers.</summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
}
