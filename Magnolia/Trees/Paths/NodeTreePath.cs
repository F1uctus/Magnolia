namespace Magnolia.Trees.Paths;

using System.Reflection;
using Interface;

public class NodeTreePath<TN, TR> : ITreePath<TreeNode<TR>, TR>,
    ITreePath<ITreeNode<TR>, TR>
    where TN : TreeNode<TR>
    where TR : TreeNode<TR> {
    readonly TN node;
    readonly PropertyInfo refToNodeInParent;

    public NodeTreePath(TN node, PropertyInfo refToNodeInParent) {
        this.node              = node;
        this.refToNodeInParent = refToNodeInParent;
    }

    ITreeNode<TR> ITreePath<ITreeNode<TR>, TR>.Node {
        get => node;
        set {
            if (node == value) {
                return;
            }

            refToNodeInParent.SetValue(node.Parent, value);
        }
    }

    public bool Traversed { get; set; }

    public TreeNode<TR> Node {
        get => node;
        set => refToNodeInParent.SetValue(node.Parent, value);
    }
}
