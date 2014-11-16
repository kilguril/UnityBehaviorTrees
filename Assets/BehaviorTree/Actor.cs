using UnityEngine;
using System.Collections;

namespace BehaviorTrees
{
    public class Actor : MonoBehaviour
    {
        public BlackboardCache              Blackboards { get; private set; }
        public BehaviorTree                 Behavior    { get; private set; }       // Todo - set to private, get from cache

        public BehaviorTree.ResultCode      Result      { get; private set; }


        public string                       m_defaultBehavior;

        public void Execute( string behavior )
        {
            Behavior = BehaviorCache.GetBehavior( behavior );
            Result = BehaviorTree.ResultCode.Running;
        }


        public void Stop( bool clearData )
        {
            if ( clearData )
            {
                Blackboard data = Blackboards.GetBlackboard( Behavior );
                data.Clear();
            }

            Behavior = null;
        }


        protected virtual void Start()
        {
            if ( !string.IsNullOrEmpty( m_defaultBehavior ) )
            {
                Execute( m_defaultBehavior );
            }
        }

        protected virtual void Update()
        {
            if ( Behavior != null )
            {
                if ( Blackboards == null )
                {
                    Blackboards = new BlackboardCache();
                }

                if ( Result == BehaviorTree.ResultCode.Running )
                {
                    Result = Behavior.Act( this );
                }
            }
        }
    }
}