// VocaPlay.Application/WordSets/Queries/GetWordSetByIdQuery.cs
namespace VocaPlay.Application.WordSets.Queries;

public record GetWordSetByIdQuery(Guid WordSetId, Guid UserId);
