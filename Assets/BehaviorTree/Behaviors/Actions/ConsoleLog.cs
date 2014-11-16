using UnityEngine;
using System.Collections;

namespace BehaviorTrees
{
    [BehaviorNode( "Console Log (Debug)" , BehaviorNodeAttribute.NodeType.Leaf )]
    public class ConsoleLog : BehaviorTreeNode
    {
        [BehaviorNodeParameter("Message")]
        public string m_message;

        public ConsoleLog( BehaviorTree owner ) : base( owner )
        {

        }


        protected override void OnEnter( Actor actor, Blackboard local )
        {

        }


        protected override BehaviorTree.ResultCode OnAct( Actor actor, Blackboard local )
        {
            Debug.Log( m_message );
            return BehaviorTree.ResultCode.Success;
        }


        protected override void OnExit( Actor actor, Blackboard local )
        {            
        }
    }
}