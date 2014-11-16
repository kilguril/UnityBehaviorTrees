using UnityEngine;
using System.Collections;

namespace BehaviorTrees
{
    [BehaviorNode( "Sequencer", BehaviorNodeAttribute.NodeType.Composite )]
    public class Sequencer : BehaviorTreeNode
    {
        private const string          INDEX_FIELD_NAME = "_internal_current_index_";

        [BehaviorNodeParameter( "Continue On Fail" )]
        public bool                  m_continueOnFail;

        public Sequencer( BehaviorTree owner ) : base( owner )
        {

        }


        protected override void OnEnter( Actor actor, Blackboard local )
        {
            local.SetValue< int >( INDEX_FIELD_NAME, 0 );
        }


        protected override BehaviorTree.ResultCode OnAct( Actor actor, Blackboard local )
        {
            int executing = 0;
            
            if ( !local.GetValue< int >( INDEX_FIELD_NAME, out executing ) )
            {
                return BehaviorTree.ResultCode.Error;
            }

            if ( Children.Count <= executing )
            {
                return BehaviorTree.ResultCode.Error;
            }

            BehaviorTree.ResultCode result = BehaviorTree.ResultCode.Success;

            while( result == BehaviorTree.ResultCode.Success && executing < Children.Count )

            {
                result = Children[ executing ].Act( actor );

                if ( m_continueOnFail && result == BehaviorTree.ResultCode.Failure )
                {
                    result = BehaviorTree.ResultCode.Success;
                }

                if ( result == BehaviorTree.ResultCode.Success )
                {
                    executing++;
                    local.SetValue<int>( INDEX_FIELD_NAME, executing );
                }
            }

            return result;
        }


        protected override void OnExit( Actor actor, Blackboard local )
        {            
        }
    }
}