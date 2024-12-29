namespace HyzenAutoClicker.Core;

public class AutoClicker(int initialCps, int initialJitterPercentage)
{
    private static readonly Random Random = new();
    private int _clickDelay = 1000 / initialCps;
    private int _jitterPercentage = initialJitterPercentage;
    private CancellationTokenSource? _cancellationTokenSource;

    public void SetCps(int cps)
    {
        _clickDelay = 1000 / cps;
    }

    public void SetJitter(int jitterPercentage)
    {
        _jitterPercentage = jitterPercentage;
    }

    public void Start(Action clickAction)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        Task.Run(() => ClickLoop(token, clickAction), token);
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }

    private async Task ClickLoop(CancellationToken token, Action clickAction)
    {
        while (!token.IsCancellationRequested)
        {
            
            var pauseChance = Random.NextDouble();
            var pauseProbability = 0.01;

            if (pauseChance < pauseProbability)
            {
                var pauseDuration = Random.Next(100, 500);
                await Task.Delay(pauseDuration, token);
            }

            double baseDelay = _clickDelay;
            var jitter = Random.NextDouble() * (_clickDelay * _jitterPercentage / 100.0);
            var randomDelay = (int)(baseDelay + jitter);

            clickAction();
            await Task.Delay(randomDelay, token);
        }
    }
}