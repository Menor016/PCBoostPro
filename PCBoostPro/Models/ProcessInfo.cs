namespace PCBoostPro.Models;

public class PerformanceSnapshot
{
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double DiskUsagePercent { get; set; }
    public string CpuUsageText => $"{CpuUsagePercent:0.0}%";
    public string MemoryUsageText => $"{MemoryUsagePercent:0.0}%";
    public string DiskUsageText => $"{DiskUsagePercent:0.0}%";
    public string NetworkStatus { get; set; } = "Não disponível";
}
