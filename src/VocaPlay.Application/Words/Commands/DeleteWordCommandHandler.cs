// VocaPlay.Application/Words/Commands/DeleteWordCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Words.Commands;

public class DeleteWordCommandHandler
{
    private readonly IWordRepository _words;

    public DeleteWordCommandHandler(IWordRepository words) => _words = words;

    /// <summary>Deletes a word. Enforces ownership.</summary>
    public async Task Handle(DeleteWordCommand command, CancellationToken ct = default)
    {
        var word = await _words.GetByIdAsync(command.WordId, ct)
            ?? throw new NotFoundException(nameof(Word), command.WordId);

        if (word.UserId != command.UserId)
            throw new ForbiddenException();

        await _words.DeleteAsync(word, ct);
    }
}
