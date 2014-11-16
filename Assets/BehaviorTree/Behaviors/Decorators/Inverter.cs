using UnityEngine;
using System.Collections;

namespace BehaviorTrees
{
    [BehaviorNode( "Inverter", BehaviorNodeAttribute.NodeType.Decorator )]
    public class Inverter : BehaviorTreeNode
    {
        public Inverter( BehaviorTree owner ) : base( owner )
        {

        }


        protected override void OnEnter( Actor actor, Blackboard local )
        {

        }


        protected override BehaviorTree.ResultCode OnAct( Actor actor, Blackboard local )
        {
            if ( Children.Count < 1 )
            {
                return BehaviorTree.ResultCode.Error;
            }

            BehaviorTree.ResultCode result = Children[ 0 ].Act( actor );

            switch( result )
            {
                default:
                case BehaviorTree.ResultCode.Running:
                case BehaviorTree.ResultCode.Error:
                    return result;                

                case BehaviorTree.ResultCode.Success:
                    return BehaviorTree.ResultCode.Failure;

                case BehaviorTree.ResultCode.Failure:
                    return BehaviorTree.ResultCode.Success;
            }
        }


        protected override void OnExit( Actor actor, Blackboard local )
        {            
        }
    }
}