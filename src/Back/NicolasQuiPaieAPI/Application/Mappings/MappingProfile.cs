namespace NicolasQuiPaieAPI.Application.Mappings;

/// <summary>
/// C# 13.0 - Enhanced AutoMapper profile with contribution-based user levels
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Proposal mappings
        CreateMap<Proposal, ProposalDto>()
            .ForMember(dest => dest.CreatedByDisplayName, opt => opt.MapFrom(src => src.CreatedBy.DisplayName))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category.Color))
            .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Category.IconClass));

        CreateMap<CreateProposalDto, Proposal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.VotesFor, opt => opt.Ignore())
            .ForMember(dest => dest.VotesAgainst, opt => opt.Ignore())
            .ForMember(dest => dest.ViewsCount, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Infrastructure.Models.ProposalStatus.Active));

        CreateMap<UpdateProposalDto, Proposal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.VotesFor, opt => opt.Ignore())
            .ForMember(dest => dest.VotesAgainst, opt => opt.Ignore())
            .ForMember(dest => dest.ViewsCount, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        // Vote mappings
        CreateMap<Vote, VoteDto>()
            .ForMember(dest => dest.Proposal, opt => opt.MapFrom(src => src.Proposal))
            .ForMember(dest => dest.VoteType, opt => opt.MapFrom(src => (NicolasQuiPaieData.DTOs.VoteType)(int)src.VoteType));

        CreateMap<CreateVoteDto, Vote>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.VotedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Weight, opt => opt.Ignore())
            .ForMember(dest => dest.VoteType, opt => opt.MapFrom(src => (Infrastructure.Models.VoteType)(int)src.VoteType));

        // Comment mappings
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.UserDisplayName, opt => opt.MapFrom(src => src.User.DisplayName))
            .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));

        CreateMap<CreateCommentDto, Comment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LikesCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsModerated, opt => opt.Ignore());

        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ProposalsCount, opt => opt.MapFrom(src => src.Proposals.Count));

        // User mappings with contribution levels
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.ContributionLevel, opt => opt.MapFrom(src => (NicolasQuiPaieData.DTOs.ContributionLevel)(int)src.ContributionLevel));

        CreateMap<UpdateUserProfileDto, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ContributionLevel, opt => opt.Ignore())
            .ForMember(dest => dest.ReputationScore, opt => opt.Ignore())
            .ForMember(dest => dest.IsVerified, opt => opt.Ignore());

        // User contribution mappings - Updated to use ContributionLevel
        CreateMap<ApplicationUser, UserContributionDto>()
            .ForMember(dest => dest.UserDisplayName, opt => opt.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.UserContributionLevel, opt => opt.MapFrom(src => (NicolasQuiPaieData.DTOs.ContributionLevel)(int)src.ContributionLevel))
            .ForMember(dest => dest.ContributionCount, opt => opt.Ignore());
    }
}