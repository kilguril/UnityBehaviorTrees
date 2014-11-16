using System;

namespace BehaviorTrees
{
    [AttributeUsage( AttributeTargets.Field )]
    public class BehaviorNodeParameterAttribute : Attribute
    {
        public readonly string displayName;

        public BehaviorNodeParameterAttribute( string name )
        {
            displayName = name;
        }
    }
}