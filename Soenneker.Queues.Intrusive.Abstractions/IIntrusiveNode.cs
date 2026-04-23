namespace Soenneker.Queues.Intrusive.Abstractions;

/// <summary>
/// Defines the intrusive linkage required by an intrusive node.
///
/// Implementations must provide stable storage for a single forward link that can be accessed
/// by reference to support lock-free publication using <see cref="System.Threading.Volatile"/>
/// and <see cref="System.Threading.Interlocked"/>.
/// </summary>
/// <typeparam name="TNode">
/// The concrete node type. This is typically the implementing type itself (self-referential generic constraint).
/// </typeparam>
/// <remarks>
/// Intrusive contract:
/// <list type="bullet">
/// <item>
/// The returned reference must point to real, stable storage (typically a field), not a computed or temporary value.
/// </item>
/// <item>
/// The <see cref="Next"/> link is owned by the intrusive data structure while the node is enqueued and must not be
/// modified by user code during that time.
/// </item>
/// <item>
/// A node must not be enqueued more than once concurrently or while it is already part of any intrusive structure.
/// </item>
/// <item>
/// Implementations should assume the structure may set <see cref="Next"/> to <c>null</c> during enqueue/dequeue
/// and when preparing a node for reuse.
/// </item>
/// </list>
/// </remarks>
public interface IIntrusiveNode<TNode> where TNode : class, IIntrusiveNode<TNode>
{
    /// <summary>
    /// Gets a reference to the next node in the intrusive structure.
    /// </summary>
    /// <remarks>
    /// This must return a reference to the underlying storage location so that lock-free algorithms can safely perform
    /// atomic and volatile operations on it.
    /// </remarks>
    ref TNode? Next { get; }
}