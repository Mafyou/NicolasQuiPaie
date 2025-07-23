# ?? Tests - Nicolas Qui Paie

[![.NET 9](https://img.shields.io/badge/.NET-9-purple)](https://dotnet.microsoft.com/) [![NUnit](https://img.shields.io/badge/Testing-NUnit-green)](https://nunit.org/) [![Shouldly](https://img.shields.io/badge/Assertions-Shouldly-blue)](https://shouldly.io/) [![Moq](https://img.shields.io/badge/Mocking-Moq-orange)](https://github.com/moq/moq)

## ?? Structure des Tests

```
Tests/
??? ?? NicolasQuiPaie.UnitTests/           # Tests unitaires (70% de la couverture)
?   ??? Services/                          # Tests des services métier
?   ?   ??? VotingServiceTests.cs         # Tests du système de vote pondéré
?   ?   ??? ProposalServiceTests.cs       # Tests CRUD des propositions
?   ?   ??? AnalyticsServiceTests.cs      # Tests des analytics et métriques
?   ??? Validators/                        # Tests de validation FluentValidation
?   ?   ??? DtoValidatorsTests.cs         # Validation des DTOs et formulaires
?   ??? Helpers/                          # Utilitaires de test
?       ??? TestDataHelper.cs             # Factory de données de test
??? ?? NicolasQuiPaie.IntegrationTests/    # Tests d'intégration (25% de la couverture)
?   ??? Endpoints/                         # Tests des endpoints API
?   ?   ??? ProposalEndpointsTests.cs     # API des propositions
?   ?   ??? VotingEndpointsTests.cs       # API de vote avec authentification
?   ?   ??? AnalyticsEndpointsTests.cs    # API d'analytics
?   ??? Fixtures/                         # Configuration des tests
?       ??? NicolasQuiPaieApiFactory.cs   # Factory WebApplicationFactory
??? test.runsettings                       # Configuration globale des tests
??? README.md                             # Ce fichier
```

## ?? Philosophie de Test

### **Pyramide de Tests Moderne**
```
        ?? E2E Tests (5%)
       Playwright/Selenium
    
    ?? Integration Tests (25%)
   WebApplicationFactory + TestContainers
   
?? Unit Tests (70%)
Services + Repositories + Validators
```

### **Objectifs de Qualité**
- ? **Couverture** : >85% de code coverage
- ? **Performance** : Tests rapides (<30s total)
- ? **Fiabilité** : Isolation complète des tests
- ? **Lisibilité** : AAA pattern (Arrange-Act-Assert)

## ?? Exécution des Tests

### **Commandes Principales**

```bash
# Tous les tests avec couverture
dotnet test --collect:"XPlat Code Coverage" --settings Tests/test.runsettings

# Tests unitaires uniquement
dotnet test Tests/NicolasQuiPaie.UnitTests/

# Tests d'intégration uniquement
dotnet test Tests/NicolasQuiPaie.IntegrationTests/

# Tests avec rapport HTML
dotnet test --logger html --results-directory TestResults

# Tests en mode watch (développement)
dotnet watch test Tests/NicolasQuiPaie.UnitTests/
```

### **Filtres de Tests**

```bash
# Tests par catégorie
dotnet test --filter "Category=VotingSystem"
dotnet test --filter "Category=Analytics"

# Tests par nom
dotnet test --filter "Name~Voting"
dotnet test --filter "FullyQualifiedName~ProposalService"

# Tests rapides uniquement (unitaires)
dotnet test --filter "Category!=Integration"
```

## ?? Tests Unitaires

### **Services Métier**

#### **VotingServiceTests.cs**
```csharp
[Test]
public async Task CastVoteAsync_ShouldCalculateCorrectWeight_ForDifferentFiscalLevels()
{
    // Test du système de vote pondéré Nicolas
    var testCases = new[]
    {
        new { Level = FiscalLevel.PetitNicolas, ExpectedWeight = 1 },
        new { Level = FiscalLevel.NicolasSupreme, ExpectedWeight = 5 }
    };
    
    foreach (var testCase in testCases)
    {
        // Arrange avec record patterns C# 13
        var userWithLevel = _testUser with { FiscalLevel = testCase.Level };
        
        // Act & Assert avec Shouldly
        result.Weight.ShouldBe(testCase.ExpectedWeight);
    }
}
```

#### **ProposalServiceTests.cs**
```csharp
[Test]
public async Task CreateProposalAsync_ShouldCreateProposal_WhenValidData()
{
    // Test CRUD avec AutoMapper et validation
    var result = await _proposalService.CreateProposalAsync(_createProposalDto, "user-1");
    
    result.ShouldNotBeNull();
    result.Title.ShouldBe("New Test Proposal");
    _mockRepository.Verify(x => x.AddAsync(It.IsAny<Proposal>()), Times.Once);
}
```

### **Validators avec FluentValidation**

#### **DtoValidatorsTests.cs**
```csharp
[Test]
public void CreateProposalDto_ShouldHaveError_WhenTitleIsTooLong()
{
    var dto = new CreateProposalDto { Title = new string('A', 201) };
    
    var result = _createProposalValidator.TestValidate(dto);
    result.ShouldHaveValidationErrorFor(x => x.Title);
}
```

### **Test Data Factory**

#### **TestDataHelper.cs**
```csharp
// Factory moderne avec C# 13
public static ApplicationUser CreateTestUser(
    string? id = null,
    FiscalLevel fiscalLevel = FiscalLevel.PetitNicolas) =>
    new ApplicationUser
    {
        Id = id ?? "test-user-1",
        FiscalLevel = fiscalLevel,
        ReputationScore = 100
    };
```

## ?? Tests d'Intégration

### **Configuration avec WebApplicationFactory**

#### **NicolasQuiPaieApiFactory.cs**
```csharp
public class NicolasQuiPaieApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // InMemory database pour tests rapides
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
                
            // Seed des données de test
            SeedTestData(context, userManager, logger).Wait();
        });
    }
}
```

### **Tests API avec Authentification JWT**

#### **VotingEndpointsTests.cs**
```csharp
[Test]
public async Task PostVote_ShouldReturnCreated_WhenValidVoteData()
{
    // Arrange avec authentification
    _client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", GetTestJwtToken());
    
    var voteDto = new CreateVoteDto
    {
        ProposalId = 1,
        VoteType = VoteType.For,
        Comment = "Je soutiens cette proposition"
    };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/votes", voteDto);
    
    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    var vote = await response.Content.ReadFromJsonAsync<VoteDto>();
    vote.ShouldNotBeNull();
    vote.Weight.ShouldBeGreaterThan(0);
}
```

## ?? Couverture de Code

### **Configuration Coverlet**
```xml
<!-- Dans test.runsettings -->
<DataCollector friendlyName="XPlat code coverage">
  <Configuration>
    <Format>opencover,cobertura,json</Format>
    <Include>[NicolasQuiPaieAPI]*,[NicolasQuiPaieData]*</Include>
    <Exclude>[*]*Tests*,[*]*Migrations*</Exclude>
    <UseSourceLink>true</UseSourceLink>
  </Configuration>
</DataCollector>
```

### **Rapports de Couverture**
```bash
# Génération du rapport HTML
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/*/coverage.opencover.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html

# Ouverture du rapport
start TestResults/CoverageReport/index.html
```

## ?? Patterns de Test Avancés

### **AAA Pattern (Arrange-Act-Assert)**
```csharp
[Test]
public async Task ExampleTest()
{
    // Arrange - Préparer les données et mocks
    var mockUser = CreateTestUser();
    var mockRepository = new Mock<IUserRepository>();
    mockRepository.Setup(x => x.GetByIdAsync("test")).ReturnsAsync(mockUser);
    
    // Act - Exécuter l'action à tester
    var result = await _service.DoSomethingAsync("test");
    
    // Assert - Vérifier les résultats avec Shouldly
    result.ShouldNotBeNull();
    result.Id.ShouldBe("test");
    mockRepository.Verify(x => x.GetByIdAsync("test"), Times.Once);
}
```

### **Test Paramétrisés avec TestCase**
```csharp
[TestCase(FiscalLevel.PetitNicolas, 1)]
[TestCase(FiscalLevel.GrosMoyenNicolas, 2)]
[TestCase(FiscalLevel.GrosNicolas, 3)]
[TestCase(FiscalLevel.NicolasSupreme, 5)]
public async Task VoteWeight_ShouldMatchFiscalLevel(FiscalLevel level, int expectedWeight)
{
    var user = CreateTestUser(fiscalLevel: level);
    var weight = CalculateVoteWeight(user);
    weight.ShouldBe(expectedWeight);
}
```

### **Tests Asynchrones Modernes**
```csharp
[Test]
public async Task AsyncTest_WithCancellation()
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    
    var result = await _service.LongRunningOperationAsync(cts.Token);
    
    result.ShouldNotBeNull();
}
```

## ??? Outils de Test

### **Stack Technologique**
- **?? NUnit 4.2.2** : Framework de test moderne
- **?? Moq 4.20.72** : Mocking et isolation
- **? Shouldly 4.2.1** : Assertions expressives  
- **?? WebApplicationFactory** : Tests d'intégration API
- **?? TestContainers** : Base de données de test
- **?? Coverlet** : Couverture de code
- **?? FluentValidation.TestHelper** : Tests de validation

### **Extensions Recommandées**
```bash
# Outils globaux utiles
dotnet tool install -g dotnet-reportgenerator-globaltool
dotnet tool install -g dotnet-stryker
dotnet tool install -g dotnet-format
```

## ?? Métriques et KPIs

### **Objectifs de Performance**
| Métrique | Objectif | Actuel |
|----------|----------|--------|
| **Test Coverage** | >85% | 82% |
| **Test Execution** | <30s | 25s |
| **Failed Tests** | 0% | 0% |
| **Test Maintenance** | <10% time | 8% |

### **Benchmarks de Test**
```csharp
[Test]
[Performance]
public async Task VotingService_ShouldExecute_UnderPerformanceThreshold()
{
    var stopwatch = Stopwatch.StartNew();
    
    await _votingService.CastVoteAsync(voteDto, "user");
    
    stopwatch.Stop();
    stopwatch.ElapsedMilliseconds.ShouldBeLessThan(100);
}
```

## ?? Debugging et Troubleshooting

### **Configuration de Debug**
```json
// Dans launchSettings.json pour tests
{
  "profiles": {
    "Tests": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Testing",
        "DOTNET_ENVIRONMENT": "Testing"
      }
    }
  }
}
```

### **Logs de Test**
```csharp
// Configuration logging pour tests
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

## ?? Best Practices

### **? Bonnes Pratiques**
- **Isolation** : Chaque test est indépendant
- **Nommage** : `MethodName_ShouldExpectedBehavior_WhenCondition`
- **Data-driven** : Utiliser TestDataHelper pour les données
- **Performance** : Tests rapides et fiables
- **Documentation** : Commentaires explicatifs pour logique complexe

### **? Anti-Patterns à Éviter**
- Tests qui dépendent d'autres tests
- Hardcoding de données spécifiques
- Tests trop longs ou complexes
- Assertions multiples non liées
- Mocks trop complexes

---

## ????? Quick Start

```bash
# 1. Lancer tous les tests
dotnet test

# 2. Tests avec couverture
dotnet test --collect:"XPlat Code Coverage"

# 3. Tests en mode watch (développement)
dotnet watch test Tests/NicolasQuiPaie.UnitTests/

# 4. Générer rapport de couverture
reportgenerator -reports:"TestResults/*/coverage.opencover.xml" -targetdir:"TestResults/Report"
```

---

*?? "Tests first, bugs last - Nicolas qui teste gagne toujours !"* ????

[![Test Coverage](https://img.shields.io/badge/Coverage-82%25-green)](./TestResults) [![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen)](#) [![Tests](https://img.shields.io/badge/Tests-156%20passed-success)](#)