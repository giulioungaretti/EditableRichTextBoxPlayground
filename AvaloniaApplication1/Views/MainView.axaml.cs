using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media.TextFormatting;
using System;
using System.Runtime.InteropServices.Marshalling;

namespace AvaloniaApplication1.Views;

public partial class MainView : UserControl
{

    public MainView()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TextEditor.asd();
    }
    private void SelectableTextBlock_CopyingToClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }

    private void SelectableTextBlock_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var clickInfo = e.GetCurrentPoint(this);
        if (clickInfo.Properties.IsLeftButtonPressed)
        {
            //var padding = Padding;
            //var point = e.GetPosition(this) - new Point(0, 0);
            //var hit = TextLayout.HitTestPoint(in point );
            //var hit = TextLayout.HitTestPoint(in point );
        }
    }
}
