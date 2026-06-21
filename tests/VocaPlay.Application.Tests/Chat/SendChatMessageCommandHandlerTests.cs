using Moq;
using VocaPlay.Application.Chat.Commands;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Common.Interfaces.Services;
using VocaPlay.Application.Words.Commands;
using VocaPlay.Domain.Entities;
using Xunit;

namespace VocaPlay.Application.Tests.Chat;

public class SendChatMessageCommandHandlerTests
{
    private readonly Mock<IChatRepository> _chat = new();
    private readonly Mock<IAiChatService> _ai = new();
    private readonly Mock<IWordRepository> _wordsRepo = new();
    private readonly SendChatMessageCommandHandler _handler;

    public SendChatMessageCommandHandlerTests()
    {
        _wordsRepo.Setup(r => r.GetEnglishWordsForUserAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new List<string>());
        var bulkAdd = new BulkAddWordsCommandHandler(_wordsRepo.Object);
        _handler = new SendChatMessageCommandHandler(_chat.Object, _ai.Object, bulkAdd);
    }

    [Fact]
    public async Task Handle_ReturnsPlainReply_WhenNoActionBlock()
    {
        var userId = Guid.NewGuid();
        _chat.Setup(r => r.GetByUserIdAsync(userId, 10, default)).ReturnsAsync(new List<ChatMessage>());
        _ai.Setup(a => a.GetCompletionAsync("hello", It.IsAny<IEnumerable<ChatMessage>>(), default))
            .ReturnsAsync("Hi there! How can I help?");

        var result = await _handler.Handle(new SendChatMessageCommand(userId, "hello"));

        Assert.Equal("Hi there! How can I help?", result.Reply);
        Assert.Null(result.Action);
        _chat.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<ChatMessage>>(msgs => msgs.Count() == 2), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ParsesActionBlock_TriggersBulkAdd_AndStripsBlockFromReply()
    {
        var userId = Guid.NewGuid();
        _chat.Setup(r => r.GetByUserIdAsync(userId, 10, default)).ReturnsAsync(new List<ChatMessage>());

        const string raw = """
            I've added that word for you!
            %%ACTION%%{"type":"BULK_ADD_WORDS","words":[{"english":"apple","vietnamese":"qua tao","pronunciation":null,"level":"A1","type":"Noun","exampleSentence":null}]}%%END%%
            """;
        _ai.Setup(a => a.GetCompletionAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ChatMessage>>(), default))
            .ReturnsAsync(raw);

        var result = await _handler.Handle(new SendChatMessageCommand(userId, "add apple = qua tao"));

        Assert.DoesNotContain("%%ACTION%%", result.Reply);
        Assert.NotNull(result.Action);
        Assert.Equal("BULK_ADD_WORDS", result.Action!.Type);
        Assert.Equal(1, result.Action.WordsAdded);
        _wordsRepo.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<Word>>(ws => ws.Count() == 1), default), Times.Once);
    }

    [Fact]
    public async Task Handle_PersistsUserMessageAndCleanReply_NotRawActionBlock()
    {
        var userId = Guid.NewGuid();
        _chat.Setup(r => r.GetByUserIdAsync(userId, 10, default)).ReturnsAsync(new List<ChatMessage>());

        const string raw = """
            Added it.
            %%ACTION%%{"type":"BULK_ADD_WORDS","words":[{"english":"book","vietnamese":"sach","pronunciation":null,"level":null,"type":null,"exampleSentence":null}]}%%END%%
            """;
        _ai.Setup(a => a.GetCompletionAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ChatMessage>>(), default))
            .ReturnsAsync(raw);

        List<ChatMessage>? persisted = null;
        _chat.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ChatMessage>>(), default))
            .Callback<IEnumerable<ChatMessage>, CancellationToken>((msgs, _) => persisted = msgs.ToList())
            .Returns(Task.CompletedTask);

        await _handler.Handle(new SendChatMessageCommand(userId, "add book"));

        Assert.All(persisted!, m => Assert.DoesNotContain("%%ACTION%%", m.Content));
    }

    [Fact]
    public async Task Handle_ReturnsNullAction_WhenActionJsonIsMalformed()
    {
        var userId = Guid.NewGuid();
        _chat.Setup(r => r.GetByUserIdAsync(userId, 10, default)).ReturnsAsync(new List<ChatMessage>());
        _ai.Setup(a => a.GetCompletionAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ChatMessage>>(), default))
            .ReturnsAsync("Reply %%ACTION%%not-json%%END%%");

        var result = await _handler.Handle(new SendChatMessageCommand(userId, "x"));

        Assert.Null(result.Action);
    }
}
