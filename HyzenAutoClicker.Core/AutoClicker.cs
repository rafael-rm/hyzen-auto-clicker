namespace HyzenAutoClicker.Core;

public class AutoClicker(int initialCps, int initialJitterPercentage, bool simulateHumanBehavior)
{
    private static readonly Random Random = new();
    private int _clickDelay = 1000 / initialCps;
    private int _jitterPercentage = initialJitterPercentage;
    private bool _simulateHumanBehavior = simulateHumanBehavior;
    private CancellationTokenSource _cancellationTokenSource;

    public void SetCps(int cps)
    {
        _clickDelay = 1000 / cps;
    }

    public void SetJitter(int jitterPercentage)
    {
        _jitterPercentage = jitterPercentage;
    }
    
    public void SetSimulateHumanBehavior(bool simulateHumanBehavior)
    {
        _simulateHumanBehavior = simulateHumanBehavior;
    }
    
    public bool IsSimulatingHumanBehavior()
    {
        return _simulateHumanBehavior;
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
            if (_simulateHumanBehavior)
                await SimulateHumanBehavior(token);

            double baseDelay = _clickDelay;
            var jitter = Random.NextDouble() * (_clickDelay * _jitterPercentage / 100.0);
            var randomDelay = (int)(baseDelay + jitter);

            clickAction();
            await Task.Delay(randomDelay, token);
        }
    }
    
    private async Task SimulateHumanBehavior(CancellationToken token)
    {
        var pauseChance = Random.NextDouble();
        const double pauseProbability = 0.03;

        if (pauseChance < pauseProbability)
        {
            var pauseDuration = Random.Next(100, 500);
            await Task.Delay(pauseDuration, token);
        }
    }
}