namespace Magnolia.Trees;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interface;
using Paths;

/// <summary>
///     A special implementation of List that can handle tree nodes.
///     It can automatically bind parent of items
///     to one defined on list construction,
///     and provides some other useful methods.
/// </summary>
public class NodeList<T, TR> : TreeNode<TR>, IList<T>
    where T : TreeNode<TR>
    where TR : TreeNode<TR> {
    readonly IList<T> items;

    public NodeList(TreeNode<TR> parent) {
        items       = new List<T>();
        this.parent = parent;
    }

    public NodeList(TreeNode<TR> parent, IEnumerable<T> collection) {
        items  = collection.ToList();
        Parent = parent;
    }

    public bool IsEmpty => items.Count == 0;

    public T First {
        get => items.Count > 0
            ? items[0]
            : throw new IndexOutOfRangeException();
        set {
            if (items.Count > 0) {
                items[0] = value;
            }
            else {
                throw new IndexOutOfRangeException();
            }
        }
    }

    public T Last {
        get => items.Count > 0
            ? items[items.Count - 1]
            : throw new IndexOutOfRangeException();
        set {
            if (items.Count > 0) {
                items[items.Count - 1] = value;
            }
            else {
                throw new IndexOutOfRangeException();
            }
        }
    }

    void IList<T>.RemoveAt(int index) {
        items.RemoveAt(index);
    }

    public T this[int i] {
        get => items[i];
        set {
            items[i]        = value;
            items[i].Parent = this;
            items[i].Path   = new NodeListTreePath<T, TR>(this, i);
        }
    }

    public bool Remove(T item) {
        return items.Remove(item);
    }

    public int Count => items.Count;

    public bool IsReadOnly => false;

    public override void Traverse<TN>(Action<TN> visitor) {
        // Collection can be modified.
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < items.Count; i++) {
            var item = items[i];
            if (item is TN n) {
                n.Traverse(visitor);
            }
        }

        if (this is TN nn) {
            visitor(nn);
        }
    }

    public override void AfterPropertyBinding<TN>(TN? value) where TN : class { }

    public override void AfterPropertyBinding<TN>(INodeList<TN, TR> value) { }

    public void Insert(int index, T item) {
        if (index == Count) {
            Add(item);
        }
        else {
            item.Parent = this;
            item.Path   = new NodeListTreePath<T, TR>(this, index);
            items.Insert(index, item);
            for (var i = index; i < Count; i++) {
                items[i].Path = new NodeListTreePath<T, TR>(this, i);
            }
        }
    }

    public static NodeList<T, TR> operator +(NodeList<T, TR> list, T item) {
        return list.Add(item);
    }

    public NodeList<T, TR> Add(T item) {
        items.Add(item);
        item.Parent = this;
        item.Path   = new NodeListTreePath<T, TR>(this, Count - 1);
        return this;
    }

    void ICollection<T>.Add(T item) {
        Add(item);
    }

    public NodeList<T, TR> AddRange(IEnumerable<T> list) {
        foreach (var item in list) {
            items.Add(item);
        }

        return this;
    }

    public int IndexOf(T item) {
        return items.IndexOf(item);
    }

    public bool Contains(T item) {
        return items.Contains(item);
    }

    public void Clear() {
        items.Clear();
    }

    public IEnumerator<T> GetEnumerator() {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    [Obsolete]
    void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
        if (array == null) {
            throw new ArgumentNullException(nameof(array));
        }

        if (arrayIndex < 0) {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (array.Rank > 1) {
            throw new ArgumentException(
                "Only single dimensional arrays are supported for the requested action.",
                nameof(array)
            );
        }

        if (array.Length - arrayIndex < Count) {
            throw new ArgumentException(
                "Not enough elements after index in the destination array."
            );
        }

        for (var i = 0; i < Count; i++) {
            array.SetValue(this[i], i + arrayIndex);
        }
    }
}
