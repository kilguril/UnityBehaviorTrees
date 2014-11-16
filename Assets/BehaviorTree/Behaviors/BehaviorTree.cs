using UnityEngine;
using System.Collections;

namespace BehaviorTrees
{
    public class BehaviorTree
    {
        public enum ResultCode
        {
            Success,
            Failure,
            Running,
            Error
        }


        public BehaviorTreeNode Root
        {
            get { return m_rootNode;  }
            set { m_rootNode = value; }
        }


        private BehaviorTreeNode        m_rootNode;


        public ResultCode Act( Actor actor )
        {
            if ( m_rootNode != null )
            {
                return m_rootNode.Act( actor );
            }

            return ResultCode.Error;
        }


    }
}