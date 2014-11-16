using UnityEngine;
using System.Collections;

namespace BehaviorTrees
{
    [BehaviorNode( "Repeater", BehaviorNodeAttribute.NodeType.Decorator )]
    public class Repeater : BehaviorTreeNode
    {
        private const string          COUNTER_FIELD_NAME = "_internal_repeat_count_";

        [BehaviorNodeParameter("Repeat Count")]
        public int                   m_repeatCount;

        [BehaviorNodeParameter( "Stop On Fail" )]
        public bool                  m_stopOnFail;

        public Repeater( BehaviorTree owner ) : base( owner )
        {

        }

        protected override void OnEnter( Actor actor, Blackboard local )
        {
            local.SetValue< int >( COUNTER_FIELD_NAME, 0 );
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
                case BehaviorTree.ResultCode.Failure:
                    if ( m_stopOnFail && result == BehaviorTree.ResultCode.Failure )
                    {
                        return BehaviorTree.ResultCode.Success;
                    }

                    int repeated = 0;

                    if ( !local.GetValue< int >( COUNTER_FIELD_NAME, out repeated ) )
                    {
                        repeated = 0;
                    }

                    repeated++;
                    local.SetValue< int >( COUNTER_FIELD_NAME, repeated );

                    if ( repeated < m_repeatCount || m_repeatCount <= 0 )
                    {
                        return BehaviorTree.ResultCode.Running;
                    }

                    return BehaviorTree.ResultCode.Success;
            }
        }


        protected override void OnExit( Actor actor, Blackboard local )
        {            
        }
    }
}