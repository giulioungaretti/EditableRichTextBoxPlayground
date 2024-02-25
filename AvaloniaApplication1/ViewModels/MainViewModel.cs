using Avalonia.Controls;
using Avalonia.Controls.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private int index = 0;

    [ObservableProperty]
    private InlineCollection sources = [
    [ObservableProperty] private InlineCollection sources =
    [
        new Run("Starting run"),

    [RelayCommand]
    private void AddInline()
    {
        Sources.Add(new Run("Added run"));
    [RelayCommand]
    private void AddRun()
    {
        Sources.Add(new Run() { Text = "Run Text " });
        Sources.Add(new LineBreak());
    }
    }

    [RelayCommand]
    private void ClearInlines()
    {
        Sources.Clear();
    }
}