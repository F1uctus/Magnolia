namespace Magnolia.Trees.Paths;

using Interface;

public class NodeListTreePath<TN, TR> : ITreePath<TreeNode<TR>, TR>,
    ITreePath<ITreeNode<TR>, TR>
    where TN : TreeNode<TR>
    where TR : TreeNode<TR> {
    readonly int indexInList;
    readonly NodeList<TN, TR> list;

    public NodeListTreePath(NodeList<TN, TR> list, int indexInList) {
        this.list        = list;
        this.indexInList = indexInList;
    }

    ITreeNode<TR> ITreePath<ITreeNode<TR>, TR>.Node {
        get => list[indexInList];
        set => list[indexInList] = (TN) value;
    }

    public bool Traversed { get; set; }

    public TreeNode<TR> Node {
        get => list[indexInList];
        set => list[indexInList] = (TN) value;
    }
}
