﻿using Forum.Domain;
using Forum.Infrastructure;
using Mapster;
using Mediator;

namespace Forum.Application;

public class CreatePostRequest : IRequest<PostResponse>
{
    public string Header { get; set; } = null!;
    public string Body { get; set; } = null!;
}

public class CreatePostRequestHandler(ForumDbContext forumDbContext, IUserContext userContext)
    : IRequestHandler<CreatePostRequest, PostResponse>
{
    private readonly ForumDbContext _forumDbContext = forumDbContext;
    private readonly IUserContext _userContext = userContext;

    public async ValueTask<PostResponse> Handle(CreatePostRequest request, CancellationToken cancellationToken)
    {
        var id = _userContext.UserId;

        var post = request.Adapt<Post>();
        post.PostCreatorId = id;
        post.CreatedAt = DateTime.UtcNow;

        await _forumDbContext.Posts.AddAsync(post);
        await _forumDbContext.SaveChangesAsync();

        return post.Adapt<PostResponse>();
    }
}