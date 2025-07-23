# Nicolas Qui Paie - Plateforme de Démocratie Souveraine Numérique

![Nicolas Qui Paie](https://img.shields.io/badge/Nicolas-Qui%20Paie-blue) ![.NET 9](https://img.shields.io/badge/.NET-9-purple) ![Blazor](https://img.shields.io/badge/Blazor-Server-green)

## ???? Description

**Nicolas Qui Paie** est une plateforme web innovante de démocratie participative numérique, inspirée du phénomène viral français. Elle permet aux citoyens de voter et débattre sur diverses propositions liées aux dépenses publiques et à la fiscalité française.

### ? Concept

Cette plateforme capitalise sur le mème viral "Nicolas Qui Paie" pour créer un espace d'expression démocratique moderne où les citoyens peuvent :
- ??? Voter sur des propositions fiscales et budgétaires
- ?? Débattre constructivement sur les politiques publiques
- ?? Visualiser en temps réel l'opinion publique
- ?? Participer à une communauté engagée avec un système de badges

## ??? Architecture Technique

### Backend
- **Framework** : ASP.NET Core Blazor Server (.NET 9)
- **Base de données** : Azure SQL Server / SQL Server LocalDB
- **ORM** : Entity Framework Core
- **Authentification** : ASP.NET Core Identity
- **Temps réel** : SignalR
- **Hébergement** : Azure Web App

### Frontend
- **UI Framework** : Blazor Server Components
- **Design System** : Bootstrap 5 + CSS personnalisé
- **Thématique** : Couleurs françaises (Bleu, Blanc, Rouge)
- **Graphiques** : Chart.js
- **Icons** : Font Awesome 6
- **Temps réel** : SignalR Client

## ?? Fonctionnalités Principales

### 1. ?? Système d'Authentification Robuste
- Inscription avec validation email
- Connexion sécurisée
- Profils utilisateurs personnalisables
- **Système de badges Nicolas** :
  - ?? **Petit Nicolas** (Poids de vote : 1x)
  - ?? **Gro Nicolas** (Poids de vote : 2x)  
  - ?? **Nicolas Suprême** (Poids de vote : 3x)

### 2. ??? Plateforme de Vote Démocratique
- **Création de propositions** : Interface intuitive pour soumettre des sujets
- **Système de vote pondéré** : Votes avec poids selon le niveau fiscal
- **Boutons thématiques** :
  - ?? "Nicolas Approuve"
  - ?? "Nicolas Refuse"
- **Commentaires et débats** : Section de discussion complète
- **Tendances** : Sujet qui buzzent le plus

### 3. ?? Dashboard "Nicolas Analytics"
- **Baromètre du ras-le-bol national** : Indicateur visuel en temps réel
- **Statistiques détaillées** :
  - Nombre de Nicolas inscrits
  - Votes exprimés total
  - Propositions actives
  - Commentaires et débats
- **Graphiques interactifs** :
  - Tendances des votes (7 derniers jours)
  - Répartition des niveaux Nicolas
  - Catégories les plus actives
- **Top Contributors** du mois
- **Propositions tendances** (24h)

### 4. ?? Fonctionnalités Sociales
- **Profils utilisateurs** complets avec statistiques
- **Système de réputation** basé sur la participation
- **Accomplissements et badges** de participation
- **Partage social** avec hashtags #JeSuisNicolas
- **Commentaires imbriqués** avec likes

### 5. ??? Sécurité et Modération
- **Protection anti-spam** et limitation de votes
- **Système de signalement** communautaire
- **Modération des commentaires**
- **Authentification sécurisée**

## ?? Structure du Projet

```
NicolasQuiPaieWeb/
??? Components/
?   ??? Layout/
?   ?   ??? MainLayout.razor      # Layout principal
?   ?   ??? NavMenu.razor         # Navigation
?   ??? Pages/
?   ?   ??? Home.razor           # Page d'accueil
?   ?   ??? Proposals.razor      # Liste des propositions
?   ?   ??? ProposalDetail.razor # Détail d'une proposition
?   ?   ??? CreateProposal.razor # Création de proposition
?   ?   ??? Profile.razor        # Profil utilisateur
?   ?   ??? Analytics.razor      # Dashboard analytics
?   ?   ??? Error.razor          # Page d'erreur
?   ??? Shared/
?       ??? ProposalCard.razor   # Carte de proposition
?       ??? VotingComponent.razor # Composant de vote
?       ??? CommentCard.razor    # Carte de commentaire
??? Data/
?   ??? ApplicationDbContext.cs  # Contexte EF
?   ??? Models/                  # Modèles de données
?       ??? ApplicationUser.cs
?       ??? Proposal.cs
?       ??? Vote.cs
?       ??? Comment.cs
?       ??? Category.cs
??? Services/
?   ??? ProposalService.cs       # Service des propositions
?   ??? VotingService.cs         # Service de vote
?   ??? AnalyticsService.cs      # Service d'analytics
??? Hubs/
?   ??? VotingHub.cs            # Hub SignalR
??? wwwroot/
    ??? css/app.css             # Styles personnalisés
    ??? js/charts.js            # Scripts JavaScript
```

## ??? Modèle de Données

### Tables Principales

```sql
-- Utilisateurs avec niveaux fiscaux
Users (Id, Username, Email, FiscalLevel, ReputationScore, CreatedAt)

-- Propositions de vote
Proposals (Id, Title, Description, CategoryId, CreatedBy, Status, VotesFor, VotesAgainst)

-- Votes pondérés
Votes (Id, UserId, ProposalId, VoteType, Weight, VotedAt)

-- Commentaires imbriqués
Comments (Id, UserId, ProposalId, Content, ParentCommentId, LikesCount, CreatedAt)

-- Catégories organisées
Categories (Id, Name, Description, IconClass, Color)
```

### Relations
- **1:N** User ? Proposals (un utilisateur peut créer plusieurs propositions)
- **1:N** User ? Votes (un utilisateur peut voter sur plusieurs propositions)
- **1:1** User/Proposal ? Vote (contrainte unique par proposition)
- **1:N** Proposal ? Comments (une proposition peut avoir plusieurs commentaires)
- **1:N** Comment ? Comment (commentaires imbriqués)

## ?? Design et UI/UX

### Thématique Visuelle
- **Couleurs principales** : Tricolore français (#002395, #ffffff, #ed2939)
- **Palette étendue** : Bootstrap 5 avec personnalisations Nicolas
- **Typographie** : Segoe UI, système fonts
- **Icons** : Font Awesome 6 avec icônes thématiques

### Composants Clés
- **Baromètre du ras-le-bol** : Indicateur visuel avec needle animée
- **Cartes de proposition** : Design cards avec hover effects
- **Boutons de vote** : Thématiques "Nicolas Approuve/Refuse"
- **Badges Nicolas** : Système de niveaux avec émojis
- **Graphiques** : Chart.js avec thème Nicolas

### Responsive Design
- **Mobile-first** approach
- **Breakpoints** Bootstrap 5 standards
- **Touch-friendly** interfaces
- **Progressive enhancement**

## ? Fonctionnalités Temps Réel

### SignalR Integration
- **Mise à jour des votes** en temps réel
- **Notifications** de nouveaux commentaires
- **Statistiques live** sur le dashboard
- **Présence utilisateur** (optionnel)

### Groupes SignalR
- `proposal_{id}` : Mises à jour d'une proposition spécifique
- `global_updates` : Statistiques globales
- Connexion/déconnexion automatique

## ?? Installation et Déploiement

### Prérequis
- **.NET 9 SDK**
- **SQL Server** ou **Azure SQL Database**
- **Visual Studio 2022** ou **VS Code**
- **Azure CLI** (pour déploiement)

### Installation Locale
1. **Cloner le repository**
   ```bash
   git clone https://github.com/votre-repo/nicolas-qui-paie.git
   cd nicolas-qui-paie
   ```

2. **Configuration de la base de données**
   ```bash
   # Modifier appsettings.json avec votre connection string
   dotnet ef database update
   ```

3. **Lancement de l'application**
   ```bash
   cd NicolasQuiPaieWeb
   dotnet run
   ```

4. **Accès** : https://localhost:5001

### Déploiement Azure

#### Option 1 : Via Azure CLI
```bash
# Connexion à Azure
az login

# Création du groupe de ressources
az group create --name NicolasQuiPaieRG --location "France Central"

# Déploiement de l'ARM template
az deployment group create \
  --resource-group NicolasQuiPaieRG \
  --template-file azure/deploy.json \
  --parameters @azure/parameters.json
```

#### Option 2 : Via GitHub Actions
- Configuration automatique avec secrets Azure
- Déploiement continu sur push main
- Tests automatisés inclus

### Variables d'Environnement
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=NicolasQuiPaieDb;..."
  },
  "Identity": {
    "RequireConfirmedAccount": false
  }
}
```

## ?? Potentiel Viral et Business

### Facteurs de Viralité
- ? **Tendance existante** : Capitalise sur "Nicolas Qui Paie"
- ? **Besoin réel** : Expression démocratique hors élections
- ? **Interactivité forte** : Votes et débats temps réel
- ? **Gamification** : Badges et système de points
- ? **Partage social** : Integration réseaux sociaux
- ? **Timing parfait** : Contexte politique français

### Roadmap Future
#### Phase 2 (3-6 mois)
- **API publique** pour apps tierces
- **Notifications push** mobiles
- **Géolocalisation** pour votes régionaux
- **Pétitions** populaires

#### Phase 3 (6-12 mois)
- **IA pour résumés** de débats
- **Mode assemblée virtuelle** live
- **Délégation de vote** numérique
- **Intégration données fiscales** réelles

### Monétisation (Optionnelle)
- **Freemium** : Fonctionnalités avancées payantes
- **Partenariats** : Organisations civiques
- **Consulting** : Solutions pour collectivités
- **API premium** : Accès données agrégées

## ?? Tests et Qualité

### Tests Unitaires
```bash
dotnet test
```

### Tests d'Intégration
- **SignalR** : Tests de connexion temps réel
- **Base de données** : Tests EF Core
- **API** : Tests des endpoints

### Performance
- **Benchmarks** : Temps de réponse < 200ms
- **Concurrent users** : Support 1000+ utilisateurs
- **Cache** : Stratégies de mise en cache

## ?? Contribution

### Standards de Code
- **C# conventions** : Microsoft guidelines
- **Razor** : Clean component structure
- **CSS** : BEM methodology
- **JavaScript** : ES6+ modules

### Pull Requests
1. Fork du repository
2. Feature branch (`feature/nouvelle-fonctionnalite`)
3. Tests ajoutés/mis à jour
4. Documentation mise à jour
5. Pull request avec description détaillée

## ?? Licence

Ce projet est sous licence **MIT** - voir [LICENSE.md](LICENSE.md)

## ????? Équipe

- **Développement** : Équipe Nicolas
- **Design** : UX/UI Nicolas
- **DevOps** : Infrastructure Nicolas

## ?? Support

- **Email** : support@nicolasquipaie.fr
- **Discord** : [Serveur Nicolas](https://discord.gg/nicolas)
- **Issues** : [GitHub Issues](https://github.com/votre-repo/issues)

---

## ?? Objectifs 2024

- ?? **10 000** Nicolas inscrits
- ?? **100 000** votes exprimés
- ?? **1 000** propositions créées
- ?? **Viralité** sur réseaux sociaux
- ?? **Couverture médiatique** nationale

---

*"C'est Nicolas qui paie, mais c'est nous tous qui décidons !"* ????

![Made with ?? in France](https://img.shields.io/badge/Made%20with-??-red?label=Made%20in&color=blue&logo=france)
