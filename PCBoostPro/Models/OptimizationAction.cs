namespace PCBoostPro.Models;

public class OptimizationAction
{
    public string Name { get; set; } = "Ação";
    public string Description { get; set; } = "Sem descrição";
    public string Status { get; set; } = "Não disponível";
    public string StatusColor { get; set; } = "#FBBF24";
    public bool IsAvailable { get; set; }
}
