using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BehaviorTrees
{
    public abstract class BehaviorTreeNode
    {
        private const string          RUNNING_STATUS_FIELD_NAME = "_internal_running_";


        public List<BehaviorTreeNode> Children { get; private set; }
        public BehaviorTree           Owner    { get; private set; }


        public BehaviorTreeNode( BehaviorTree owner )
        {
            Owner    = owner;
            Children = new List<BehaviorTreeNode>();
        }


        public BehaviorTree.ResultCode Act( Actor actor )
        {
            Blackboard                   local  = actor.Blackboards.GetBlackboard( this );
            BehaviorTree.ResultCode      result = BehaviorTree.ResultCode.Error;

            bool running;
            bool runningExists;

            runningExists = local.GetValue< bool >( RUNNING_STATUS_FIELD_NAME, out running );

            if ( !running || ! runningExists )
            {
                OnEnter( actor, local );
            }

            result = OnAct( actor, local );

            if ( result == BehaviorTree.ResultCode.Running )
            {
                local.SetValue< bool >( RUNNING_STATUS_FIELD_NAME, true );
            }
            else
            {
                local.SetValue<bool>( RUNNING_STATUS_FIELD_NAME, false );
                OnExit( actor, local );
            }

            return result;
        }

        protected abstract void                     OnEnter( Actor actor, Blackboard local );
        protected abstract BehaviorTree.ResultCode  OnAct( Actor actor, Blackboard local );
        protected abstract void                     OnExit( Actor actor, Blackboard local );
    }
}