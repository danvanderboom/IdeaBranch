using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CriticalInsight.Data.Hierarchical;

public interface ITreeNode<T> : ITreeNode
{
    T Payload { get; set; }
}

public interface ITreeNode : INotifyPropertyChanged
{
    string NodeId { get; set; }

    [JsonIgnore]
    ITreeNode Root { get; }

    [JsonIgnore]
    ITreeNode? Parent { get; }

    [JsonIgnore]
    int Depth => Parent == null ? 0 : Parent.Depth + 1; // distance from Root

    TreeNodeList Children { get; }

    string PayloadType { get; set; }

    object PayloadObject { get; set; }

    // all nodes along path toward root: Parent, Parent.Parent, Parent.Parent.Parent, ...
    [JsonIgnore]
    IEnumerable<ITreeNode> Ancestors { get; }

    [JsonIgnore]
    IEnumerable<ITreeNode> Subtree { get; }

    // iterator: Children, Children[i].Children, ...
    [JsonIgnore]
    IEnumerable<ITreeNode> Descendants { get; }

    void SetParent(ITreeNode? Node, bool updateChildNodes = true);

    void RaiseAncestorChangedEvent(NodeChangeType changeType, ITreeNode node);

    void RaiseDescendantChangedEvent(NodeChangeType changeType, ITreeNode node);

    void RaiseDepthChangedEvent();

    string Serialize(Dictionary<string, Type> payloadTypes, bool writeIndented = true);
}