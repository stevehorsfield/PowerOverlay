using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PowerOverlay;

public class DebugLog
{
    public ObservableCollection<string> LogEntries { get; set; }

    public DebugLog()
    {
        LogEntries = new ObservableCollection<string>();
    }

    private void log(ReadOnlySpan<char> text)
    {
        LogEntries.Add(text.ToString());
    }

    public static void Log(ReadOnlySpan<char> text)
    {
        ((App)App.Current).DebugLog.log($"{System.DateTime.Now.ToString("HH:mm:ss.ffff")} {text}");
    }

    public void Clear()
    {
        LogEntries.Clear();
    }

    public void CopyToClipboard()
    {
        System.Windows.Clipboard.SetText(String.Join(System.Environment.NewLine, LogEntries));
    }
}
