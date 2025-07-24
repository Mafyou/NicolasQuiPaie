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

## ?? Mode Construction (Banner affich�, site accessible)
```json
{
  "MaintenanceSettings": {
    "IsUnderConstruction": true,
    "IsCompletelyDown": false,
    "ConstructionMessage": "?? Site en cours de construction - Certaines fonctionnalit�s peuvent �tre indisponibles"
  }
}
```

## ?? Mode Maintenance Compl�te (Site ferm�)
```json
{
  "MaintenanceSettings": {
    "IsUnderConstruction": false,
    "IsCompletelyDown": true,
    "MaintenancePageMessage": "?? Site temporairement indisponible pour maintenance\n\nNous effectuons des am�liorations importantes pour vous offrir une meilleure exp�rience. Le site reviendra bient�t !",
    "MaintenancePageTitle": "Maintenance en cours - Nicolas Qui Paie",
    "ExpectedCompletionDate": "2024-12-25T18:00:00",
    "ContactMessage": "Pour toute urgence, contactez l'�quipe technique � admin@nicolasquipaie.fr"
  }
}
```

## ?? Instructions de d�ploiement

### Pour activer la maintenance compl�te :
1. Copiez la configuration "Mode Maintenance Compl�te" ci-dessus
2. Remplacez le contenu de `src/Front/NicolasQuiPaieWeb/wwwroot/appsettings.json`
3. Committez et poussez vers GitHub
4. Azure Static Web Apps red�ploiera automatiquement

### Pour r�activer le site :
1. Copiez la configuration "Mode Normal" ci-dessus
2. Remplacez le contenu de `src/Front/NicolasQuiPaieWeb/wwwroot/appsettings.json`
3. Committez et poussez vers GitHub

## ?? Points importants :
- Seul le fichier `wwwroot/appsettings.json` est utilis� en production pour Blazor WebAssembly
- Le fichier `wwwroot/appsettings.Development.json` garde le site accessible localement
- La page de diagnostic (`/diagnostics`) affiche l'�tat actuel de la maintenance
- La page de maintenance inclut un auto-retry toutes les 5 minutes