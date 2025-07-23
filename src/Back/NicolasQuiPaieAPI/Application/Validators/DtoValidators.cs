namespace NicolasQuiPaieAPI.Application.Validators;

public class CreateProposalDtoValidator : AbstractValidator<CreateProposalDto>
{
    public CreateProposalDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre est obligatoire")
            .MaximumLength(200).WithMessage("Le titre ne peut pas dépasser 200 caractères")
            .MinimumLength(10).WithMessage("Le titre doit contenir au moins 10 caractères");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La description est obligatoire")
            .MaximumLength(2000).WithMessage("La description ne peut pas dépasser 2000 caractères")
            .MinimumLength(50).WithMessage("La description doit contenir au moins 50 caractères");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Veuillez sélectionner une catégorie valide");

        RuleFor(x => x.Tags)
            .MaximumLength(200).WithMessage("Les tags ne peuvent pas dépasser 200 caractères");

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).WithMessage("L'URL de l'image n'est pas valide")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class UpdateProposalDtoValidator : AbstractValidator<UpdateProposalDto>
{
    public UpdateProposalDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre est obligatoire")
            .MaximumLength(200).WithMessage("Le titre ne peut pas dépasser 200 caractères")
            .MinimumLength(10).WithMessage("Le titre doit contenir au moins 10 caractères");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La description est obligatoire")
            .MaximumLength(2000).WithMessage("La description ne peut pas dépasser 2000 caractères")
            .MinimumLength(50).WithMessage("La description doit contenir au moins 50 caractères");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Veuillez sélectionner une catégorie valide");

        RuleFor(x => x.Tags)
            .MaximumLength(200).WithMessage("Les tags ne peuvent pas dépasser 200 caractères");

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).WithMessage("L'URL de l'image n'est pas valide")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class CreateVoteDtoValidator : AbstractValidator<CreateVoteDto>
{
    public CreateVoteDtoValidator()
    {
        RuleFor(x => x.ProposalId)
            .GreaterThan(0).WithMessage("ID de proposition invalide");

        RuleFor(x => x.VoteType)
            .IsInEnum().WithMessage("Type de vote invalide");

        RuleFor(x => x.Comment)
            .MaximumLength(500).WithMessage("Le commentaire ne peut pas dépasser 500 caractères");
    }
}

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.ProposalId)
            .GreaterThan(0).WithMessage("ID de proposition invalide");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Le contenu du commentaire est obligatoire")
            .MaximumLength(1000).WithMessage("Le commentaire ne peut pas dépasser 1000 caractères")
            .MinimumLength(5).WithMessage("Le commentaire doit contenir au moins 5 caractères");

        RuleFor(x => x.ParentCommentId)
            .GreaterThan(0).WithMessage("ID de commentaire parent invalide")
            .When(x => x.ParentCommentId.HasValue);
    }
}

public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("Le nom d'affichage ne peut pas dépasser 100 caractères")
            .MinimumLength(2).WithMessage("Le nom d'affichage doit contenir au moins 2 caractères")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("La biographie ne peut pas dépasser 500 caractères");

        RuleFor(x => x.ProfileImageUrl)
            .Must(BeAValidUrl).WithMessage("L'URL de l'image de profil n'est pas valide")
            .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl));
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}