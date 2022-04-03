namespace Magnolia.Interface;

using System.Collections.Generic;

/// <summary>
///     An interface for List that can handle tree nodes.
/// </summary>
public interface INodeList<T, TR> : ITreeNode<TR>, IList<T>
    where T : ITreeNode<TR>
    where TR : class, ITreeNode<TR> {
    bool IsEmpty { get; }

    T First { get; set; }

    T Last { get; set; }
}
