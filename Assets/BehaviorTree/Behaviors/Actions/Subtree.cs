using UnityEngine;
using System.Collections;

namespace BehaviorTrees
{
    [BehaviorNode( "Subtree", BehaviorNodeAttribute.NodeType.Leaf )]
    public class Subtree : BehaviorTreeNode
    {
        [BehaviorNodeParameter( "Subtree Behavior Id" )]
        public string m_id;

        private BehaviorTree    m_subtree;

        public Subtree( BehaviorTree owner ) : base( owner )
        {
            
        }


        protected override void OnEnter( Actor actor, Blackboard local )
        {
            if ( m_subtree == null )
            {
                m_subtree = BehaviorCache.GetBehavior( m_id );
            }
        }


        protected override BehaviorTree.ResultCode OnAct( Actor actor, Blackboard local )
        {
            if ( m_subtree == null )
            {
                return BehaviorTree.ResultCode.Failure;
            }

            return m_subtree.Act( actor );
        }


        protected override void OnExit( Actor actor, Blackboard local )
        {            
        }
    }
}