using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private int index = 0;

    [ObservableProperty] private bool editing = false;

    [ObservableProperty] private InlineCollection sources =
    [
        new Run("Starting run"),
    ];

    [RelayCommand]
    private void AddInline()
    {
        Sources.Add(new TextBox()
        {
            Text = "Input", BorderThickness = Avalonia.Thickness.Parse("0"), Margin = Avalonia.Thickness.Parse("0"),
            Padding = Avalonia.Thickness.Parse("0"),
        });
        Sources.Add(new LineBreak());
    }

    [RelayCommand]
    private void AddRun()
    {
        Sources.Add(new Run() { Text = "Run Text " });
        Sources.Add(new LineBreak());
    }

    [RelayCommand]
    private void Edit()
    {
        InlineCollection newSources = [];
        int i = 0;
        foreach (Inline inline in Sources)
        {
            if (i == Index)
            {
                if (inline is Run r)
                {
                    // transform run into textbox    
                    var textBox = new TextBox()
                    {
                        Text = r.Text,
                        BorderThickness = Thickness.Parse("5"),
                        Margin = Thickness.Parse("0"),
                        Padding = Thickness.Parse("0"),
                    };
                    newSources.Add(textBox);
                }
                else if (inline is InlineUIContainer inlineUiContainer)
                {
                    if (inlineUiContainer.Child is TextBox t)
                    {
                        newSources.Add(new Run(t.Text));
                    }
                    //TODO: handle all the other cases here of course
                }
                else
                {
                    // if the inline is not a text run just add it, we can't 
                    // turn it into a textbox directly
                    newSources.Add(inline);
                }
            }
            else
            {
                // add all the other inlines
                newSources.Add(inline);
            }
            i++;
        }

        Sources = newSources;
    }

    [RelayCommand]
    private void ClearInlines()
    {
        Sources.Clear();
    }
}