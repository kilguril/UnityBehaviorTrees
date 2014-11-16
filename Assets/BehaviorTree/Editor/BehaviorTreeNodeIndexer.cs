using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using BehaviorTrees;

namespace BehaviorTreesEditor
{
    public class BehaviorTreeNodeIndexer
    {
        public Dictionary< BehaviorNodeControl, string > IndexTable
        {
            get { return m_indexTable; }
        }

        private Dictionary< BehaviorNodeControl, string > m_indexTable;

        public BehaviorTreeNodeIndexer( BehaviorNodeControl root )
        {
            m_indexTable = new Dictionary<BehaviorNodeControl, string>();
            IndexNode( root );
        }

        private void IndexNode( BehaviorNodeControl node )
        {
            if ( !m_indexTable.ContainsKey( node ) )
            {
                m_indexTable.Add( node, string.Format( "n{0}", m_indexTable.Keys.Count + 1 ) );
            }

            foreach ( BehaviorNodeControl o in node.m_outputs )
            {
                IndexNode( o );
            }
        }
    }
}