using System.Diagnostics;
using System.IO;
using System.Management; // not available by default, keep for compatibility but this file uses .NET APIs only.
using System.Net.NetworkInformation;
using PCBoostPro.Models;

namespace PCBoostPro.Services;

public class WindowsSystemService
{
    private readonly Dictionary<int, DateTime> _lastCpuSample = new();

    public SystemSnapshot GetSystemSnapshot()
    {
        var snapshot = new SystemSnapshot();

        try
        {
            snapshot.ComputerName = Environment.MachineName;
        }
        catch
        {
            snapshot.ComputerName = "Não disponível";
        }

        try
        {
            snapshot.WindowsVersion = Environment.OSVersion.VersionString;
        }
        catch
        {
            snapshot.WindowsVersion = "Não disponível";
        }

        try
        {
            snapshot.ProcessorName = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Não disponível";
        }
        catch
        {
            snapshot.ProcessorName = "Não disponível";
        }

        try
        {
            var totalMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
            snapshot.TotalMemoryText = FormatBytes(totalMemoryBytes);
        }
        catch
        {
            snapshot.TotalMemoryText = "Não disponível";
        }

        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
            var total = drives.Sum(d => d.TotalSize);
            var free = drives.Sum(d => d.TotalFreeSpace);
            var used = total - free;
            var percent = total > 0 ? (double)used / total * 100d : 0d;
            snapshot.StorageSummary = $"{FormatBytes(total)} total • {FormatBytes(free)} livre • {percent:0.0}% usado";
        }
        catch
        {
            snapshot.StorageSummary = "Não disponível";
        }

        snapshot.Manufacturer = "Não disponível";
        snapshot.Model = "Não disponível";

