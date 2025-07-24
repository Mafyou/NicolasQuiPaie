using System.ComponentModel.DataAnnotations;

namespace NicolasQuiPaieWeb.Configuration;

public class MaintenanceSettings
{
    public const string SectionName = "MaintenanceSettings";
    
    /// <summary>
    /// Si true, affiche un banner de construction (site accessible)
    /// </summary>
    public bool IsUnderConstruction { get; set; } = false;
    
    /// <summary>
    /// Si true, le site est complètement inaccessible (mode maintenance totale)
    /// </summary>
    public bool IsCompletelyDown { get; set; } = false;
    
    [Required]
    public string ConstructionMessage { get; set; } = "";
    
    /// <summary>
    /// Message affiché quand le site est complètement fermé
    /// </summary>
    public string MaintenancePageMessage { get; set; } = "?? Site temporairement indisponible pour maintenance";
    
    /// <summary>
    /// Titre de la page de maintenance
    /// </summary>
    public string MaintenancePageTitle { get; set; } = "Maintenance en cours";
    
    public string? ExpectedCompletionDate { get; set; }
    
    public bool ShowMaintenanceNotice { get; set; } = false;
    
    public string? MaintenanceMessage { get; set; }
    
    /// <summary>
    /// Message de contact pour la maintenance
    /// </summary>
    public string? ContactMessage { get; set; } = "Pour toute urgence, contactez l'équipe technique.";
}