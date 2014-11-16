using System;

namespace BehaviorTrees
{
    [AttributeUsage( AttributeTargets.Class )]
    public class BehaviorNodeAttribute : Attribute
    {
        public enum NodeType
        {
            Composite,
            Decorator,
            Leaf
        }

        public readonly string      displayName;
        public readonly NodeType    nodeType;

        public BehaviorNodeAttribute( string name, NodeType type )
        {
            displayName = name;
            nodeType    = type;
        }
    }
}