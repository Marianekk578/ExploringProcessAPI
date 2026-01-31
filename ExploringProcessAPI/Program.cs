using System.Diagnostics;

var sourceFile = "C:\\process\\NumberGenerator.cs";

ProcessStartInfo startInfo = new ProcessStartInfo
{
    FileName = "dotnet",
    Arguments = $"run \"{sourceFile}\"",
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false
};

Process process = Process.Start(startInfo);

process.OutputDataReceived += (sender, e) =>
{
    if (!string.IsNullOrEmpty(e.Data))
    {
        Console.Write($"{e.Data}");
    }
};

process.ErrorDataReceived += (sender, e) =>
{
    if (!string.IsNullOrEmpty(e.Data))
    {
        Console.WriteLine($"Error: {e.Data}");
    }
};

process.Start();
process.BeginOutputReadLine();
process.BeginErrorReadLine();
Console.WriteLine($"Started process id = {process.Id}");
await process.WaitForExitAsync();