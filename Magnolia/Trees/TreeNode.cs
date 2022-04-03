namespace Magnolia.Trees;

using System;
using Attributes;
using Interface;

/// <typeparam name="TR">
///     Type of the tree root node.
/// </typeparam>
public abstract class TreeNode<TR> : ITreeNode<TR> where TR : TreeNode<TR> {
    TR? root;

    /// <summary>
    ///     Root of node's tree.
    /// </summary>
    public TR? Root {
        get {
            if (root != null) {
                return root;
            }

            var p = this;
            while (p is { } and not TR) {
                p = (TreeNode<TR>?) p.Parent;
            }

            root = (TR?) p;
            return root;
        }
    }

    /// <summary>
    ///     Direct reference to the attribute of
    ///     parent to which this node is bound.
    /// </summary>
    public ITreePath<ITreeNode<TR>, TR> Path { get; set; } = null!;

    protected TreeNode<TR>? parent;

    /// <summary>
    ///     Reference to parent of this node.
    /// </summary>
    [NotTraversable]
    public ITreeNode<TR>? Parent {
        get => parent;
        set {
            if (this is TR) {
                return;
            }

            parent = (TreeNode<TR>?) value;

            if (parent == null) {
                return;
            }

            Traverse<TreeNode<TR>>(
                child => {
                    if (child != this) {
                        child.parent = this;
                    }
                }
            );
        }
    }

    /// <summary>
    ///     Returns first parent of this node with given type.
    ///     (<code>null</code> if parent of given type does not exists).
    /// </summary>
    public TN? GetParent<TN>() where TN : class, ITreeNode<TR> {
        ITreeNode<TR>? p = this;
        while (true) {
            p = p.Parent;
            switch (p) {
                case TN node:    return node;
                case TR or null: return null;
            }
        }
    }

    /// <param name="visitor">
    ///     Visited node processor. To replace
    ///     selected child in parent entirely,
    ///     you can use the <see cref="Path"/> property.
    /// </param>
    /// <typeparam name="TN">
    ///     Type of desired nodes to traverse.
    /// </typeparam>
    public abstract void Traverse<TN>(Action<TN> visitor)
        where TN : ITreeNode<TR>;

    /// <summary>
    ///     A handler invoked each time some child node
    ///     has been bound to this parent node.
    /// </summary>
    /// <param name="value">
    ///     Newly bound child node.
    /// </param>
    public abstract void AfterPropertyBinding<TN>(TN? value)
        where TN : class, ITreeNode<TR>;

    /// <summary>
    ///     A handler invoked each time some children nodes list
    ///     has been bound to this parent node.
    /// </summary>
    /// <param name="value">
    ///     Newly bound children nodes list.
    /// </param>
    public abstract void AfterPropertyBinding<TN>(INodeList<TN, TR> value)
        where TN : ITreeNode<TR>;
}
