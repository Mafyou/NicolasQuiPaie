using VoteDto = NicolasQuiPaieData.DTOs.VoteType;

namespace NicolasQuiPaie.UnitTests.Validators;

/// <summary>
/// C# 13.0 - DTO validator tests using collection expressions, params collections, and primary constructors
/// </summary>
[TestFixture]
public class DtoValidatorsTests
{
    private CreateProposalDtoValidator _createProposalValidator = null!;
    private UpdateProposalDtoValidator _updateProposalValidator = null!;
    private CreateVoteDtoValidator _createVoteValidator = null!;
    private CreateCommentDtoValidator _createCommentValidator = null!;

    [SetUp]
    public void Setup()
    {
        _createProposalValidator = new CreateProposalDtoValidator();
        _updateProposalValidator = new UpdateProposalDtoValidator();
        _createVoteValidator = new CreateVoteDtoValidator();
        _createCommentValidator = new CreateCommentDtoValidator();
    }

    #region CreateProposalDto Validation Tests

    // C# 13.0 - Record for test scenarios
    public record ValidationTestCase(
        string PropertyName,
        object? Value,
        bool ShouldBeValid,
        string? ExpectedErrorMessage = null);

    // C# 13.0 - Collection expressions for validation test data
    public static readonly ValidationTestCase[] CreateProposalTitleCases =
    [
        new("Title", "Valid Proposal Title", true),
        new("Title", "A", false, "Le titre doit contenir au moins 10 caract�res"),
        new("Title", "", false, "Le titre est obligatoire"),
        new("Title", "   ", false, "Le titre est obligatoire"),
        new("Title", new string('x', 201), false, "Le titre ne peut pas d�passer 200 caract�res"),
        new("Title", "Proposition avec �mojis ??????", true),
        new("Title", "Proposition avec caract�res sp�ciaux @#$%", true)
    ];

    public static readonly ValidationTestCase[] CreateProposalDescriptionCases =
    [
        new("Description", "This is a valid description that is long enough to meet the minimum requirements for proposal descriptions.", true),
        new("Description", "Too short", false, "La description doit contenir au moins 50 caract�res"),
        new("Description", "", false, "La description est obligatoire"),
        new("Description", new string('x', 2001), false, "La description ne peut pas d�passer 2000 caract�res"),
        new("Description", "Description avec des caract�res sp�ciaux et des �mojis ?? permettant de tester la validation compl�te", true)
    ];

    public static readonly ValidationTestCase[] CreateProposalCategoryCases =
    [
        new("CategoryId", 1, true),
        new("CategoryId", 5, true),
        new("CategoryId", 0, false, "Une cat�gorie doit �tre s�lectionn�e"),
        new("CategoryId", -1, false, "Une cat�gorie doit �tre s�lectionn�e")
    ];

