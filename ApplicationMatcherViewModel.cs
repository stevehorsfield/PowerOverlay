using System;
using System.ComponentModel;

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
}
