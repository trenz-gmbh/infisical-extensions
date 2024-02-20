namespace Example;

public class Worker(IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"Worker running at: {DateTime.Now}");

            var foo = configuration["FOO"];

            Console.WriteLine($"FOO={foo}");

            await Task.Delay(1000, stoppingToken);
        }
    }
}
