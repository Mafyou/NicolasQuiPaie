# Configuration API - Nicolas Qui Paie

Ce document explique comment le projet NicolasQuiPaieWeb est configur� pour utiliser l'API externe avec des URLs diff�rentes selon l'environnement.

## Configuration automatique par environnement

### ?? D�veloppement Local
- **URL API**: `https://localhost:7398`
- **Fichier**: `appsettings.Development.json`
- **Usage**: Quand vous lancez l'application avec `dotnet run` ou Visual Studio en mode Debug

### ?? Production
- **URL API**: `https://nicolasquipaieapi-a6behmeyaahpc7c0.francecentral-01.azurewebsites.net`
- **Fichier**: `appsettings.Production.json`
- **Usage**: Quand l'application est d�ploy�e avec `ASPNETCORE_ENVIRONMENT=Production`

## Services API disponibles

### `ApiProposalService`
Service HTTP pour les propositions avec m�thodes :
- `GetActiveProposalsAsync()` - R�cup�re les propositions actives
- `GetTrendingProposalsAsync()` - R�cup�re les propositions tendances
- `GetProposalDtoByIdAsync(id)` - R�cup�re une proposition par ID
- `CreateProposalAsync(dto)` - Cr�e une nouvelle proposition
- `IncrementViewsAsync(id)` - Incr�mente les vues d'une proposition
- `GetCategoriesAsync()` - R�cup�re les cat�gories

### `ApiVotingService`
Service HTTP pour les votes avec m�thodes :
- `CastVoteAsync(voteDto)` - Soumet un vote
- `GetProposalVotesAsync(proposalId)` - R�cup�re les votes d'une proposition
- `GetUserVotesAsync()` - R�cup�re les votes de l'utilisateur
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
        // G�rer l'erreur...
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
        // Vote r�ussi...
    }
}
```

## Page de test

Une page de d�monstration est disponible � `/api-example` qui montre :
- Comment charger des propositions depuis l'API
- Comment soumettre des votes via l'API
- La configuration automatique des URLs selon l'environnement

## Fallback vers les services locaux

Les services locaux (`ProposalService` et `VotingService`) restent disponibles et configur�s pour une compatibilit� descendante. Vous pouvez utiliser les deux approches :

- **Services API** (`ApiProposalService`, `ApiVotingService`) pour les nouvelles fonctionnalit�s
- **Services locaux** (`ProposalService`, `VotingService`) pour le code existant

## Configuration avanc�e

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
- Retour de collections vides en cas d'�chec
- Timeout configur� � 30 secondes
- En-t�tes HTTP personnalis�s pour l'identification

## Authentification

?? **Important**: Les services API ne g�rent pas encore l'authentification JWT. 
Pour les endpoints prot�g�s, vous devrez ajouter l'authentification Bearer token.

## Support

Pour toute question sur l'int�gration API, consultez :
- Page de test : `/api-example`
- Logs de l'application pour les erreurs de connectivit�
- Configuration dans `Program.cs`