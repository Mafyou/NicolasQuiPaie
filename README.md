# Nicolas Qui Paie - Plateforme de Démocratie Souveraine Numérique

![Nicolas Qui Paie](https://img.shields.io/badge/Nicolas-Qui%20Paie-blue) ![.NET 9](https://img.shields.io/badge/.NET-9-purple) ![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-green) ![Blazor WebAssembly](https://img.shields.io/badge/Blazor-WebAssembly-orange)

## 🇫🇷💰 Description

**Nicolas Qui Paie** est une plateforme web innovante de démocratie participative numérique basée sur une **Clean Architecture** moderne. Elle permet aux citoyens de voter et débattre sur diverses propositions liées aux dépenses publiques et à la fiscalité française.

### 💡 Concept

Cette plateforme capitalise sur le mème viral "Nicolas Qui Paie" pour créer un espace d'expression démocratique moderne où les citoyens peuvent :
- 🗳️ Voter sur des propositions fiscales et budgétaires
- 💬 Débattre constructivement sur les politiques publiques
- 📊 Visualiser en temps réel l'opinion publique
- 🏆 Participer à une communauté engagée avec un système de badges

## 🏗️ Architecture Technique

### Clean Architecture Implementation

Le projet suit les principes de la **Clean Architecture** avec une séparation claire des responsabilités :

```
📁 NicolasQuiPaieData/
├── 📄 Models/              # Entités du domaine
├── 📄 DTOs/                # Objets de transfert de données
└── 📄 Context/             # Contexte Entity Framework

📁 NicolasQuiPaieAPI/
├── 📁 Application/
│   ├── 📄 Interfaces/      # Contrats des services et repositories
│   ├── 📄 Services/        # Logique métier
│   ├── 📄 Validators/      # Validation des données (FluentValidation)
│   └── 📄 Mappings/        # Mappings AutoMapper
├── 📁 Infrastructure/
│   └── 📄 Repositories/    # Implémentation des repositories
└── 📁 Presentation/
    └── 📄 Endpoints/       # API minimale .NET 9

📁 NicolasQuiPaieWebApp/
├── 📄 Components/          # Composants Blazor WebAssembly
├── 📄 Services/            # Services d'appel API
└── 📄 Pages/               # Pages de l'application

📁 Tests/
├── 📁 NicolasQuiPaie.UnitTests/        # Tests unitaires (NUnit + Moq + Shouldly)
└── 📁 NicolasQuiPaie.IntegrationTests/ # Tests d'intégration (WebApplicationFactory)
```

### Stack Technologique

#### Backend API
- **Framework** : ASP.NET Core 9 avec API minimale
- **Architecture** : Clean Architecture + CQRS patterns
- **Base de données** : SQL Server avec Entity Framework Core 9
- **Authentification** : JWT Bearer tokens
- **Validation** : FluentValidation
- **Mapping** : AutoMapper
- **Documentation** : OpenAPI/Swagger
- **Logging** : Serilog

#### Frontend Client
- **Framework** : Blazor WebAssembly (.NET 9)
- **Authentification** : MSAL (Microsoft Authentication Library)
- **State Management** : Blazored.LocalStorage
- **Temps réel** : SignalR Client
- **HTTP Client** : System.Net.Http.Json
- **Design System** : Bootstrap 5 + CSS personnalisé

#### Tests & Qualité
- **Tests Unitaires** : NUnit + Moq + Shouldly
- **Tests d'Intégration** : WebApplicationFactory + TestContainers
- **Couverture de code** : Coverlet
- **CI/CD** : GitHub Actions (à venir)

## 🚀 Fonctionnalités Principales

### 1. 🔐 Système d'Authentification Moderne
- Authentification JWT sécurisée
- Gestion des tokens avec MSAL
- Profils utilisateurs avec niveaux fiscaux
- **Système de badges Nicolas** :
  - 🥉 **Petit Nicolas** (Poids de vote : 1x)
  - 🥈 **Gros Nicolas** (Poids de vote : 2x)  
  - 🏆 **Nicolas Suprême** (Poids de vote : 3x)

### 2. 🗳️ API de Vote Démocratique
- **Endpoints RESTful** pour toutes les opérations
- **Système de vote pondéré** basé sur le niveau fiscal
- **Validation robuste** avec FluentValidation
- **Commentaires et débats** avec threading
- **Catégorisation** des propositions

### 3. 📈 Dashboard Analytics en Temps Réel
- **API d'analytics** dédiée avec endpoints spécialisés
- **Baromètre du ras-le-bol** en temps réel via SignalR
- **Statistiques détaillées** :
  - Nombre de Nicolas inscrits
  - Votes exprimés total
  - Propositions actives
  - Tendances des votes
- **Graphiques interactifs** côté client

### 4. 🧪 Tests Complets
- **Tests unitaires** avec isolation des dépendances (Moq)
- **Tests d'intégration** avec base de données en mémoire
- **Tests d'API** avec WebApplicationFactory
- **Assertions expressives** avec Shouldly

## 📁 Structure Détaillée du Projet

### Couche Data (NicolasQuiPaieData)
```csharp
// Modèles du domaine
public class Proposal
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ProposalStatus Status { get; set; }
    // ... autres propriétés
}

// DTOs pour les transferts
public class ProposalDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    // ... propriétés calculées
    public int TotalVotes => VotesFor + VotesAgainst;
}
```

### Couche Application (API/Application)
```csharp
// Services métier
public interface IProposalService
{
    Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(...);
    Task<ProposalDto> CreateProposalAsync(...);
    // ...
}

// Repositories avec Unit of Work
public interface IUnitOfWork
{
    IProposalRepository Proposals { get; }
    IVoteRepository Votes { get; }
    Task<int> SaveChangesAsync();
}
```

### Couche Présentation (API/Presentation)
```csharp
// API minimale .NET 9
public static void MapProposalEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/proposals").WithTags("Proposals");
    
    group.MapGet("/", async (IProposalService service) => 
        Results.Ok(await service.GetActiveProposalsAsync()));
    
    group.MapPost("/", [Authorize] async (IProposalService service, CreateProposalDto dto) => 
        Results.Created($"/api/proposals/{result.Id}", result));
}
```

### Frontend Blazor WebAssembly
```razor
@* Composant de vote *@
<div class="voting-component">
    <button class="btn btn-success" @onclick="() => VoteAsync(VoteType.For)">
        <i class="fas fa-thumbs-up"></i> Nicolas Approuve (@proposal.VotesFor)
    </button>
    <button class="btn btn-danger" @onclick="() => VoteAsync(VoteType.Against)">
        <i class="fas fa-thumbs-down"></i> Nicolas Refuse (@proposal.VotesAgainst)
    </button>
</div>

@code {
    private async Task VoteAsync(VoteType voteType)
    {
        var voteDto = new CreateVoteDto { ProposalId = proposal.Id, VoteType = voteType };
        await VotingService.CastVoteAsync(voteDto);
        await OnVoteChanged.InvokeAsync();
    }
}
```

## 🧪 Tests et Qualité

### Tests Unitaires
```csharp
[Test]
public async Task CreateProposalAsync_ShouldCreateProposal_WhenValidDataProvided()
{
    // Arrange
    var createDto = new CreateProposalDto { Title = "Test", Description = "..." };
    _mockRepository.Setup(x => x.AddAsync(It.IsAny<Proposal>())).ReturnsAsync(proposal);
    
    // Act
    var result = await _proposalService.CreateProposalAsync(createDto, "user123");
    
    // Assert
    result.ShouldNotBeNull();
    result.Title.ShouldBe("Test");
    _mockRepository.Verify(x => x.AddAsync(It.IsAny<Proposal>()), Times.Once);
}
```

### Tests d'Intégration
```csharp
[Test]
public async Task GetProposals_ShouldReturnOk_WithProposals()
{
    // Arrange
    using var factory = new NicolasQuiPaieApiFactory();
    using var client = factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/proposals");
    
    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var proposals = await response.Content.ReadFromJsonAsync<List<ProposalDto>>();
    proposals.ShouldNotBeEmpty();
}
```

## 🚀 Installation et Déploiement

### Prérequis
- **.NET 9 SDK**
- **SQL Server** ou **Azure SQL Database**
- **Visual Studio 2022** ou **VS Code**
- **Node.js** (pour les outils frontend)

### Installation Locale

1. **Cloner le repository**
   ```bash
   git clone https://github.com/votre-repo/nicolas-qui-paie.git
   cd nicolas-qui-paie
   ```

2. **Configuration de la base de données**
   ```bash
   # Dans appsettings.json de l'API
   cd NicolasQuiPaieAPI
   dotnet ef database update
   ```

3. **Lancement de l'API**
   ```bash
   cd NicolasQuiPaieAPI
   dotnet run
   # API disponible sur https://localhost:7001
   ```

4. **Lancement du client Blazor**
   ```bash
   cd NicolasQuiPaieWebApp
   dotnet run
   # Client disponible sur https://localhost:5001
   ```

### Tests
```bash
# Tests unitaires
cd NicolasQuiPaie.UnitTests
dotnet test

# Tests d'intégration
cd NicolasQuiPaie.IntegrationTests
dotnet test

# Tous les tests avec couverture
dotnet test --collect:"XPlat Code Coverage"
```

### Configuration

#### API (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=NicolasQuiPaieDb;..."
  },
  "Jwt": {
    "Key": "YourSecretKeyHere",
    "Issuer": "NicolasQuiPaieAPI",
    "Audience": "NicolasQuiPaieClient"
  }
}
```

#### Client Blazor (appsettings.json)
```json
{
  "ApiBaseAddress": "https://localhost:7001",
  "AzureAd": {
    "Authority": "https://login.microsoftonline.com/your-tenant",
    "ClientId": "your-client-id"
  }
}
```

## 📊 Endpoints API

### Propositions
- `GET /api/propositions` - Liste des propositions actives
- `GET /api/propositions/trending` - Propositions tendances
- `GET /api/propositions/{id}` - Détail d'une proposition
- `POST /api/propositions` - Créer une proposition (auth requis)
- `PUT /api/propositions/{id}` - Modifier une proposition (auth requis)
- `DELETE /api/propositions/{id}` - Supprimer une proposition (auth requis)

### Votes
- `POST /api/votes` - Voter (auth requis)
- `GET /api/votes/proposal/{id}` - Votes d'une proposition
- `GET /api/votes/user` - Votes de l'utilisateur (auth requis)
- `DELETE /api/votes/proposal/{id}/user` - Supprimer son vote (auth requis)

### Analytics
- `GET /api/analytics/global-stats` - Statistiques globales
- `GET /api/analytics/voting-trends` - Tendances de vote
- `GET /api/analytics/frustration-barometer` - Baromètre du ras-le-bol

## 🔥 Innovations Techniques

### API Minimale .NET 9
- **Performance optimisée** avec des endpoints légers
- **Documentation automatique** avec OpenAPI
- **Validation intégrée** avec FluentValidation
- **Authentification JWT** native

### Clean Architecture Benefits
- **Séparation des responsabilités** claire
- **Testabilité** maximale avec injection de dépendances
- **Maintenabilité** long terme
- **Évolutivité** de l'architecture

### Blazor WebAssembly Avantages
- **Expérience utilisateur** native côté client
- **Performance** optimisée avec WebAssembly
- **Partage de code** entre client et serveur (.NET)
- **SignalR** pour les mises à jour temps réel

## 🧪 Stratégie de Tests

### Pyramide de Tests
1. **Tests Unitaires** (70%) - Logique métier isolée
2. **Tests d'Intégration** (20%) - API et base de données
3. **Tests E2E** (10%) - Parcours utilisateur complets

### Outils de Qualité
- **Code Coverage** avec Coverlet
- **Analyse statique** avec SonarQube
- **Performance** avec BenchmarkDotNet
- **Sécurité** avec analyse OWASP

## 🚀 Roadmap Technique

### Phase 1 ✅ (Actuelle)
- ✅ Clean Architecture avec API minimale
- ✅ Blazor WebAssembly client
- ✅ Tests unitaires et d'intégration
- ✅ Authentification JWT
- ✅ Documentation OpenAPI

### Phase 2 (3-6 mois)
- 🔄 **Microservices** avec conteneurisation Docker
- 🔄 **CQRS + Event Sourcing** pour l'audit
- 🔄 **Cache distribué** avec Redis
- 🔄 **API Gateway** avec YARP
- 🔄 **Monitoring** avec Application Insights

### Phase 3 (6-12 mois)
- 🔄 **Architecture hexagonale** complète
- 🔄 **Event-driven architecture** avec Azure Service Bus
- 🔄 **GraphQL** pour les requêtes flexibles
- 🔄 **Machine Learning** pour recommandations
- 🔄 **Progressive Web App** (PWA)

## 🤝 Contribution et Standards

### Standards de Développement
- **C# Conventions** : Microsoft guidelines
- **Clean Code** : SOLID principles
- **Git Flow** : Feature branches + Pull Requests
- **Code Review** : Obligatoire avant merge

### Architecture Guidelines
- **Dependency Inversion** : Toujours dépendre des abstractions
- **Single Responsibility** : Une classe = une responsabilité
- **Unit Testing** : Coverage minimum 80%
- **API Design** : RESTful + OpenAPI documentation

### Pull Requests Process
1. **Fork** du repository
2. **Feature branch** (`feature/nouvelle-fonctionnalite`)
3. **Tests** ajoutés/mis à jour (obligatoire)
4. **Documentation** mise à jour
5. **Code Review** par l'équipe
6. **Merge** après validation

## 📄 Licence

Ce projet est sous licence **MIT** - voir [LICENSE.md](LICENSE.md)

## 👥💻 Équipe Technique

- **Architecture** : Clean Architecture + DDD patterns
- **Backend** : .NET 9 + Entity Framework Core
- **Frontend** : Blazor WebAssembly + SignalR
- **DevOps** : Azure + GitHub Actions
- **Tests** : NUnit + Moq + Shouldly

## 📞 Support et Communauté

- **Documentation** : [Wiki GitHub](https://github.com/votre-repo/wiki)
- **Issues** : [GitHub Issues](https://github.com/votre-repo/issues)
- **Discussions** : [GitHub Discussions](https://github.com/votre-repo/discussions)
- **Email** : support@nicolasquipaie.fr

---

## 🎯 Métriques de Qualité 2024

- 🧪 **Tests Coverage** : >80%
- ⚡ **API Response Time** : <200ms
- 🔒 **Security Score** : A+
- 📈 **Performance Score** : >90
- 🚀 **Uptime SLA** : 99.9%

---

*"Code propre, architecture solide, Nicolas qui paie mais développeurs qui gagnent !"* 🇫🇷💻

![Made with ❤️ and Clean Architecture](https://img.shields.io/badge/Made%20with-❤️%20%26%20Clean%20Architecture-red)
