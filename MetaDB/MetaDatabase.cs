namespace MetaDB;

public sealed class MetaDatabase
{
    private readonly Database db;

    private readonly ConcurrentQueue<(Action<Database>, TaskCompletionSource)> actionQueue = new();

    private bool started = false;

    private readonly Stopwatch timer = new();

    private long timeSinceSaved = 0;

    private bool stop = false;
    private TaskCompletionSource stopped;
    private readonly ManualResetEvent stoppedEvent = new(false);

    public MetaDatabase(string path)
    {
        db = new(path);
    }

    public void Start(int cycleWait, int savePeriod)
    {
        if (started)
        {
            Log("Tried to restart database when running");
            return;
        }
        stopped = new();
        started = true;
        timer.Start();
        while (!stop)
        {
            timeSinceSaved += timer.ElapsedMilliseconds;
            timer.Restart();
            if (timeSinceSaved >= savePeriod)
            {
                timeSinceSaved -= savePeriod;
                db.Save();
            }
            int actionCount = actionQueue.Count;
            for (int i = 0; i < actionCount; i++)
            {
                if (actionQueue.TryDequeue(out (Action<Database>, TaskCompletionSource) action))
                {
                    action.Item1(db);
                    action.Item2?.SetResult();
                }
                else break;
            }
            Thread.Sleep(cycleWait);
        }
        timer.Reset();
        timeSinceSaved = 0;
        stop = false;
        stopped.SetResult();
        stoppedEvent.Set();
        Log("Stopped database");
    }

    public async Task StopAsync()
    {
        if (Stop()) return;
        await stopped.Task;
    }

    public void StopAndBlock()
    {
        if (Stop()) return;
        stoppedEvent.WaitOne();
        stoppedEvent.Reset();
    }

    private bool Stop()
    {
        if (!started)
        {
            Log("Tried to stop database when stopped");
            return true;
        }
        Log("Stopping database");
        started = false;
        stop = true;
        return false;
    }


    public async Task ExecuteAsync(Action<Database> action)
    {
        TaskCompletionSource tcs = new();
        actionQueue.Enqueue((action, tcs));
        await tcs.Task;
    }

    public void QueueExecute(Action<Database> action)
    {
        actionQueue.Enqueue((action, null));
    }

    internal static void Log(string text)
    {
        Console.WriteLine($"[MetaDB] {text}");
    }
}