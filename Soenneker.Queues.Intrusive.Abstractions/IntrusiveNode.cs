namespace Soenneker.Queues.Intrusive.Abstractions;

/// <summary>
/// Base class for nodes used in intrusive, singly-linked structures.
///
/// This class provides a storage-backed implementation of <see cref="IIntrusiveNode{TNode}"/>,
/// exposing the required <see cref="Next"/> linkage via a ref-returning property.
///
/// Deriving from this class avoids having to manually implement the linkage in each node type.
/// </summary>
/// <typeparam name="TNode">
/// The concrete node type. This is typically the deriving type itself (self-referential generic constraint).
/// </typeparam>
/// <remarks>
/// Intrusive contract:
/// <list type="bullet">
/// <item>A node must not be inserted while it is already part of any intrusive structure.</item>
/// <item>The <see cref="Next"/> link is owned and manipulated by the structure while the node is linked.</item>
/// <item>Nodes may be reused only after they are removed by the owning structure.</item>
/// </list>
/// </remarks>
public abstract class IntrusiveNode<TNode> : IIntrusiveNode<TNode>
    where TNode : class, IIntrusiveNode<TNode>
{
    private TNode? _next;

    /// <summary>
    /// Gets a reference to the next node in the intrusive structure.
    /// </summary>
    /// <remarks>
    /// This returns a reference to the underlying storage field to enable lock-free algorithms
    /// to perform <see cref="System.Threading.Volatile"/> and <see cref="System.Threading.Interlocked"/>
    /// operations directly on it.
    /// </remarks>
    public ref TNode? Next => ref _next;
}