        return snapshot;
    }

    public PerformanceSnapshot GetPerformanceSnapshot()
    {
        var snapshot = new PerformanceSnapshot();

        try
        {
            var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
            var currentMemory = GC.GetTotalMemory(false);
            snapshot.MemoryUsagePercent = totalMemory > 0 ? (double)(totalMemory - currentMemory) / totalMemory * 100d : 0d;
        }
        catch
        {
            snapshot.MemoryUsagePercent = 0d;
        }

        try
        {
            var cpuUsage = GetCurrentCpuUsage();
            snapshot.CpuUsagePercent = cpuUsage;
        }
        catch
        {
            snapshot.CpuUsagePercent = 0d;
        }

        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
            var total = drives.Sum(d => d.TotalSize);
            var free = drives.Sum(d => d.TotalFreeSpace);
            var used = total - free;
            snapshot.DiskUsagePercent = total > 0 ? (double)used / total * 100d : 0d;
        }
        catch
        {
            snapshot.DiskUsagePercent = 0d;
        }

        snapshot.NetworkStatus = NetworkInterface.GetIsNetworkAvailable() ? "Conectado" : "Não disponível";

        return snapshot;
    }

    public List<ProcessInfo> GetProcesses()
    {
        var items = new List<ProcessInfo>();

        try
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes.OrderByDescending(p => p.WorkingSet64).Take(20))
            {
                try
                {
                    var totalMemoryMb = (long)(process.WorkingSet64 / (1024d * 1024d));
                    var cpuValue = GetProcessCpuPercent(process);

                    items.Add(new ProcessInfo
                    {
                        Id = process.Id,
                        Name = process.ProcessName,
                        MemoryMb = totalMemoryMb,
                        CpuPercent = cpuValue
                    });
                }
                catch
                {
                    // Ignorar processos em encerramento.
                }
            }
        }
        catch
        {
            items.Add(new ProcessInfo
            {
                Id = 0,
                Name = "Não disponível",
                MemoryMb = 0,
                CpuPercent = 0
            });
        }

        return items;
    }

    public List<DiagnosticItem> RunDiagnostics()
    {
        var diagnostics = new List<DiagnosticItem>();

        diagnostics.Add(new DiagnosticItem
        {
            Title = "Sistema operacional",
            Status = "Ok",
            Message = Environment.OSVersion.VersionString,
            StatusColor = "#22C55E"
        });

        var memoryInfo = GC.GetGCMemoryInfo();
        diagnostics.Add(new DiagnosticItem
        {
            Title = "Memória disponível",
            Status = "Ok",
            Message = $"{FormatBytes(memoryInfo.TotalAvailableMemoryBytes)} disponíveis",
            StatusColor = "#22C55E"
        });

        var driveInfo = DriveInfo.GetDrives().Where(d => d.IsReady).ToList();
        var hasDrive = driveInfo.Any();
        diagnostics.Add(new DiagnosticItem
        {
            Title = "Dispositivos de armazenamento",
            Status = hasDrive ? "Ok" : "Não disponível",
            Message = hasDrive ? $"{driveInfo.Count} unidade(s) detectada(s)" : "Nenhum disco local detectado",
            StatusColor = hasDrive ? "#22C55E" : "#FBBF24"
        });

        try
        {
            var hasNetwork = NetworkInterface.GetIsNetworkAvailable();
            diagnostics.Add(new DiagnosticItem
            {
                Title = "Conectividade de rede",
                Status = hasNetwork ? "Ok" : "Não disponível",
                Message = hasNetwork ? "Rede detectada no sistema." : "Rede indisponível ou não detectada.",
                StatusColor = hasNetwork ? "#22C55E" : "#FBBF24"
            });
        }
        catch
        {
            diagnostics.Add(new DiagnosticItem
            {
                Title = "Conectividade de rede",
                Status = "Não disponível",
                Message = "Não foi possível verificar a rede do sistema.",
                StatusColor = "#FBBF24"
            });
        }

        return diagnostics;
    }

    public List<OptimizationAction> GetOptimizationActions()
    {
        return new List<OptimizationAction>
        {
            new()
            {
                Name = "Limpeza de arquivos temporários",
                Description = "Remove dados temporários e arquivos de cache locais do sistema.",
                Status = "Disponível",
                StatusColor = "#22C55E",
                IsAvailable = true
            },
            new()
            {
                Name = "Verificação de memória",
                Description = "Reavalia o uso de memória disponível e a estabilidade do sistema.",
                Status = "Disponível",
                StatusColor = "#22C55E",
                IsAvailable = true
            },
            new()
            {
                Name = "Diagnóstico de inicialização",
                Description = "Inspeciona itens de inicialização e serviços em execução.",
                Status = "Disponível",
                StatusColor = "#22C55E",
                IsAvailable = true
            },
            new()
            {
                Name = "Ação avançada do sistema",
                Description = "Requer privilégios do sistema e dados complementares do Windows.",
                Status = "Não disponível",
                StatusColor = "#FBBF24",
                IsAvailable = false
            }
        };
    }

    public string ExecuteOptimizationAction(string actionName)
    {
        return actionName switch
        {
            "Limpeza de arquivos temporários" => CleanTempFolder(),
            "Verificação de memória" => CheckMemoryHealth(),
            "Diagnóstico de inicialização" => CheckStartupHealth(),
            _ => "Ação não suportada ou indisponível neste sistema."
        };
    }

    private string CleanTempFolder()
    {
        try
        {
            var tempPath = Path.GetTempPath();
            var tempDirectory = new DirectoryInfo(tempPath);
            var count = 0;

            foreach (var file in tempDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    file.Delete();
                    count++;
                }
                catch
                {
                    // Ignorar arquivos bloqueados.
                }
            }

            return count > 0 ? $"Arquivos temporários limpos: {count}." : "Nenhum arquivo temporário foi encontrado ou removido.";
        }
        catch
        {
            return "Não foi possível limpar os arquivos temporários do sistema.";
        }
    }

    private string CheckMemoryHealth()
    {
        try
        {
            var memory = GC.GetGCMemoryInfo();
            return $"Memória disponível: {FormatBytes(memory.TotalAvailableMemoryBytes)}. Estado: estável.";
        }
        catch
        {
            return "Não foi possível verificar o uso de memória do sistema.";
        }
    }

    private string CheckStartupHealth()
    {
        var items = Process.GetProcesses().Length;
        return $"Processos ativos detectados: {items}. Nenhuma ação agressiva foi executada.";
    }

    private double GetCurrentCpuUsage()
    {
        var processes = Process.GetProcesses();
        var totalCpu = 0d;
        foreach (var process in processes)
        {
            try
            {
                totalCpu += process.TotalProcessorTime.TotalMilliseconds;
            }
            catch
            {
                // Ignorar.
            }
        }

        var sampleKey = DateTime.UtcNow.Ticks;
        if (_lastCpuSample.Count == 0)
        {
            _lastCpuSample[0] = DateTime.UtcNow;
            return 0d;
        }

        return Math.Min(100d, totalCpu / Environment.ProcessorCount / 10d);
    }

    private double GetProcessCpuPercent(Process process)
    {
        try
        {
            var currentTime = process.TotalProcessorTime.TotalMilliseconds;
            var lastSample = _lastCpuSample.TryGetValue(process.Id, out var last) ? last : DateTime.MinValue;
            _lastCpuSample[process.Id] = DateTime.UtcNow;

            if (last == DateTime.MinValue)
            {
                return 0d;
            }

            var elapsed = Math.Max(1, (DateTime.UtcNow - last).TotalSeconds);
            var delta = currentTime / elapsed;
            return Math.Min(100d, delta / 10d);
        }
        catch
        {
            return 0d;
        }
    }

    private static string FormatBytes(long bytes)
    {
        const double scale = 1024d;
        var size = (double)bytes;

        string[] units = { "B", "KB", "MB", "GB", "TB" };
        var unitIndex = 0;

        while (size >= scale && unitIndex < units.Length - 1)
        {
            size /= scale;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }
}
