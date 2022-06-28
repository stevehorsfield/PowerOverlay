using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace overlay_popup;

public class ApplicationMatcherViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string executablePattern = String.Empty;
    private string windowTitlePattern = String.Empty;

    private bool useRegexForExecutable = false;
    private bool useRegexForWindowTitle = false;

    public bool UseRegexForExecutable
    {
        get { return useRegexForExecutable; }
        set
        {
            useRegexForExecutable = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseRegexForExecutable)));
        }
    }

    public bool UseRegexForWindowTitle
    {
        get { return useRegexForWindowTitle; }
        set
        {
            useRegexForWindowTitle = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseRegexForWindowTitle)));
        }
    }

    public string ExecutablePattern
    {
        get { return executablePattern; }
        set
        {
            executablePattern = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExecutablePattern)));
        }
    }
    public string WindowTitlePattern
    {
        get { return windowTitlePattern; }
        set
        {
            windowTitlePattern = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowTitlePattern)));
        }
    }

    public ApplicationMatcherViewModel Clone()
    {
        return new ApplicationMatcherViewModel
        {
            ExecutablePattern = ExecutablePattern,
            WindowTitlePattern = WindowTitlePattern,
            UseRegexForExecutable = UseRegexForExecutable,
            UseRegexForWindowTitle = UseRegexForWindowTitle,
        };
    }

    private bool? CompareString(string value, string pattern, bool useRegEx)
    {
        if (String.IsNullOrEmpty(pattern)) return null;
        if (String.IsNullOrEmpty(value)) return false; // has pattern but no input

        if (useRegEx)
        {
            return Regex.IsMatch(value, pattern);
        } else
        {
            return value.Equals(pattern, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    private bool? MatchesWindowTitle(IntPtr hwnd)
    {
        var windowTitle = NativeUtils.GetWindowTitle(hwnd);
        return CompareString(windowTitle, WindowTitlePattern, UseRegexForWindowTitle);
    }

    private bool? MatchesExecutable(IntPtr hwnd)
    {
        var executable = NativeUtils.GetWindowProcessMainFilename(hwnd);
        return CompareString(executable, ExecutablePattern, UseRegexForExecutable);
    }

    public bool Matches(IntPtr hwnd)
    {
        bool? matchesWindowTitle = MatchesWindowTitle(hwnd);
        bool? matchesExecutable = MatchesExecutable(hwnd);

        if ( (!matchesWindowTitle.HasValue) && (!matchesExecutable.HasValue))
        {
            return false; // neither property available
        }

        return matchesWindowTitle.GetValueOrDefault(true) && matchesExecutable.GetValueOrDefault(true);
    }
}
