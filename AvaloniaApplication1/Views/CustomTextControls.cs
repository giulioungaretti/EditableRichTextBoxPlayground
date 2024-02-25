using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AvaloniaApplication.Views;

public class CustomTextControl : SelectableTextBlock
{
    private void computeInlineMap () {
        var position = 0;
        // we should create a mapping of where each inline starts 
        foreach (Inline inline in Inlines)
        {
            Debug.WriteLine($"starting position of inline {position}");
            inlinePositions[position] = inline;
            if (inline is not Run r)
            {
                // TODO:is this true? Do they occpuy just one char in the text layout algo?
                position += 1;
                continue;
            }
            position += r.Text!.Length;
        }
    }

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
    
    /// <summary>
    /// hit StyledProperty definition
    /// indicates where the cursor is hitting the text.
    /// </summary>
    public static readonly StyledProperty<int> HitProperty =
        AvaloniaProperty.Register<CustomTextControl, int>(nameof(Hit));
    
    /// <summary>
    /// Gets or sets the hit property. This StyledProperty
    /// indicates where the cursor is hitting the text.
    /// </summary>
    public int Hit
    {
       get => this.GetValue(HitProperty);
       set => SetValue(HitProperty, value);
    }
    

    public static readonly StyledProperty<bool> EditModeProperty =
        AvaloniaProperty.Register<CustomTextControl, bool>(nameof(EditMode), false);

    public bool EditMode
    {
        get => GetValue(EditModeProperty);
        set => SetValue(EditModeProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(SelectableTextBlock);

    private Dictionary<int, Inline> inlinePositions = [];
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        Debug.WriteLine($"Pointer Pressed from:{e.Source}");
        computeInlineMap();

        var clickInfo = e.GetCurrentPoint(this);
        if (clickInfo.Properties.IsLeftButtonPressed)
        {
            var padding = Padding;
            var index = getHitIndexFromPointer(e);
            Debug.WriteLine($"hit point {index}");

            var currentInLine = getClostestInline(index, inlinePositions);

            // find the line index and textline
            // TODO: we don't use at the moment
            // var lineIndex = TextLayout.GetLineIndexFromCharacterIndex(index, charHit.TrailingLength > 0);
            // var textLine = TextLayout.TextLines[lineIndex];
            // // Index = closestKey;
            // EditMode = true;
        }

        base.OnPointerPressed(e);
    }

    
    private int getHitIndexFromPointer (PointerEventArgs e){
        var point = e.GetPosition(this) - new Point(0, 0);
        var hit = TextLayout.HitTestPoint(in point);
        var charHit = hit.CharacterHit;
        var index = charHit.FirstCharacterIndex + charHit.TrailingLength;
        return index;
    }
    
    private Inline getClostestInline(int index, Dictionary<int, Inline> inlines){   
        var closestKey = inlinePositions.Keys.OrderBy(x => Math.Abs(x - index)).FirstOrDefault(0);
        Debug.WriteLine($"closest key {closestKey}");
        Index = closestKey;
        return inlinePositions[closestKey];
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var index = getHitIndexFromPointer(e);
        computeInlineMap();
        var inline =  getClostestInline(index, inlinePositions);
        if ( inline is Run r){
            r.TextDecorations = Avalonia.Media.TextDecorations.Underline;
        };
        // get inline under the cursor ish.
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
                //EditCommand.Execute(null);
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