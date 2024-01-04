using FluentAssertions;
using Forum.Application;
using Forum.Common;
using Forum.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace Forum.UnitTests;

public class PostCrudeTests
{   
    private readonly Guid _userid = Guid.NewGuid();
    private readonly ForumDbContext _forumDbContext;

    public PostCrudeTests()
    {
        var contextOptions = new DbContextOptionsBuilder<ForumDbContext>()
            .UseInMemoryDatabase("PostCrudeTests")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _forumDbContext = new(contextOptions);

        _forumDbContext.Database.EnsureDeleted();
        _forumDbContext.Database.EnsureCreated();
    }

    public async Task<PostResponse> CreatePost()
    {
        var handler = new CreatePostRequestHandler(_forumDbContext);
        
        var post = await handler.Handle(new()
        {
            Header = "testheader",
            Body = "testbody",
            PostCreatorId = _userid
        }, new());

        return post;
    }

    [Fact]
    public async Task CreatePostTest()
    {
        var post = await CreatePost();

        (await _forumDbContext.Posts.FirstAsync()).Id.Should().Be(post.Id);
    }

    [Fact]
    public async Task GetAllPostsTest()
    {
        await CreatePost();
        await CreatePost();
        await CreatePost();

        _forumDbContext.Posts.Should().NotBeEmpty();
        _forumDbContext.Posts.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPostTest()
    {
        var post = await CreatePost();

        var handler = new GetPostRequestHandler(_forumDbContext);
        var response = await handler.Handle(new() 
        {
            Id = post.Id
        }, new());

        response.Value.Id.Should().Be(post.Id);
        _forumDbContext.Posts.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPostTestWithWrongId()
    {
        await CreatePost();

        var handler = new GetPostRequestHandler(_forumDbContext);
        var request = new GetPostRequest()
        {
            Id = Guid.NewGuid()
        };
        
        var response = await handler.Handle(request, new());

        response.Errors.Should().NotBeEmpty();
        response.Errors.First().Code.Should().Be("General.NotFound");
    }
}