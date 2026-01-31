using System.Diagnostics;

var processesToRun = new[]
{
    ("number generator", @"C:\process\NumberGenerator.cs"),
    ("random word generator", @"C:\process\RandomWordGenerator.cs")
};

List<Process> runningProcesses = [];

var logPath = @"C:\process\processLogs.txt";
var errorPath = @"C:\process\processErrors.txt";

File.WriteAllText(logPath, "STANDARD OUTPUT\n");
File.WriteAllText(errorPath, "ERRORS\n");

foreach (var p in Process.GetProcesses().OrderBy(p => p.ProcessName))
{
    File.AppendAllLines(logPath, new List<string>() { $"{p.Id} - {p.ProcessName}" });
}

object logLock = new();
object errorLock = new();

using var logWriter = new StreamWriter(logPath, append: true) { AutoFlush = true };
using var errorWriter = new StreamWriter(errorPath, append: true) { AutoFlush = true };

foreach (var (label, source) in processesToRun)
{
    var process = CreateProcess(source, label, logWriter, errorWriter, logLock, errorLock);
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    Console.WriteLine($"Started {label} process id = {process.Id}");
    runningProcesses.Add(process);
}

foreach (var process in runningProcesses)
{
    await process.WaitForExitAsync();
}

static Process CreateProcess(
    string source,
    string label,
    StreamWriter logWriter,
    StreamWriter errorWriter,
    object logLock,
    object errorLock)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = $"run \"{source}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };

    var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

    process.OutputDataReceived += (_, e) =>
    {
        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }

        Console.Write(e.Data);

        lock (logLock)
        {
            logWriter.Write(e.Data);
        }
    };

    process.ErrorDataReceived += (_, e) =>
    {
        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }

        var line = $"Error ({label}): {e.Data}";
        Console.WriteLine(line);

        lock (errorLock)
        {
            errorWriter.WriteLine(line);
        }
    };

    return process;
}