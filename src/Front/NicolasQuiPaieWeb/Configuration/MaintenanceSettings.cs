using System.ComponentModel.DataAnnotations;

namespace NicolasQuiPaieWeb.Configuration;

public class MaintenanceSettings
{
    public const string SectionName = "MaintenanceSettings";
    
    public bool IsUnderConstruction { get; set; } = false;
    
    [Required]
    public string ConstructionMessage { get; set; } = "";
    
    public string? ExpectedCompletionDate { get; set; }
    
    public bool ShowMaintenanceNotice { get; set; } = false;
    
    public string? MaintenanceMessage { get; set; }
}