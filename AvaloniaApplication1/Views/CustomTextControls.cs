using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AvaloniaApplication1.Views;

public class CustomTextControl: SelectableTextBlock
{

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
    protected override Type StyleKeyOverride { get { return typeof(SelectableTextBlock); } }
    protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
    {
        var inlinePositions = new Dictionary<int, Inline>();
        int position = 0;

        //TODO: decide if we need to handle non run inlines, memento, we are using this to find the inline by the closest key
        // and turn it into a textbox, so we can edit it.
        foreach (Inline inline in Inlines)
        {
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
           
            // find the inline by the closest key
            // TODO: review semantics, inline are indexed by start position
            var closestKey = inlinePositions.Keys.OrderBy(x => Math.Abs(x - index)).FirstOrDefault(0);
            var currentInLine = inlinePositions[closestKey];

            // find the line index and textline
            // TODO: we don't use at the moment
            // var lineIndex = TextLayout.GetLineIndexFromCharacterIndex(index, charHit.TrailingLength > 0);
            // var textLine = TextLayout.TextLines[lineIndex];
            Index = closestKey;
        }
        base.OnPointerPressed(e);
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
