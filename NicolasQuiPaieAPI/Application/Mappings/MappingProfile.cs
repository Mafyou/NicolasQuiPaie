using AutoMapper;
using NicolasQuiPaieData.DTOs;
using NicolasQuiPaieAPI.Infrastructure.Models;

namespace NicolasQuiPaieAPI.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Enum mappings between Infrastructure models and DTOs
            CreateMap<Infrastructure.Models.FiscalLevel, NicolasQuiPaieData.DTOs.FiscalLevel>()
                .ConvertUsing(src => (NicolasQuiPaieData.DTOs.FiscalLevel)(int)src);
            CreateMap<NicolasQuiPaieData.DTOs.FiscalLevel, Infrastructure.Models.FiscalLevel>()
                .ConvertUsing(src => (Infrastructure.Models.FiscalLevel)(int)src);
                
            CreateMap<Infrastructure.Models.ProposalStatus, NicolasQuiPaieData.DTOs.ProposalStatus>()
                .ConvertUsing(src => (NicolasQuiPaieData.DTOs.ProposalStatus)(int)src);
            CreateMap<NicolasQuiPaieData.DTOs.ProposalStatus, Infrastructure.Models.ProposalStatus>()
                .ConvertUsing(src => (Infrastructure.Models.ProposalStatus)(int)src);
                
            CreateMap<Infrastructure.Models.VoteType, NicolasQuiPaieData.DTOs.VoteType>()
                .ConvertUsing(src => (NicolasQuiPaieData.DTOs.VoteType)(int)src);
            CreateMap<NicolasQuiPaieData.DTOs.VoteType, Infrastructure.Models.VoteType>()
                .ConvertUsing(src => (Infrastructure.Models.VoteType)(int)src);

            // Proposal mappings
            CreateMap<Proposal, ProposalDto>()
                .ForMember(dest => dest.CreatedByDisplayName, opt => opt.MapFrom(src => src.CreatedBy.DisplayName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category.Color))
                .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Category.IconClass))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (NicolasQuiPaieData.DTOs.ProposalStatus)(int)src.Status));

            CreateMap<CreateProposalDto, Proposal>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.VotesFor, opt => opt.Ignore())
                .ForMember(dest => dest.VotesAgainst, opt => opt.Ignore())
                .ForMember(dest => dest.ViewsCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsFeatured, opt => opt.Ignore())
                .ForMember(dest => dest.ClosedAt, opt => opt.Ignore());

            CreateMap<UpdateProposalDto, Proposal>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.VotesFor, opt => opt.Ignore())
                .ForMember(dest => dest.VotesAgainst, opt => opt.Ignore())
                .ForMember(dest => dest.ViewsCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsFeatured, opt => opt.Ignore())
                .ForMember(dest => dest.ClosedAt, opt => opt.Ignore());

            // Vote mappings
            CreateMap<Vote, VoteDto>()
                .ForMember(dest => dest.VoteType, opt => opt.MapFrom(src => (NicolasQuiPaieData.DTOs.VoteType)(int)src.VoteType))
                .ForMember(dest => dest.Proposal, opt => opt.MapFrom(src => src.Proposal));

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

            // User mappings
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.FiscalLevel, opt => opt.MapFrom(src => (NicolasQuiPaieData.DTOs.FiscalLevel)(int)src.FiscalLevel));
                
            CreateMap<UpdateUserProfileDto, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.FiscalLevel, opt => opt.Ignore())
                .ForMember(dest => dest.ReputationScore, opt => opt.Ignore())
                .ForMember(dest => dest.IsVerified, opt => opt.Ignore());

            // User contribution mappings - Fixed to use correct property names
            CreateMap<ApplicationUser, UserContributionDto>()
                .ForMember(dest => dest.UserDisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.UserFiscalLevel, opt => opt.MapFrom(src => (NicolasQuiPaieData.DTOs.FiscalLevel)(int)src.FiscalLevel))
                .ForMember(dest => dest.ContributionCount, opt => opt.Ignore());
        }
    }
}