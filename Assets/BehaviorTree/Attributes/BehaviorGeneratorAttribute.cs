using System;

namespace BehaviorTrees
{
    [AttributeUsage( AttributeTargets.Method )]
    public class BehaviorGeneratorAttribute : Attribute
    {
        public readonly string id;

        public BehaviorGeneratorAttribute( string behaviorId )
        {
            id = behaviorId;
        }
    }
}