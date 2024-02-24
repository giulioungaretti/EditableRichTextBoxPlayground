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
        new Run("Starting run"),
        // TODO: having this inline container here breaks adding new inlines
        new InlineUIContainer(new TextBox() { Text="Input", BorderThickness = Avalonia.Thickness.Parse("0"), }),
        ];

    [RelayCommand]
    private void AddInline()
    {
        Sources.Add(new Run("Added run"));
        Sources.Add(new LineBreak());
        // TODO: having this inline container here breaks adding new inlines
        Sources.Add(new TextBox() { Text = "Input", BorderThickness = Avalonia.Thickness.Parse("0"), });
    }

    [RelayCommand]
    private void ClearInlines()
    {
        //TODO: this PR  should fix ths https://github.com/AvaloniaUI/Avalonia/pull/14247
        Sources.Clear();
    }
}
