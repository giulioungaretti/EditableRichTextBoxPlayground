using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntervalTree;

namespace AvaloniaApplication1.ViewModels;

public partial class MainViewModel : ViewModelBase

{
    private bool _debug_mode = true;

    [ObservableProperty] private int? _index;
    [ObservableProperty] private int? _cursorRelased;

    private IntervalTree<int, int> _inlineMap = [];

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

    #region sematics of the colelction changed
    /*

    If Action is NotifyCollectionChangedAction.Remove, then OldItems contains
    the items that were removed.In addition, if OldStartingIndex is not -1,
    then it contains the index where the old items were removed.

    If Action is NotifyCollectionChangedAction.Replace, then OldItems contains the
    replaced items and NewItems contains the replacement items. In addition,
    NewStartingIndex and OldStartingIndex are equal, and if they are not -1, then
    they contain the index where the items were replaced.

    If Action is NotifyCollectionChangedAction.Move, then NewItems and OldItems are
    logically equivalent (i.e., they are SequenceEqual, even if they are different
    instances), and they contain the items that moved.In addition, OldStartingIndex
    contains the index where the items were moved from, and NewStartingIndex
    contains the index where the items were moved to.A Move operation is logically
    treated as a Remove followed by an Add, so NewStartingIndex is interpreted as
    though the items had already been removed.
    */
    #endregion
    private static T checkItemNotNull<T>(IList? input)
    {
        var newItems = input?.Cast<T>();
        return newItems!.Single();
    }
    private void Sources_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _inlineMap = computeInlineMap(_sources);
        //switch (e.Action)
        //{
        //    //If Action is NotifyCollectionChangedAction.Add, then NewItems contains the
        //    //items that were added.In addition, if NewStartingIndex is not -1, then it
        //    //contains the index where the new items were added.
        //    case NotifyCollectionChangedAction.Add:
        //        var newItem = checkItemNotNull<Inline>(e.NewItems);
        //        if (e.NewStartingIndex != -1)
        //        {
        //            //var  = _inlineMap.MaxBy((inline) => inline.From);
        //        }
        //        break;
        //    case NotifyCollectionChangedAction.Remove:
        //        break;
        //    case NotifyCollectionChangedAction.Replace:
        //        break;
        //};
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

    private static int addInlineToTree(int position, Inline inline, int index, IntervalTree<int, int> tree)
    {
        int startPosition = position;
        if (inline is InlineUIContainer i)
        {
            if (i.Child is TextBox t)
            {
                position += t.Text!.Length;
            }
            else
            {
                position = +1;
            }
        }
        if (inline is Run r)
        {
            position += r.Text!.Length;
        }
        tree.Add(startPosition, position, index);
        return position;
    }
    private static IntervalTree<int, int> computeInlineMap(IList values)
    {
        IntervalTree<int, int> tree = [];
        int position = 0;
        int index = 0;
        foreach (Inline inline in values)
        {
            position = addInlineToTree(position, inline, index, tree);
            index++;
        }
        return tree;
    }

    partial void OnCursorRelasedChanged(int? oldValue, int? newValue)
    {
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

        var newInlineID = _inlineMap.Query(newValue.Value).First();
        var newInline = Sources[newInlineID];


        if (newInline is Run nr)
        {
            // turn the current inline underlined
            Sources.RemoveAt(newInlineID);
            // transform run into textbox    
            var textBox = new TextBox()
            {
                Text = nr.Text,
                BorderThickness = Thickness.Parse("0"),
                Margin = Thickness.Parse("0"),
                Padding = Thickness.Parse("0"),
            };
            textBox.PointerExited += (s, e) => TextBox_LostFocus(s, e, newInlineID);
            var inlineUiContainer = new InlineUIContainer(textBox);
            Sources.Insert(newInlineID, inlineUiContainer);
        }
        //if (oldValue is not null)
        //{
        //    var oldInLineId = _inlineMap.Query(oldValue.Value).First();
        //    var oldInline = Sources[oldInLineId];
        //    if (oldInLineId != newInlineID)
        //    {
        //        if (oldInline is InlineUIContainer i)
        //        {
        //            Sources.RemoveAt(oldInLineId);
        //            if (i.Child is TextBox t)
        //            {
        //                var r = new Run(t.Text);
        //                Sources.Insert(oldInLineId, r);
        //            }
        //        }
        //    }
        //}
    }

    private void TextBox_LostFocus(object? _, Avalonia.Interactivity.RoutedEventArgs e, int newInlineID)
    {
        var inline = Sources[newInlineID];
        if (inline is InlineUIContainer i)
        {
            Sources.RemoveAt(newInlineID);
            if (i.Child is TextBox t)
            {
                var r = new Run(t.Text);
                Sources.Insert(newInlineID, r);
            }
        }
        e.Handled = true;
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

        var newInlineID = _inlineMap.Query(newValue.Value).First();
        var newInline = Sources[newInlineID];


        if (newInline is Run nr)
        {
            // turn the current inline underlined
            Sources.RemoveAt(newInlineID);
            // see TODO above, for now we just aggressively replace all decorations.
            nr.TextDecorations = Avalonia.Media.TextDecorations.Underline;
            Sources.Insert(newInlineID, nr);
        }
        if (oldValue is not null)
        {
            var oldInLineId = _inlineMap.Query(oldValue.Value).First();
            var oldInline = Sources[oldInLineId];
            if (oldInLineId != newInlineID)
            {
                if (oldInline is Run r)
                {
                    Sources.RemoveAt(oldInLineId);
                    // TODO: technically the run could have other decorations but alas to make this
                    // really good we'd need to create a custom decoration collection
                    r.TextDecorations = [];
                    Sources.Insert(oldInLineId, r);
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
        Sources.Add(new Run() { Text = "Appended Text " });
    }

    [RelayCommand]
    private void AddRunAtTwo()
    {
        Sources.Insert(1, new Run() { Text = "/ Added Text second position" });
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
                        BorderThickness = Thickness.Parse("0"),
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
