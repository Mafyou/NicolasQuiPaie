# Configuration pour Nicolas Qui Paie - Gestion de la maintenance

## ?? Mode Normal (Site accessible)
```json
{
  "MaintenanceSettings": {
    "IsUnderConstruction": false,
    "IsCompletelyDown": false
  }
}
```

## ?? Mode Construction (Banner affiché, site accessible)
```json
{
  "MaintenanceSettings": {
    "IsUnderConstruction": true,
    "IsCompletelyDown": false,
    "ConstructionMessage": "?? Site en cours de construction - Certaines fonctionnalités peuvent être indisponibles"
  }
}
```

## ?? Mode Maintenance Complète (Site fermé)
```json
{
  "MaintenanceSettings": {
    "IsUnderConstruction": false,
    "IsCompletelyDown": true,
    "MaintenancePageMessage": "?? Site temporairement indisponible pour maintenance\n\nNous effectuons des améliorations importantes pour vous offrir une meilleure expérience. Le site reviendra bientôt !",
    "MaintenancePageTitle": "Maintenance en cours - Nicolas Qui Paie",
    "ExpectedCompletionDate": "2024-12-25T18:00:00",
    "ContactMessage": "Pour toute urgence, contactez l'équipe technique à admin@nicolasquipaie.fr"
  }
}
```

## ?? Instructions de déploiement

### Pour activer la maintenance complète :
1. Copiez la configuration "Mode Maintenance Complète" ci-dessus
2. Remplacez le contenu de `src/Front/NicolasQuiPaieWeb/wwwroot/appsettings.json`
3. Committez et poussez vers GitHub
4. Azure Static Web Apps redéploiera automatiquement

### Pour réactiver le site :
1. Copiez la configuration "Mode Normal" ci-dessus
2. Remplacez le contenu de `src/Front/NicolasQuiPaieWeb/wwwroot/appsettings.json`
3. Committez et poussez vers GitHub

## ?? Points importants :
- Seul le fichier `wwwroot/appsettings.json` est utilisé en production pour Blazor WebAssembly
- Le fichier `wwwroot/appsettings.Development.json` garde le site accessible localement
- La page de diagnostic (`/diagnostics`) affiche l'état actuel de la maintenance
- La page de maintenance inclut un auto-retry toutes les 5 minutes