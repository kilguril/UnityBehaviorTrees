using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using BehaviorTrees;

namespace BehaviorTreesEditor
{

    public struct EditorCachedNodeField
    {
        public Type     type;
        public string   name;
        public string   systemName;
    }

    public struct EditorCachedNode
    {
        public Type                             type;
        public BehaviorNodeAttribute.NodeType   nodeType;
        public string                           displayName;
        public EditorCachedNodeField[]          fields;
    }

    public class EditorNodeTypeCache
    {
        public EditorCachedNode[] Cache { get { return m_nodeCache.ToArray(); } }

        private List< EditorCachedNode > m_nodeCache;

        public EditorNodeTypeCache()
        {
            m_nodeCache = new List<EditorCachedNode>();
        }

        public void CacheAvailableNodes()
        {
            AppDomain domain = AppDomain.CurrentDomain;
            Assembly[] asms = domain.GetAssemblies();

            foreach( Assembly asm in asms )
            {
                Type[] types = asm.GetTypes();

                foreach( Type type in types )
                {
                    object[] attribs = type.GetCustomAttributes( typeof(BehaviorNodeAttribute), false );
                    if ( attribs.Length > 0 )
                    {
                        CacheType( type );
                    }
                }
            }
        }

        private void CacheType( Type type )
        {
            object[] attribs = type.GetCustomAttributes( typeof( BehaviorNodeAttribute ), false );
            BehaviorNodeAttribute nodeAttrib = ( BehaviorNodeAttribute )attribs[ 0 ];

            List< EditorCachedNodeField > fieldList = new List<EditorCachedNodeField>();

            FieldInfo[] fields = type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            foreach ( FieldInfo f in fields )
            {
                object[] fattribs = f.GetCustomAttributes( typeof( BehaviorNodeParameterAttribute ), false );

                if ( fattribs.Length > 0 )
                {
                    BehaviorNodeParameterAttribute fattrib = ( BehaviorNodeParameterAttribute )fattribs[ 0 ];

                    EditorCachedNodeField cachedField;
                    cachedField.name        = fattrib.displayName;
                    cachedField.type        = f.FieldType;
                    cachedField.systemName  = f.Name;

                    fieldList.Add( cachedField );
                }
            }

            EditorCachedNode cachedNode;
            cachedNode.displayName = nodeAttrib.displayName;
            cachedNode.nodeType    = nodeAttrib.nodeType;
            cachedNode.type        = type;
            cachedNode.fields      = fieldList.ToArray();

            m_nodeCache.Add( cachedNode );
        }
    }

}