namespace PCBoostPro.Models;

public class ProcessInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "Não disponível";
    public long MemoryMb { get; set; }
    public string MemoryMbText => MemoryMb > 0 ? $"{MemoryMb} MB" : "Não disponível";
    public double CpuPercent { get; set; }
    public string CpuText => $"{CpuPercent:0.0}%";
}
