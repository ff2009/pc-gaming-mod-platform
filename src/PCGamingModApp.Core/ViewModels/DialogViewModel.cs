using CommunityToolkit.Mvvm.ComponentModel;

namespace PCGamingModApp.Core.ViewModels;

public partial class DialogViewModel : ViewModelBase
{
    [NotifyPropertyChangedFor(nameof(ContentBlurRadius))]
    [ObservableProperty] private bool _isDialogOpen;

    public double ContentBlurRadius => IsDialogOpen ? 256 : 0;
    
    protected TaskCompletionSource closeTask = new();

    public async Task WaitAsync()
    {
        await closeTask.Task;
    }

    public void Show()
    {
        if (closeTask.Task.IsCompleted)
            closeTask = new TaskCompletionSource();

        IsDialogOpen = true;
    }

    public void Close()
    {
        IsDialogOpen = false;

        closeTask.TrySetResult();
    }
}