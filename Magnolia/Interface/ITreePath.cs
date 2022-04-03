namespace Magnolia.Interface;

/// <summary>
///     Data structure designed to simplify object trees traversal.
///     It binds each child to parent by reference,
///     so you can dynamically modify/replace children of base expression.
/// </summary>
public interface ITreePath<TN, TR>
    where TN : ITreeNode<TR>
    where TR : class, ITreeNode<TR> {
    bool Traversed { get; set; }
    TN Node { get; set; }
}
