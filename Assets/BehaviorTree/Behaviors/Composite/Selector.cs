using UnityEngine;
using System.Collections;

namespace BehaviorTrees
{
    [BehaviorNode( "Selector", BehaviorNodeAttribute.NodeType.Composite )]
    public class Selector : BehaviorTreeNode
    {
        private const string          INDEX_FIELD_NAME = "_internal_current_index_";

        [BehaviorNodeParameter( "Random" )]
        public bool                  m_random;

        public Selector( BehaviorTree owner ) : base( owner )
        {

        }


        protected override void OnEnter( Actor actor, Blackboard local )
        {
            if ( m_random )
            {
                // Shuffle children
                Children.Sort( ( a, b ) => { return Random.Range( -1, 3 ); } );
            }

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

            BehaviorTree.ResultCode result = BehaviorTree.ResultCode.Failure;

            while ( result == BehaviorTree.ResultCode.Failure && executing < Children.Count )
            {
                result = Children[ executing ].Act( actor );

                if ( result == BehaviorTree.ResultCode.Failure )
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