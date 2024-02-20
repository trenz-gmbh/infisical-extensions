namespace Example;

public class Worker(IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"Worker running at: {DateTime.Now}");

            PrintRecursively(configuration);

            await Task.Delay(5000, stoppingToken);
        }
    }

    private static void PrintRecursively(IConfiguration section, int indent = 0)
    {
        var children = section.GetChildren().ToList();
        foreach (var child in children)
        {
            Console.WriteLine($"{new string('\t', indent)}[{child.Key}]: {child.Value}");

            PrintRecursively(child, indent + 1);
        }
    }
}
