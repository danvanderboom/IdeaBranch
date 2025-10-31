using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CriticalInsight.Data.Hierarchical;

public class TreeNode<T> : ObservableObject, ITreeNode<T>, IDisposable
    where T : new()
{
    public TreeNode()
    {
        PayloadType = typeof(T).AssemblyQualifiedName ?? string.Empty;
        _PayloadObject = typeof(T).IsSubclassOf(typeof(TreeNode<T>)) ? this : new T();

        _Parent = null;
        Children = new TreeNodeList(this);
        Children.CollectionChanged += Children_CollectionChanged;
    }

    public TreeNode(T payload)
    {
        PayloadType = typeof(T).AssemblyQualifiedName ?? string.Empty;
        _PayloadObject = payload ?? new T();

        _Parent = null;
        Children = new TreeNodeList(this);
        Children.CollectionChanged += Children_CollectionChanged;
    }

    public TreeNode(T payload, TreeNode<T> parent)
    {
        PayloadType = typeof(T).AssemblyQualifiedName ?? string.Empty;
        _PayloadObject = payload ?? new T();

        _Parent = parent;
        Children = new TreeNodeList(this);
        Children.CollectionChanged += Children_CollectionChanged;
    }

    private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasChildren));
    }

    [JsonPropertyOrder(0)]
    public string NodeId { get; set; } = Guid.NewGuid().ToString();

    private ITreeNode? _Parent;

    [JsonIgnore]
    public ITreeNode? Parent
    {
        get => _Parent;
        set => SetParent(value, true);
    }

    public void SetParent(ITreeNode? node, bool updateChildNodes = true)
    {
        if (node == Parent)
            return;

        var oldParent = Parent;
        var oldDepth = Depth;

        // if oldParent isn't null
        // remove this node from its newly ex-parent's children
        if (oldParent != null && oldParent.Children.Contains(this))
            oldParent.Children.Remove(this, updateParent: false);

        // update the backing field
        _Parent = node;

        // add this node to its new parent's children
        if (_Parent != null && updateChildNodes)
            _ = _Parent.Children.Add(this, updateParent: false);

        // signal the old parent that it has lost this child
        if (oldParent != null)
            oldParent.RaiseDescendantChangedEvent(NodeChangeType.NodeRemoved, this);

        if (oldDepth != Depth)
            RaiseDepthChangedEvent();

        // if this operation has changed the height of any parent, initiate the bubble-up height changed event
        if (Parent != null)
            Parent.RaiseDescendantChangedEvent(NodeChangeType.NodeAdded, this);

        OnParentChanged(oldParent, Parent);
    }

    protected virtual void OnParentChanged(ITreeNode? oldValue, ITreeNode? newValue) => OnPropertyChanged(nameof(Parent));

    [JsonPropertyOrder(3)]
    public TreeNodeList Children { get; set; }

    [JsonIgnore]
    public virtual bool HasChildren => Children.Count > 0;

    public event Action<NodeChangeType, ITreeNode>? AncestorChanged;
    public virtual void RaiseAncestorChangedEvent(NodeChangeType changeType, ITreeNode node)
    {
        if (AncestorChanged != null)
            AncestorChanged(changeType, node);

        foreach (ITreeNode child in Children)
            child.RaiseAncestorChangedEvent(changeType, node);
    }

    public event Action<NodeChangeType, ITreeNode>? DescendantChanged;
    public virtual void RaiseDescendantChangedEvent(NodeChangeType changeType, ITreeNode node)
    {
        if (DescendantChanged != null)
            DescendantChanged(changeType, node);

        if (Parent != null)
            Parent.RaiseDescendantChangedEvent(changeType, node);
    }

    [JsonPropertyOrder(1)]
    public string PayloadType { get; set; }

    private object _PayloadObject;

    [JsonIgnore]
    public object PayloadObject
    {
        get => _PayloadObject;
        set
        {
            Set(nameof(PayloadObject), ref _PayloadObject, value);
            OnPropertyChanged(nameof(Payload));
        }
    }

    [JsonPropertyOrder(2)]
    [JsonIgnore]
    public T Payload
    {
        get => (T)_PayloadObject;
        set => _PayloadObject = value ?? new T();
    }

    // [recurse up] bubble up aggregate property
    [JsonIgnore]
    public int Depth => Parent == null ? 0 : Parent.Depth + 1;

    // [recurse up] bubble up event
    public virtual void RaiseDepthChangedEvent()
    {
        OnPropertyChanged(nameof(Depth));

        if (Parent != null)
            Parent.RaiseDepthChangedEvent();
    }

    [JsonIgnore]
    public ITreeNode Root => Parent == null ? this : Parent.Root;

    // all nodes along path toward root: Parent, Parent.Parent, Parent.Parent.Parent, ...
    [JsonIgnore]
    public IEnumerable<ITreeNode> Ancestors
    {
        get
        {
            if (Parent == null)
                yield break;

            yield return Parent;

            foreach (ITreeNode node in Parent.Ancestors)
                yield return node;

            yield break;
        }
    }

    // iterator: Children, Children[i].Children, ...
    [JsonIgnore]
    public IEnumerable<ITreeNode> Descendants
    {
        get
        {
            foreach (ITreeNode node in Children)
            {
                yield return node;

                foreach (ITreeNode descendant in node.Descendants)
                    yield return descendant;
            }

            yield break;
        }
    }

    [JsonIgnore]
    public IEnumerable<ITreeNode> Subtree
    {
        get
        {
            yield return this;

            foreach (ITreeNode node in Descendants)
                yield return node;

            yield break;
        }
    }

    public string Serialize(Dictionary<string, Type> payloadTypes, bool writeIndented = true)
    {
        return TreeJsonSerializer.Serialize(this, payloadTypes, writeIndented);
    }

    #region Dispose

    protected UpDownTraversalType DisposeTraversal { get; set; } = UpDownTraversalType.BottomUp;

    protected bool IsDisposed { get; private set; }

    public event EventHandler? Disposing;

    public virtual void Dispose()
    {
        CheckDisposed();
        OnDisposing();

        // clean up contained objects (in Value property)
        if (Payload is IDisposable)
        {
            if (DisposeTraversal == UpDownTraversalType.BottomUp)
                foreach (TreeNode<T> node in Children)
                    node.Dispose();

            (Payload as IDisposable)?.Dispose();

            if (DisposeTraversal == UpDownTraversalType.TopDown)
                foreach (TreeNode<T> node in Children)
                    node.Dispose();
        }

        IsDisposed = true;
    }

    protected void OnDisposing()
    {
        if (Disposing != null)
            Disposing(this, EventArgs.Empty);
    }

    public void CheckDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    #endregion

    public override string ToString() => GetType().Name + ": " + "Depth = " + Depth + ", Children = " + (Children?.Count ?? 0);
}