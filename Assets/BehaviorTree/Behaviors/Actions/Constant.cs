using UnityEngine;
using System.Collections;

namespace BehaviorTrees
{
    [BehaviorNode( "Constant (Debug)", BehaviorNodeAttribute.NodeType.Leaf )]
    public class Constant : BehaviorTreeNode
    {
        [BehaviorNodeParameter("Success")]
        public bool m_success;

        public Constant( BehaviorTree owner ) : base( owner )
        {

        }


        protected override void OnEnter( Actor actor, Blackboard local )
        {

        }


        protected override BehaviorTree.ResultCode OnAct( Actor actor, Blackboard local )
        {
            if ( m_success )
            {
                return BehaviorTree.ResultCode.Success;
            }

            return BehaviorTree.ResultCode.Failure;
        }


        protected override void OnExit( Actor actor, Blackboard local )
        {            
        }
    }
}