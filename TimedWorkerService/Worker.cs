using System.Globalization;
using System.Security.Cryptography;

namespace TimedWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            // Console.WriteLine("Processing....");
            // using (var progress = new ProgressBar()) {
            //     for (var i = 0.0; i <= 100; i=i+0.02) {
            //         progress.Report((double) i / 100);
            //         Thread.Sleep(2);
            //     }
            // }
            // Console.WriteLine("Done.");

            Random random = new Random();
            Console.WriteLine(new string('-', 72));
            Console.WriteLine(new string('=', 80));
            PrintProgress(1.0);
            PrintProgress(0.3142);
            PrintProgress(0.03142);
            PrintProgress(0.0);
            PrintProgress(random.NextDouble());
            Console.WriteLine(new string('=', 80));
            Console.WriteLine(new string('-', 72));

            await Task.Delay(100, stoppingToken);
        }
    }

    private static void PrintProgress(double currentProgress = 0.0, int totalBlockCount = 70)
    {
        if (double.IsNegative(currentProgress))
            throw new NotSupportedException("Progress value cannot have negative value.");

        var completedBlockCount = Convert.ToInt32(currentProgress * totalBlockCount);

        if (totalBlockCount - completedBlockCount < 0)
            throw new NotSupportedException("Progress block count cannot have negative value.");

        var percent = Convert.ToDouble(currentProgress * 100);

        var progressBarText =
            // $"❴{new string('█', completedBlockCount)}{new string('▁', totalBlockCount - completedBlockCount)}❵" +
            // $" {String.Format("{0,6:0.00}", percent)}%";
            $"▕{new string('█', completedBlockCount)}{new string('▁', totalBlockCount - completedBlockCount)}▏" +
            $" {percent.ToString("0.##", CultureInfo.InvariantCulture).PadLeft(5,' ')} %";

        Console.WriteLine(progressBarText);

    }
}