    [Test]
    [TestCaseSource(nameof(CreateProposalTitleCases))]
    public void CreateProposalDtoValidator_ShouldValidateTitle_Correctly(ValidationTestCase testCase)
    {
        // Arrange - C# 13.0 target-typed object creation
        var dto = new CreateProposalDto
        {
            Title = testCase.Value?.ToString() ?? "",
            Description = "This is a valid description that meets the minimum length requirements for testing purposes.",
            CategoryId = 1
        };

        // Act
        var result = _createProposalValidator.TestValidate(dto);

        // Assert - C# 13.0 enhanced validation with modern null checking
        if (testCase.ShouldBeValid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Title);
            if (testCase.ExpectedErrorMessage is not null)
            {
                result.Errors.Any(e => e.PropertyName == testCase.PropertyName &&
                                      e.ErrorMessage.Contains(testCase.ExpectedErrorMessage.Split(' ')[0]))
                      .ShouldBeTrue($"Expected error message containing '{testCase.ExpectedErrorMessage.Split(' ')[0]}'");
            }
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateProposalDescriptionCases))]
    public void CreateProposalDtoValidator_ShouldValidateDescription_Correctly(ValidationTestCase testCase)
    {
        // Arrange
        var dto = new CreateProposalDto
        {
            Title = "Valid Test Title",
            Description = testCase.Value?.ToString() ?? "",
            CategoryId = 1
        };

        // Act
        var result = _createProposalValidator.TestValidate(dto);

        // Assert
        if (testCase.ShouldBeValid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }

    [Test]
    [TestCaseSource(nameof(CreateProposalCategoryCases))]
    public void CreateProposalDtoValidator_ShouldValidateCategory_Correctly(ValidationTestCase testCase)
    {
        // Arrange
        var dto = new CreateProposalDto
        {
            Title = "Valid Test Title",
            Description = "This is a valid description that meets all the minimum requirements.",
            CategoryId = Convert.ToInt32(testCase.Value)
        };

        // Act
        var result = _createProposalValidator.TestValidate(dto);

        // Assert
        if (testCase.ShouldBeValid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }
    }

    // C# 13.0 - Comprehensive validation test with collection expressions
    [Test]
    public void CreateProposalDtoValidator_ShouldPassValidation_WithCompleteValidDto()
    {
        // Arrange - C# 13.0 collection expressions for valid test data
        var validDtos = new CreateProposalDto[]
        {
            new()
            {
                Title = "Proposition de r�forme fiscale progressive",
                Description = "Cette proposition vise � mettre en place une r�forme fiscale progressive qui permettrait une meilleure redistribution des richesses tout en maintenant la comp�titivit� �conomique.",
                CategoryId = 1,
                ImageUrl = "https://example.com/image.jpg",
                Tags = "fiscalit�,r�forme,progressive"
            },
            new()
            {
                Title = "Am�lioration du syst�me de sant� publique",
                Description = "Proposition d�taill�e pour l'am�lioration du syst�me de sant� publique fran�ais avec un focus sur l'accessibilit� et la qualit� des soins pour tous les citoyens.",
                CategoryId = 3,
                Tags = "sant�,public,accessibilit�"
            }
        };

        // Act & Assert
        foreach (var dto in validDtos)
        {
            var result = _createProposalValidator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    #endregion

    #region CreateVoteDto Validation Tests

    // C# 13.0 - Collection expressions for vote validation
    public static readonly object[][] CreateVoteTestCases =
    [
        [1, VoteDto.For, true, null],
        [999, VoteDto.Against, true, null],
        [0, VoteDto.For, false, "ProposalId"],
        [-1, VoteDto.Against, false, "ProposalId"]
    ];

    [Test]
    [TestCaseSource(nameof(CreateVoteTestCases))]
    public void CreateVoteDtoValidator_ShouldValidateCorrectly(
        int proposalId, VoteDto voteType, bool shouldBeValid, string? expectedErrorProperty)
    {
        // Arrange
        var dto = new CreateVoteDto
        {
            ProposalId = proposalId,
            VoteType = voteType,
            Comment = "Optional vote comment that provides additional context"
        };

        // Act
        var result = _createVoteValidator.TestValidate(dto);

        // Assert - C# 13.0 modern null checking
        if (shouldBeValid)
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
        else
        {
            expectedErrorProperty.ShouldNotBeNull();
            result.ShouldHaveValidationErrorFor(expectedErrorProperty);
        }
    }

    // C# 13.0 - Enhanced comment validation testing with modern null patterns
    [Test]
    [TestCase("", true)] // Empty comment is allowed
    [TestCase("Valid comment", true)]
    [TestCase("Comment with special chars !@#$%^&*()", true)]
    [TestCase("Commentaire avec �mojis ???????", true)]
    public void CreateVoteDtoValidator_ShouldAllowOptionalComments(string comment, bool shouldBeValid)
    {
        // Arrange
        var dto = new CreateVoteDto
        {
            ProposalId = 1,
            VoteType = VoteDto.For,
            Comment = comment
        };

        // Act
        var result = _createVoteValidator.TestValidate(dto);

        // Assert
        if (shouldBeValid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Comment);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Comment);
        }
    }

    #endregion

    #region CreateCommentDto Validation Tests

    // C# 13.0 - Collection expressions for comment validation scenarios
    public static readonly ValidationTestCase[] CreateCommentContentCases =
    [
        new("Content", "Valid comment content", true),
        new("Content", "A", false, "minimum"),
        new("Content", "", false, "obligatoire"),
        new("Content", new string('x', 1001), false, "maximum"),
        new("Content", "Commentaire avec �mojis et caract�res sp�ciaux ?? @user #hashtag", true)
    ];

    [Test]
    [TestCaseSource(nameof(CreateCommentContentCases))]
    public void CreateCommentDtoValidator_ShouldValidateContent_Correctly(ValidationTestCase testCase)
    {
        // Arrange
        var dto = new CreateCommentDto
        {
            Content = testCase.Value?.ToString() ?? "",
            ProposalId = 1,
            ParentCommentId = null
        };

        // Act
        var result = _createCommentValidator.TestValidate(dto);

        // Assert
        if (testCase.ShouldBeValid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Content);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Content);
        }
    }

    // C# 13.0 - Enhanced hierarchical comment testing with modern null patterns
    [Test]
    [TestCase(1, null, true)] // Root comment
    [TestCase(1, 5, true)]    // Reply to comment
    [TestCase(0, null, false)] // Invalid proposal ID
    [TestCase(1, 0, false)]   // Invalid parent comment ID
    [TestCase(-1, -1, false)] // Both invalid
    public void CreateCommentDtoValidator_ShouldValidateHierarchy_Correctly(
        int proposalId, int? parentCommentId, bool shouldBeValid)
    {
        // Arrange
        var dto = new CreateCommentDto
        {
            Content = "This is a valid comment content for testing purposes",
            ProposalId = proposalId,
            ParentCommentId = parentCommentId
        };

        // Act
        var result = _createCommentValidator.TestValidate(dto);

        // Assert - C# 13.0 modern null checking and pattern matching
        if (shouldBeValid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.ProposalId);
            if (parentCommentId is not null)
            {
                result.ShouldNotHaveValidationErrorFor(x => x.ParentCommentId);
            }
        }
        else
        {
            var hasProposalError = proposalId <= 0;
            var hasParentError = parentCommentId is <= 0;

            if (hasProposalError)
                result.ShouldHaveValidationErrorFor(x => x.ProposalId);
            if (hasParentError)
                result.ShouldHaveValidationErrorFor(x => x.ParentCommentId);
        }
    }

    #endregion

    #region Cross-Validation Integration Tests

    // C# 13.0 - Comprehensive validation test with realistic data and modern null patterns
    [Test]
    public async Task AllValidators_ShouldWork_WithRealisticDataScenarios()
    {
        // Arrange - C# 13.0 collection expressions for realistic test scenarios
        var testScenarios = new[]
        {
            new
            {
                Name = "Complete Proposal Creation Flow",
                Proposal = new CreateProposalDto
                {
                    Title = "R�forme de la fiscalit� environnementale",
                    Description = "Cette proposition d�taille une r�forme compl�te de la fiscalit� environnementale fran�aise, incluant une taxe carbone progressive, des incitations pour les �nergies renouvelables, et un syst�me de bonus-malus pour les entreprises bas� sur leur empreinte carbone.",
                    CategoryId = 1,
                    ImageUrl = "https://example.com/env-tax-reform.jpg",
                    Tags = "environnement,fiscalit�,carbone,entreprises"
                },
                Vote = new CreateVoteDto
                {
                    ProposalId = 1,
                    VoteType = VoteDto.For,
                    Comment = "Excellente proposition qui va dans le bon sens pour l'environnement"
                },
                Comment = new CreateCommentDto
                {
                    ProposalId = 1,
                    Content = "Cette proposition est tr�s int�ressante, mais il faudrait pr�ciser les modalit�s d'application pour les PME",
                    ParentCommentId = null
                }
            },
            new
            {
                Name = "Social Policy Proposal",
                Proposal = new CreateProposalDto
                {
                    Title = "Renforcement du syst�me de protection sociale",
                    Description = "Proposition pour renforcer le syst�me de protection sociale fran�ais en am�liorant l'acc�s aux soins, en augmentant les aides au logement et en cr�ant un revenu universel de base pour lutter contre la pr�carit�.",
                    CategoryId = 3,
                    Tags = "social,protection,sant�,logement,revenu"
                },
                Vote = new CreateVoteDto
                {
                    ProposalId = 2,
                    VoteType = VoteDto.Against,
                    Comment = "Les co�ts de cette mesure semblent trop �lev�s pour les finances publiques"
                },
                Comment = new CreateCommentDto
                {
                    ProposalId = 2,
                    Content = "Il faudrait une �tude d'impact budg�taire plus d�taill�e",
                    ParentCommentId = null
                }
            }
        };

        // Act & Assert - C# 13.0 modern null checking in async context
        foreach (var scenario in testScenarios)
        {
            // Validate proposal
            var proposalResult = await _createProposalValidator.TestValidateAsync(scenario.Proposal);
            proposalResult.ShouldNotHaveAnyValidationErrors();

            // Validate vote
            var voteResult = await _createVoteValidator.TestValidateAsync(scenario.Vote);
            voteResult.ShouldNotHaveAnyValidationErrors();

            // Validate comment
            var commentResult = await _createCommentValidator.TestValidateAsync(scenario.Comment);
            commentResult.ShouldNotHaveAnyValidationErrors();
        }
    }

    #endregion

    // C# 13.0 - Modern performance testing with enhanced null safety
    [Test]
    public async Task Validators_ShouldPerformWell_WithLargeDataSets()
    {
        // Arrange - Generate test data using C# 13.0 features
        const int testCount = 100;
        var proposals = Enumerable.Range(1, testCount)
            .Select(i => new CreateProposalDto
            {
                Title = $"Performance Test Proposal {i}",
                Description = $"This is performance test proposal number {i} with a description that meets all the validation requirements for testing purposes.",
                CategoryId = i % 5 + 1
            }).ToArray();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var validationTasks = proposals.Select(async proposal =>
            await _createProposalValidator.TestValidateAsync(proposal));

        var results = await Task.WhenAll(validationTasks);

        stopwatch.Stop();

        // Assert - Performance should be reasonable with modern null checking
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000); // Should complete in under 1 second
        results.All(r => r is not null && !r.Errors.Any()).ShouldBeTrue(); // All should be valid
    }

    [TearDown]
    public void TearDown()
    {
        // C# 13.0 - Clean disposal pattern with modern null safety
        _createProposalValidator = null!;
        _updateProposalValidator = null!;
        _createVoteValidator = null!;
        _createCommentValidator = null!;
    }
}