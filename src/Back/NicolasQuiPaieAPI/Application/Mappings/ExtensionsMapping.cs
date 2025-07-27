using NicolasQuiPaieAPI.Infrastructure.Models;
using NicolasQuiPaieData.DTOs;

namespace NicolasQuiPaieAPI.Application.Mappings;

public static class ExtensionsMapping
{
    public static ProposalDto ToDto(this Proposal proposal)
     => new()
     {
         Id = proposal.Id,
         Title = proposal.Title,
         Description = proposal.Description,
         CreatedById = proposal.CreatedById,
         CreatedByDisplayName = proposal.CreatedBy?.DisplayName ?? "Unknown",
         CategoryId = proposal.CategoryId,
         CategoryName = proposal.Category?.Name ?? "Uncategorized",
         CategoryColor = proposal.Category?.Color ?? "#000000",
         CategoryIcon = proposal.Category?.IconClass ?? "fa fa-question-circle",
         Status = (NicolasQuiPaieData.DTOs.ProposalStatus)(int)proposal.Status,
         VotesFor = proposal.VotesFor,
         VotesAgainst = proposal.VotesAgainst,
         ViewsCount = proposal.ViewsCount,
         ImageUrl = proposal.ImageUrl,
         Tags = proposal.Tags,
         CreatedAt = proposal.CreatedAt,
         UpdatedAt = proposal.UpdatedAt,
         ClosedAt = proposal.ClosedAt,
         IsFeatured = proposal.IsFeatured
     };

    public static VoteDto ToDto(this Vote vote)
    => new()
    {
        Id = vote.Id,
        UserId = vote.UserId,
        ProposalId = vote.ProposalId,
        VotedAt = vote.VotedAt,
        Weight = vote.Weight,
        VoteType = (NicolasQuiPaieData.DTOs.VoteType)(int)vote.VoteType,
        Comment = vote.Comment,
        Proposal = vote.Proposal?.ToDto()
    };

    public static CommentDto ToDto(this Comment comment)
    => new()
    {
        Id = comment.Id,
        UserId = comment.UserId,
        ProposalId = comment.ProposalId,
        Content = comment.Content,
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt,
        LikesCount = comment.LikesCount,
        IsDeleted = comment.IsDeleted,
        IsModerated = comment.IsModerated,
        UserDisplayName = comment.User?.DisplayName ?? "Unknown",
        ParentCommentId = comment.ParentCommentId,
        Replies = comment.Replies.Select(r => r.ToDto()).ToList()
    };

    public static CategoryDto ToDto(this Category category)
    => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        Color = category.Color,
        IconClass = category.IconClass,
        IsActive = category.IsActive,
        SortOrder = category.SortOrder,
        ProposalsCount = category.Proposals?.Count ?? 0
    };

    public static UserDto ToDto(this ApplicationUser user)
    => new()
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt,
        IsVerified = user.IsVerified,
        ProfileImageUrl = user.ProfileImageUrl,
        ContributionLevel = (NicolasQuiPaieData.DTOs.ContributionLevel)(int)user.ContributionLevel,
        ReputationScore = user.ReputationScore,
        Bio = user.Bio
    };

    public static CreateProposalDto ToCreateDto(this Proposal proposal)
    => new()
    {
        Title = proposal.Title,
        Description = proposal.Description,
        CategoryId = proposal.CategoryId,
        ImageUrl = proposal.ImageUrl,
        Tags = proposal.Tags
    };

    public static UpdateProposalDto ToUpdateDto(this Proposal proposal)
    => new()
    {
        Title = proposal.Title,
        Description = proposal.Description,
        CategoryId = proposal.CategoryId,
        ImageUrl = proposal.ImageUrl,
        Tags = proposal.Tags
    };

    public static CreateVoteDto ToCreateDto(this Vote vote)
    => new()
    {
        ProposalId = vote.ProposalId,
        VoteType = (NicolasQuiPaieData.DTOs.VoteType)(int)vote.VoteType,
        Comment = vote.Comment
    };

    public static CreateCommentDto ToCreateDto(this Comment comment)
    => new()
    {
        ProposalId = comment.ProposalId,
        ParentCommentId = comment.ParentCommentId,
        Content = comment.Content,
    };

    // Reverse mapping from DTOs to domain models
    public static ApplicationUser ToEntity(this UserDto user)
    => new()
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt,
        IsVerified = user.IsVerified,
        ProfileImageUrl = user.ProfileImageUrl,
        ContributionLevel = (Infrastructure.Models.ContributionLevel)(int)user.ContributionLevel,
        ReputationScore = user.ReputationScore,
        Bio = user.Bio
    };

    public static Proposal ToEntity(this CreateProposalDto dto)
    => new()
    {
        Title = dto.Title,
        Description = dto.Description,
        CategoryId = dto.CategoryId,
        ImageUrl = dto.ImageUrl,
        Tags = dto.Tags
    };

    public static Vote ToEntity(this CreateVoteDto dto)
    => new()
    {
        ProposalId = dto.ProposalId,
        VoteType = (Infrastructure.Models.VoteType)(int)dto.VoteType,
        Comment = dto.Comment
    };

    public static Comment ToEntity(this CreateCommentDto dto)
    => new()
    {
        ProposalId = dto.ProposalId,
        ParentCommentId = dto.ParentCommentId,
        Content = dto.Content
    };
}