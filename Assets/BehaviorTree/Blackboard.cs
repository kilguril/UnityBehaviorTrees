using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BehaviorTrees
{
    public class Blackboard
    {
        private Dictionary< System.Type, Dictionary< string, object > >     m_typedBackingFields;


        public Blackboard()
        {
            m_typedBackingFields = new Dictionary<System.Type, Dictionary<string, object>>();
        }


        public void Clear()
        {
            m_typedBackingFields.Clear();
        }


        public bool GetValue< T >( string key, out T val )
        {
            System.Type t = typeof( T );

            if ( m_typedBackingFields.ContainsKey( t ) )
            {
                if ( m_typedBackingFields[ t ].ContainsKey( key ) )
                {
                    val = (T)m_typedBackingFields[ t ][ key ];
                    return true;
                }
            }

            val = default(T);
            return false;
        }

         
        public void SetValue< T >( string key, T val )
        {
            System.Type t = typeof( T );

            if ( !m_typedBackingFields.ContainsKey( t ) )
            {
                m_typedBackingFields.Add( t, new Dictionary<string,object>() );
                m_typedBackingFields[ t ].Add( key, val );
            }
            else
            {
                if ( !m_typedBackingFields[ t ].ContainsKey( key ) )
                {
                    m_typedBackingFields[ t ].Add( key, val );
                }
                else
                {
                    m_typedBackingFields[ t ][ key ] = val;
                }
            }
        }            
    }


    public class BlackboardCache
    {
        private Blackboard                                   m_globalScope;
        private Dictionary< BehaviorTree, Blackboard >       m_treeScope;
        private Dictionary< BehaviorTreeNode, Blackboard >   m_nodeScope;


        public BlackboardCache()
        {
            m_globalScope = new Blackboard();
            m_treeScope   = new Dictionary<BehaviorTree,Blackboard>();
            m_nodeScope   = new Dictionary<BehaviorTreeNode,Blackboard>();
        }


        public Blackboard GetBlackboard()
        {
            return m_globalScope;
        }


        public Blackboard GetBlackboard( BehaviorTree tree )
        {
            if ( !m_treeScope.ContainsKey( tree ) )
            {
                m_treeScope.Add( tree, new Blackboard() );
            }

            return m_treeScope[ tree ];
        }


        public Blackboard GetBlackboard( BehaviorTreeNode node )
        {
            if ( !m_nodeScope.ContainsKey( node ) )
            {
                m_nodeScope.Add( node, new Blackboard() );
            }

            return m_nodeScope[ node ];
        }
    }

}