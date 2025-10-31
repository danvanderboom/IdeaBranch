using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriticalInsight.Data.Hierarchical;

public class TreeNodeList : ObservableCollection<ITreeNode>
{
    public ITreeNode? Parent { get; set; }

    public TreeNodeList() : base() { }

    public TreeNodeList(ITreeNode parent) : base()
    {
        Parent = parent;
    }

    // Overrides that allow an optional update of the child's Parent property.
    public new ITreeNode Add(ITreeNode node) => Add(node, updateParent: true);

    public ITreeNode Add(ITreeNode node, bool updateParent)
    {
        if (updateParent)
        {
            node.SetParent(Parent, updateChildNodes: true);
            return node;
        }

        base.Add(node);
        return node;
    }

    public new void Remove(ITreeNode node) => Remove(node, updateParent: true);

    public void Remove(ITreeNode node, bool updateParent)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        if (!Contains(node))
            return;

        if (updateParent)
            node.SetParent(null, updateChildNodes: false);

        base.Remove(node);
    }

    public override string ToString() => $"Count = {Count}";
}