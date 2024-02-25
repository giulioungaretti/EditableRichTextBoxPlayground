using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaloniaApplication.Views;

public class CustomTextControl : SelectableTextBlock
{
    public static readonly StyledProperty<ICommand> EditCommandProperty =
        AvaloniaProperty.Register<CustomTextControl, ICommand>(
            "EditCommand");

    public ICommand EditCommand
    {
        get => GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public static readonly StyledProperty<int> IndexProperty =
        AvaloniaProperty.Register<CustomTextControl, int>(nameof(Index), defaultValue: 0);

    /// <summary>
    /// Index should be the position of the clicked inline in the collection
    /// </summary>
    public int Index
    {
        get => GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    public static readonly StyledProperty<bool> EditModeProperty =
        AvaloniaProperty.Register<CustomTextControl, bool>(nameof(EditMode), false);

    public bool EditMode
    {
        get => GetValue(EditModeProperty);
        set => SetValue(EditModeProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(SelectableTextBlock);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        Debug.WriteLine($"Pointer Pressed from:{e.Source}");
        var inlinePositions = new Dictionary<int, Inline>();
        var position = 0;

        //TODO: decide if we need to handle non run inlines, memento, we are using this to find the inline by the closest key
        // and turn it into a textbox, so we can edit it.
        foreach (Inline inline in Inlines)
        {
            Debug.WriteLine($"starting position of inline {position}");
            if (inline is not Run r)
            {
                continue;
            }

            inlinePositions[position] = inline;
            position += r.Text!.Length;
        }

        var clickInfo = e.GetCurrentPoint(this);
        if (clickInfo.Properties.IsLeftButtonPressed)
        {
            var padding = Padding;
            var point = e.GetPosition(this) - new Point(0, 0);
            var hit = TextLayout.HitTestPoint(in point);
            var charHit = hit.CharacterHit;
            var index = charHit.FirstCharacterIndex + charHit.TrailingLength;
            Debug.WriteLine($"hit point {index}");

            // find the inline by the closest key
            // TODO: review semantics, inline are indexed by start position
            var closestKey = inlinePositions.Keys.OrderBy(x => Math.Abs(x - index)).FirstOrDefault(0);
            Debug.WriteLine($"closest key {closestKey}");
            var currentInLine = inlinePositions[closestKey];

            // find the line index and textline
            // TODO: we don't use at the moment
            // var lineIndex = TextLayout.GetLineIndexFromCharacterIndex(index, charHit.TrailingLength > 0);
            // var textLine = TextLayout.TextLines[lineIndex];
            Index = closestKey;
            EditMode = true;
        }

        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        // if (SelectedText.Length != 0 )
        // {
        //     EditMode = false;
        // }
        base.OnPointerMoved(e);
    }


    protected override void OnLostFocus(RoutedEventArgs e)
    {
        // EditCommand.Execute(null);
        base.OnLostFocus(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        Debug.WriteLine($"Pointer released from {e.Source}");
        if (e.Source?.GetType() == typeof(CustomTextControl))
        {
            if (SelectedText.Length != 0)
            {
                // we are selecting text, not editing 
                EditMode = false;
            }
            else
            {
                EditCommand.Execute(null);
            }
        }

        base.OnPointerReleased(e);
    }

    public void asd()
    {
        var collection = this.Inlines;
        var newLineCollection = new InlineCollection();
        foreach (var inline in collection)
        {
            if (inline is not Run r)
            {
                continue;
            }

            var split = r.Text!.Split(Environment.NewLine);
            if (split.Length > 1)
            {
                foreach (var line in split)
                {
                    var run = new Run(line);
                    // { Classes = r.Classes };
                    newLineCollection.Add(run);
                    newLineCollection.Add(new LineBreak());
                }
            }
            else
            {
                newLineCollection.Add(inline);
            }
        }

        collection = newLineCollection;
    }
}