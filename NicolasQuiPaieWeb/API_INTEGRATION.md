# Configuration API - Nicolas Qui Paie

Ce document explique comment le projet NicolasQuiPaieWeb est configuré pour utiliser l'API externe avec des URLs différentes selon l'environnement.

## Configuration automatique par environnement

### ?? Développement Local
- **URL API**: `https://localhost:7398`
- **Fichier**: `appsettings.Development.json`
- **Usage**: Quand vous lancez l'application avec `dotnet run` ou Visual Studio en mode Debug

### ?? Production
- **URL API**: `https://nicolasquipaieapi-a6behmeyaahpc7c0.francecentral-01.azurewebsites.net`
- **Fichier**: `appsettings.Production.json`
- **Usage**: Quand l'application est déployée avec `ASPNETCORE_ENVIRONMENT=Production`

## Services API disponibles

### `ApiProposalService`
Service HTTP pour les propositions avec méthodes :
- `GetActiveProposalsAsync()` - Récupère les propositions actives
- `GetTrendingProposalsAsync()` - Récupère les propositions tendances
- `GetProposalDtoByIdAsync(id)` - Récupère une proposition par ID
- `CreateProposalAsync(dto)` - Crée une nouvelle proposition
- `IncrementViewsAsync(id)` - Incrémente les vues d'une proposition
- `GetCategoriesAsync()` - Récupère les catégories

### `ApiVotingService`
Service HTTP pour les votes avec méthodes :
- `CastVoteAsync(voteDto)` - Soumet un vote
- `GetProposalVotesAsync(proposalId)` - Récupère les votes d'une proposition
- `GetUserVotesAsync()` - Récupère les votes de l'utilisateur
- `DeleteUserVoteAsync(proposalId)` - Supprime le vote de l'utilisateur

## Utilisation dans vos composants Blazor

### 1. Injection des services
```razor
@inject ApiProposalService ApiProposalService
@inject ApiVotingService ApiVotingService
```

### 2. Exemple d'utilisation des propositions
```csharp
private async Task LoadProposals()
{
    try
    {
        var proposals = await ApiProposalService.GetActiveProposalsAsync(0, 20);
        // Traiter les propositions...
    }
    catch (Exception ex)
    {
        // Gérer l'erreur...
    }
}
```

### 3. Exemple d'utilisation des votes
```csharp
private async Task VoteFor(int proposalId)
{
    var voteDto = new CreateVoteApiDto
    {
        ProposalId = proposalId,
        VoteType = VoteType.For
    };
    
    var success = await ApiVotingService.CastVoteAsync(voteDto);
    if (success)
    {
        // Vote réussi...
    }
}
```

## Page de test

Une page de démonstration est disponible à `/api-example` qui montre :
- Comment charger des propositions depuis l'API
- Comment soumettre des votes via l'API
- La configuration automatique des URLs selon l'environnement

## Fallback vers les services locaux

Les services locaux (`ProposalService` et `VotingService`) restent disponibles et configurés pour une compatibilité descendante. Vous pouvez utiliser les deux approches :

- **Services API** (`ApiProposalService`, `ApiVotingService`) pour les nouvelles fonctionnalités
- **Services locaux** (`ProposalService`, `VotingService`) pour le code existant

## Configuration avancée

Si vous voulez personnaliser l'URL de l'API, modifiez la section `ApiSettings` dans les fichiers appsettings :

```json
{
  "ApiSettings": {
    "BaseUrl": "https://votre-api-personnalisee.com"
  }
}
```

## Gestion des erreurs

Les services API incluent une gestion d'erreurs robuste :
- Logs automatiques des erreurs
- Retour de collections vides en cas d'échec
- Timeout configuré à 30 secondes
- En-têtes HTTP personnalisés pour l'identification

## Authentification

?? **Important**: Les services API ne gèrent pas encore l'authentification JWT. 
Pour les endpoints protégés, vous devrez ajouter l'authentification Bearer token.

## Support

Pour toute question sur l'intégration API, consultez :
- Page de test : `/api-example`
- Logs de l'application pour les erreurs de connectivité
- Configuration dans `Program.cs`