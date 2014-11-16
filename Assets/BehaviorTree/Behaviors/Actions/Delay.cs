using UnityEngine;
using System.Collections;

namespace BehaviorTrees
{
    [BehaviorNode( "Delay", BehaviorNodeAttribute.NodeType.Leaf )]
    public class Delay : BehaviorTreeNode
    {
        private const string          START_TIME_FIELD_NAME = "_internal_delay_start_";

        [BehaviorNodeParameter("Duration (sec)")]
        public float m_duration;

        [BehaviorNodeParameter("Use Realtime")]
        public bool  m_useRealtime;

        public Delay( BehaviorTree owner ) : base( owner )
        {

        }


        protected override void OnEnter( Actor actor, Blackboard local )
        {
            float t = m_useRealtime ? Time.realtimeSinceStartup : Time.time;
            local.SetValue< float >( START_TIME_FIELD_NAME, t );
        }


        protected override BehaviorTree.ResultCode OnAct( Actor actor, Blackboard local )
        {
            float t = 0.0f; 
            
            if ( !local.GetValue< float >( START_TIME_FIELD_NAME, out t ) )
            {
                return BehaviorTree.ResultCode.Error;
            }

            if ( m_useRealtime && ( Time.realtimeSinceStartup - t ) < m_duration )
            {
                return BehaviorTree.ResultCode.Running;
            }

            if ( !m_useRealtime && ( Time.time - t ) < m_duration )
            {
                return BehaviorTree.ResultCode.Running;
            }

            return BehaviorTree.ResultCode.Success;
        }


        protected override void OnExit( Actor actor, Blackboard local )
        {            
        }
    }
}