using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PowerOverlay;

public static class Sleeper
{

    public static async Task Sleep(int milliseconds)
    {
        if (milliseconds <= 0) return;
        DateTimeOffset now = DateTimeOffset.Now;
        DateTimeOffset complete = now.AddMilliseconds(milliseconds);

        DebugLog.Log($"Sleeping for {milliseconds}ms");

        while (true)
        {
            await Dispatcher.Yield(DispatcherPriority.Background);
            var delta = (int)complete.Subtract(DateTimeOffset.Now).TotalMilliseconds;
            if (delta <= 0) break;
            var sleepMillis = delta > 10 ? 10 : delta;
            await Task.Delay(sleepMillis);
        }
    }
}
