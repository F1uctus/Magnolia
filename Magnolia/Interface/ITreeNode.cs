namespace Magnolia.Interface;

using System;

public interface ITreeNode<TR> where TR : class, ITreeNode<TR> {
    /// <summary>
    ///     Root of node's tree.
    /// </summary>
    TR? Root { get; }

    /// <summary>
    ///     Direct reference to the attribute of
    ///     parent to which this node is bound.
    /// </summary>
    ITreePath<ITreeNode<TR>, TR> Path { get; set; }

    /// <summary>
    ///     Reference to parent of this node.
    /// </summary>
    ITreeNode<TR>? Parent { get; set; }

    /// <summary>
    ///     Returns first parent of this node with given type.
    ///     (<code>null</code> if parent of given type does not exists).
    /// </summary>
    T? GetParent<T>() where T : class, ITreeNode<TR>;

    void Traverse<TN>(Action<TN> visitor)
        where TN : ITreeNode<TR>;

    void AfterPropertyBinding<TN>(TN? value)
        where TN : class, ITreeNode<TR>;

    void AfterPropertyBinding<TN>(INodeList<TN, TR> value)
        where TN : ITreeNode<TR>;
}
