namespace NicolasQuiPaieAPI.Application.Validators;

public class CreateProposalDtoValidator : AbstractValidator<CreateProposalDto>
{
    public CreateProposalDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre est obligatoire")
            .MaximumLength(200).WithMessage("Le titre ne peut pas d�passer 200 caract�res")
            .MinimumLength(10).WithMessage("Le titre doit contenir au moins 10 caract�res");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La description est obligatoire")
            .MaximumLength(2000).WithMessage("La description ne peut pas d�passer 2000 caract�res")
            .MinimumLength(50).WithMessage("La description doit contenir au moins 50 caract�res");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Veuillez s�lectionner une cat�gorie valide");

        RuleFor(x => x.Tags)
            .MaximumLength(200).WithMessage("Les tags ne peuvent pas d�passer 200 caract�res");

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
            .MaximumLength(200).WithMessage("Le titre ne peut pas d�passer 200 caract�res")
            .MinimumLength(10).WithMessage("Le titre doit contenir au moins 10 caract�res");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La description est obligatoire")
            .MaximumLength(2000).WithMessage("La description ne peut pas d�passer 2000 caract�res")
            .MinimumLength(50).WithMessage("La description doit contenir au moins 50 caract�res");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Veuillez s�lectionner une cat�gorie valide");

        RuleFor(x => x.Tags)
            .MaximumLength(200).WithMessage("Les tags ne peuvent pas d�passer 200 caract�res");

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
            .MaximumLength(500).WithMessage("Le commentaire ne peut pas d�passer 500 caract�res");
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
            .MaximumLength(1000).WithMessage("Le commentaire ne peut pas d�passer 1000 caract�res")
            .MinimumLength(5).WithMessage("Le commentaire doit contenir au moins 5 caract�res");

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
            .MaximumLength(100).WithMessage("Le nom d'affichage ne peut pas d�passer 100 caract�res")
            .MinimumLength(2).WithMessage("Le nom d'affichage doit contenir au moins 2 caract�res")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("La biographie ne peut pas d�passer 500 caract�res");

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