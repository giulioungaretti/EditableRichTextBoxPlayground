using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaloniaApplication.Views;

public class CustomTextControl : SelectableTextBlock
{
    // allow styling of this control like if it was a SelectableTextBlock
    protected override Type StyleKeyOverride => typeof(SelectableTextBlock);

    #region Styled properties
    public static readonly StyledProperty<ICommand> EditCommandProperty =
        AvaloniaProperty.Register<CustomTextControl, ICommand>(
            nameof(EditCommand));

    public ICommand EditCommand
    {
        get => GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public static readonly StyledProperty<int?> IndexProperty =
        AvaloniaProperty.Register<CustomTextControl, int?>(nameof(Index), defaultValue: 0);

    /// <summary>
    /// Index should be the position of the clicked inline in the collection
    /// </summary>
    public int? Index
    {
        get => GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    #endregion

    #region overrides
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        // TODO: we don't use at the moment
        // find the line index and textline
        // var lineIndex = TextLayout.GetLineIndexFromCharacterIndex(index, charHit.TrailingLength > 0);
        // var textLine = TextLayout.TextLines[lineIndex];
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        Index = getHitIndexFromPointer(e);
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        Index = null;
        base.OnLostFocus(e);
    }
    #endregion

    private int getHitIndexFromPointer(PointerEventArgs e)
    {
        var ogpoint = e.GetPosition(this);
        var point = ogpoint- new Point(0, 0);
        var hit = TextLayout.HitTestPoint(in point);
        var charHit = hit.CharacterHit;
        var index = charHit.FirstCharacterIndex + charHit.TrailingLength;
        return index;
    }
}
