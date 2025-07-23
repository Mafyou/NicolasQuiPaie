# Nicolas Qui Paie - Plateforme de D�mocratie Souveraine Num�rique

![Nicolas Qui Paie](https://img.shields.io/badge/Nicolas-Qui%20Paie-blue) ![.NET 9](https://img.shields.io/badge/.NET-9-purple) ![Blazor](https://img.shields.io/badge/Blazor-Server-green)

## ???? Description

**Nicolas Qui Paie** est une plateforme web innovante de d�mocratie participative num�rique, inspir�e du ph�nom�ne viral fran�ais. Elle permet aux citoyens de voter et d�battre sur diverses propositions li�es aux d�penses publiques et � la fiscalit� fran�aise.

### ? Concept

Cette plateforme capitalise sur le m�me viral "Nicolas Qui Paie" pour cr�er un espace d'expression d�mocratique moderne o� les citoyens peuvent :
- ??? Voter sur des propositions fiscales et budg�taires
- ?? D�battre constructivement sur les politiques publiques
- ?? Visualiser en temps r�el l'opinion publique
- ?? Participer � une communaut� engag�e avec un syst�me de badges

## ??? Architecture Technique

### Backend
- **Framework** : ASP.NET Core Blazor Server (.NET 9)
- **Base de donn�es** : Azure SQL Server / SQL Server LocalDB
- **ORM** : Entity Framework Core
- **Authentification** : ASP.NET Core Identity
- **Temps r�el** : SignalR
- **H�bergement** : Azure Web App

### Frontend
- **UI Framework** : Blazor Server Components
- **Design System** : Bootstrap 5 + CSS personnalis�
- **Th�matique** : Couleurs fran�aises (Bleu, Blanc, Rouge)
- **Graphiques** : Chart.js
- **Icons** : Font Awesome 6
- **Temps r�el** : SignalR Client

## ?? Fonctionnalit�s Principales

### 1. ?? Syst�me d'Authentification Robuste
- Inscription avec validation email
- Connexion s�curis�e
- Profils utilisateurs personnalisables
- **Syst�me de badges Nicolas** :
  - ?? **Petit Nicolas** (Poids de vote : 1x)
  - ?? **Gro Nicolas** (Poids de vote : 2x)  
  - ?? **Nicolas Supr�me** (Poids de vote : 3x)

### 2. ??? Plateforme de Vote D�mocratique
- **Cr�ation de propositions** : Interface intuitive pour soumettre des sujets
- **Syst�me de vote pond�r�** : Votes avec poids selon le niveau fiscal
- **Boutons th�matiques** :
  - ?? "Nicolas Approuve"
  - ?? "Nicolas Refuse"
- **Commentaires et d�bats** : Section de discussion compl�te
- **Tendances** : Sujet qui buzzent le plus

### 3. ?? Dashboard "Nicolas Analytics"
- **Barom�tre du ras-le-bol national** : Indicateur visuel en temps r�el
- **Statistiques d�taill�es** :
  - Nombre de Nicolas inscrits
  - Votes exprim�s total
  - Propositions actives
  - Commentaires et d�bats
- **Graphiques interactifs** :
  - Tendances des votes (7 derniers jours)
  - R�partition des niveaux Nicolas
  - Cat�gories les plus actives
- **Top Contributors** du mois
- **Propositions tendances** (24h)

### 4. ?? Fonctionnalit�s Sociales
- **Profils utilisateurs** complets avec statistiques
- **Syst�me de r�putation** bas� sur la participation
- **Accomplissements et badges** de participation
- **Partage social** avec hashtags #JeSuisNicolas
- **Commentaires imbriqu�s** avec likes

### 5. ??? S�curit� et Mod�ration
- **Protection anti-spam** et limitation de votes
- **Syst�me de signalement** communautaire
- **Mod�ration des commentaires**
- **Authentification s�curis�e**

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
?   ?   ??? ProposalDetail.razor # D�tail d'une proposition
?   ?   ??? CreateProposal.razor # Cr�ation de proposition
?   ?   ??? Profile.razor        # Profil utilisateur
?   ?   ??? Analytics.razor      # Dashboard analytics
?   ?   ??? Error.razor          # Page d'erreur
?   ??? Shared/
?       ??? ProposalCard.razor   # Carte de proposition
?       ??? VotingComponent.razor # Composant de vote
?       ??? CommentCard.razor    # Carte de commentaire
??? Data/
?   ??? ApplicationDbContext.cs  # Contexte EF
?   ??? Models/                  # Mod�les de donn�es
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
    ??? css/app.css             # Styles personnalis�s
    ??? js/charts.js            # Scripts JavaScript
```

## ??? Mod�le de Donn�es

### Tables Principales

```sql
-- Utilisateurs avec niveaux fiscaux
Users (Id, Username, Email, FiscalLevel, ReputationScore, CreatedAt)

-- Propositions de vote
Proposals (Id, Title, Description, CategoryId, CreatedBy, Status, VotesFor, VotesAgainst)

-- Votes pond�r�s
Votes (Id, UserId, ProposalId, VoteType, Weight, VotedAt)

-- Commentaires imbriqu�s
Comments (Id, UserId, ProposalId, Content, ParentCommentId, LikesCount, CreatedAt)

-- Cat�gories organis�es
Categories (Id, Name, Description, IconClass, Color)
```

### Relations
- **1:N** User ? Proposals (un utilisateur peut cr�er plusieurs propositions)
- **1:N** User ? Votes (un utilisateur peut voter sur plusieurs propositions)
- **1:1** User/Proposal ? Vote (contrainte unique par proposition)
- **1:N** Proposal ? Comments (une proposition peut avoir plusieurs commentaires)
- **1:N** Comment ? Comment (commentaires imbriqu�s)

## ?? Design et UI/UX

### Th�matique Visuelle
- **Couleurs principales** : Tricolore fran�ais (#002395, #ffffff, #ed2939)
- **Palette �tendue** : Bootstrap 5 avec personnalisations Nicolas
- **Typographie** : Segoe UI, syst�me fonts
- **Icons** : Font Awesome 6 avec ic�nes th�matiques

### Composants Cl�s
- **Barom�tre du ras-le-bol** : Indicateur visuel avec needle anim�e
- **Cartes de proposition** : Design cards avec hover effects
- **Boutons de vote** : Th�matiques "Nicolas Approuve/Refuse"
- **Badges Nicolas** : Syst�me de niveaux avec �mojis
- **Graphiques** : Chart.js avec th�me Nicolas

### Responsive Design
- **Mobile-first** approach
- **Breakpoints** Bootstrap 5 standards
- **Touch-friendly** interfaces
- **Progressive enhancement**

## ? Fonctionnalit�s Temps R�el

### SignalR Integration
- **Mise � jour des votes** en temps r�el
- **Notifications** de nouveaux commentaires
- **Statistiques live** sur le dashboard
- **Pr�sence utilisateur** (optionnel)

### Groupes SignalR
- `proposal_{id}` : Mises � jour d'une proposition sp�cifique
- `global_updates` : Statistiques globales
- Connexion/d�connexion automatique

## ?? Installation et D�ploiement

### Pr�requis
- **.NET 9 SDK**
- **SQL Server** ou **Azure SQL Database**
- **Visual Studio 2022** ou **VS Code**
- **Azure CLI** (pour d�ploiement)

### Installation Locale
1. **Cloner le repository**
   ```bash
   git clone https://github.com/votre-repo/nicolas-qui-paie.git
   cd nicolas-qui-paie
   ```

2. **Configuration de la base de donn�es**
   ```bash
   # Modifier appsettings.json avec votre connection string
   dotnet ef database update
   ```

3. **Lancement de l'application**
   ```bash
   cd NicolasQuiPaieWeb
   dotnet run
   ```

4. **Acc�s** : https://localhost:5001

### D�ploiement Azure

#### Option 1 : Via Azure CLI
```bash
# Connexion � Azure
az login

# Cr�ation du groupe de ressources
az group create --name NicolasQuiPaieRG --location "France Central"

# D�ploiement de l'ARM template
az deployment group create \
  --resource-group NicolasQuiPaieRG \
  --template-file azure/deploy.json \
  --parameters @azure/parameters.json
```

#### Option 2 : Via GitHub Actions
- Configuration automatique avec secrets Azure
- D�ploiement continu sur push main
- Tests automatis�s inclus

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

### Facteurs de Viralit�
- ? **Tendance existante** : Capitalise sur "Nicolas Qui Paie"
- ? **Besoin r�el** : Expression d�mocratique hors �lections
- ? **Interactivit� forte** : Votes et d�bats temps r�el
- ? **Gamification** : Badges et syst�me de points
- ? **Partage social** : Integration r�seaux sociaux
- ? **Timing parfait** : Contexte politique fran�ais

### Roadmap Future
#### Phase 2 (3-6 mois)
- **API publique** pour apps tierces
- **Notifications push** mobiles
- **G�olocalisation** pour votes r�gionaux
- **P�titions** populaires

#### Phase 3 (6-12 mois)
- **IA pour r�sum�s** de d�bats
- **Mode assembl�e virtuelle** live
- **D�l�gation de vote** num�rique
- **Int�gration donn�es fiscales** r�elles

### Mon�tisation (Optionnelle)
- **Freemium** : Fonctionnalit�s avanc�es payantes
- **Partenariats** : Organisations civiques
- **Consulting** : Solutions pour collectivit�s
- **API premium** : Acc�s donn�es agr�g�es

## ?? Tests et Qualit�

### Tests Unitaires
```bash
dotnet test
```

### Tests d'Int�gration
- **SignalR** : Tests de connexion temps r�el
- **Base de donn�es** : Tests EF Core
- **API** : Tests des endpoints

### Performance
- **Benchmarks** : Temps de r�ponse < 200ms
- **Concurrent users** : Support 1000+ utilisateurs
- **Cache** : Strat�gies de mise en cache

## ?? Contribution

### Standards de Code
- **C# conventions** : Microsoft guidelines
- **Razor** : Clean component structure
- **CSS** : BEM methodology
- **JavaScript** : ES6+ modules

### Pull Requests
1. Fork du repository
2. Feature branch (`feature/nouvelle-fonctionnalite`)
3. Tests ajout�s/mis � jour
4. Documentation mise � jour
5. Pull request avec description d�taill�e

## ?? Licence

Ce projet est sous licence **MIT** - voir [LICENSE.md](LICENSE.md)

## ????? �quipe

- **D�veloppement** : �quipe Nicolas
- **Design** : UX/UI Nicolas
- **DevOps** : Infrastructure Nicolas

## ?? Support

- **Email** : support@nicolasquipaie.fr
- **Discord** : [Serveur Nicolas](https://discord.gg/nicolas)
- **Issues** : [GitHub Issues](https://github.com/votre-repo/issues)

---

## ?? Objectifs 2024

- ?? **10 000** Nicolas inscrits
- ?? **100 000** votes exprim�s
- ?? **1 000** propositions cr��es
- ?? **Viralit�** sur r�seaux sociaux
- ?? **Couverture m�diatique** nationale

---

*"C'est Nicolas qui paie, mais c'est nous tous qui d�cidons !"* ????

![Made with ?? in France](https://img.shields.io/badge/Made%20with-??-red?label=Made%20in&color=blue&logo=france)
