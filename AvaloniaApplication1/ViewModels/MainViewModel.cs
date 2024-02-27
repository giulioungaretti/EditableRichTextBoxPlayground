using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntervalTree;

namespace AvaloniaApplication1.ViewModels;

public partial class MainViewModel : ViewModelBase

{
    private bool _debug_mode = true;

    [ObservableProperty] private int? _index;

    private IntervalTree<int,Inline> _inlineMap = [];

    private InlineCollection _sources =
    [
        new Run("First run"),
        new Run("Second run"),
        new Run("Third run"),
    ];

    public InlineCollection Sources
    {
        get => _sources;
        set => SetProperty(ref _sources, value);
    }

    public MainViewModel()
    {
        _inlineMap = computeInlineMap(_sources);
        Sources.CollectionChanged += Sources_CollectionChanged;
    }

    private void Sources_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        //var newItems = e.NewItems?.Cast<Inline>();
        //if (newItems is not null)
        //{
        //    // TODO: handle remove
        //    try
        //    {
        //        var newInline = newItems.Single();
        //        switch (e.OldStartingIndex)
        //        {
        //            // this  is the last item
        //            case -1:
        //                // get the item in the tree with the max starting point
        //                var maxItem = _inlineMap.MaxBy((inline) => inline.From);
        //                // check that it's actually a new one and not the last one
        //                if (maxItem.Value == newInline)
        //                {
        //                    break;
        //                }
        //                addInlineToTree(maxItem.To, newInline, _inlineMap);
        //                break;
        //            default:
        //                break;
        //        }

        //    }
        //    catch (InvalidOperationException)
        //    {
        //        Debug.WriteLine("Multiple Items added to the sources");
        //    }
        //}
    }

    private static int addInlineToTree(int position, Inline inline, IntervalTree<int, Inline> tree)
    {
        int startPosition = position;
        if (inline is not Run r)
        {
            // TODO:is this true? Do they occupy just one char in the text layout algo?
            position += 1;
        }
        else
        {
            position += r.Text!.Length;
        }
        tree.Add(startPosition, position, inline);
        return position;
    }
    private static IntervalTree<int, Inline> computeInlineMap(IList values)
    {
        IntervalTree<int, Inline> tree = [];
        int position = 0;
        foreach (Inline inline in values)
        {
            position = addInlineToTree(position, inline, tree);
        }
        return tree;
    }

    // if the index is changing and we are in debug mode we show the inline where the index is
    partial void OnIndexChanging(int? oldValue, int? newValue)
    {
        if (!_debug_mode) { return; }
        // we left the text area no point doing anything
        if (newValue is null)
        {
            return;
        }
        // nothing changed, do nothing
        if (oldValue == newValue)
        {
            return;
        }

        var newInline = _inlineMap.Query(newValue.Value).First();
        var newLineIndex = Sources.IndexOf(newInline);


        if (newInline is Run nr)
        {
            // turn the current inline underlined
            if (Sources.Remove(newInline))
            {
                // see TODO above, for now we just aggressively replace all decorations.
                nr.TextDecorations = Avalonia.Media.TextDecorations.Underline;
                Sources.Insert(newLineIndex, nr);
            };
        }
        if (oldValue is not null)
        {
            var oldInline = _inlineMap.Query(oldValue.Value).First();
            if (oldInline != newInline)
            {
                if (oldInline is Run r)
                {
                    var oldIndex = Sources.IndexOf(oldInline);
                    Sources.Remove(oldInline);
                    // TODO: technically the run could have other decorations but alas to make this
                    // really good we'd need to create a custom decoration collection
                    r.TextDecorations = [];
                    Sources.Insert(oldIndex, r);
                }
            }
        }
    }

    [RelayCommand]
    private void AddInline()
    {
        Sources.Add(new LineBreak());
    }

    [RelayCommand]
    private void AddRun()
    {
        Sources.Add(new Run() { Text = "Run Text " });
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
