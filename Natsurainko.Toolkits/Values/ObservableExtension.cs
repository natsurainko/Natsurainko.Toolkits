using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Natsurainko.Toolkits.Values;

public static class ObservableExtension
{
    public static void Sort<T, TKey>(
        this ObservableCollection<T> collection, 
        Func<T, TKey> keySelector,
        IComparer<TKey> comparer,
        bool descending = true)
    {
        var list = (descending
            ? collection.OrderByDescending(keySelector, comparer)
            : collection.OrderBy(keySelector, comparer)).ToList();

        for (int i = 0; i < list.Count; i++)
        {
            int old = collection.IndexOf(list[i]);

            if (old != i)
                collection.Move(old, i);
        }
    }

    public static void Sort<T, TKey>(
        this ObservableCollection<T> collection,
        Func<T, TKey> keySelector,
        bool descending = true)
    {
        var list = (descending
            ? collection.OrderByDescending(keySelector)
            : collection.OrderBy(keySelector)).ToList();

        for (int i = 0; i < list.Count; i++)
        {
            int old = collection.IndexOf(list[i]);

            if (old != i)
                collection.Move(old, i);
        }
    }

    public static void Sort<T>(this ObservableCollection<T> collection, bool descending = true) where T : IComparable<T>
    {
        var list = (descending
            ? collection.OrderByDescending(x => x)
            : collection.OrderBy(x => x)).ToList();

        for (int i = 0; i < list.Count; i++)
        {
            int old = collection.IndexOf(list[i]);

            if (old != i)
                collection.Move(old, i);
        }
    }
}
