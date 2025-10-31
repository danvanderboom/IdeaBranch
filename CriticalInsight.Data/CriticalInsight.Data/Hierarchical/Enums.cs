using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriticalInsight.Data.Hierarchical;

public enum UpDownTraversalType
{
    TopDown,
    BottomUp
}

public enum DepthBreadthTraversalType
{
    DepthFirst,
    BreadthFirst
}

public enum NodeChangeType
{
    NodeAdded,
    NodeRemoved,
    PropertyChanged
}

public enum NodeRelationType
{
    Ancestor,
    Parent,
    Self,
    Child,
    Descendant
}

public enum ExpandableNodeState
{
    Collapsed,
    Expanded
}