using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using BehaviorTrees;
using UForms.Controls;
using UForms.Controls.Fields;
using UForms.Controls.Dropdowns;

namespace BehaviorTreesEditor
{
    public class BehaviorTreeGenerator
    {
        private const string TEMPLATE =
        // {0} - generator timestamp
        // {1} - behaviorId safe handle
        // {2} - behaviorId
        // {3} - generated code

@"using UnityEngine;
using BehaviorTrees;

/*
*   Behavior Tree: {2}                         
*   This class has been auto generated on {0}  
*   DO NOT MODIFY MANUALLY!                    
*/

public static class BehaviorTree{1} 
{{
    [BehaviorGenerator( ""{2}"")]
    public static BehaviorTree Generate()
    {{
        BehaviorTree t = new BehaviorTree();
        {3}
        t.Root = n1;
        return t;
    }}
}}
";
        private Dictionary< BehaviorNodeControl, string > m_indexTable;

        public string Generate( string id, string safeHandle, BehaviorNodeControl root )
        {
            m_indexTable = new BehaviorTreeNodeIndexer( root ).IndexTable;            

            string code = "";

            foreach( BehaviorNodeControl node in m_indexTable.Keys )
            {
                code = string.Format( "{0}\n{1}\n", code, GenerateNodeCode( node ) );
            }

            foreach ( BehaviorNodeControl node in m_indexTable.Keys )
            {
                code = string.Format( "{0}{1}", code, GenerateChildLinks( node ) );
            }

            return string.Format(
                TEMPLATE, System.DateTime.Now, safeHandle, id, code
            );
        }

        private string GenerateNodeCode( BehaviorNodeControl node )
        {
            EditorCachedNode data = node.GetData();

            string s = "\n";
            s = string.Format( "\t\t{0} {1} = new {0}( t );", data.type, m_indexTable[ node ] );

            foreach( EditorCachedNodeField field in data.fields )
            {
                if ( node.m_fieldControls.ContainsKey( field.name ) )
                {
                    Control c = node.m_fieldControls[ field.name ];

                    if ( field.type == typeof( int ) )
                    {                        
                        s += string.Format( "\n\t\t{0}.{1} = {2};", m_indexTable[ node ], field.systemName, ( c as IntField ).Value );
                    }
                    else if ( field.type == typeof( float ) )
                    {
                        s += string.Format( "\n\t\t{0}.{1} = {2}f;", m_indexTable[ node ], field.systemName, ( c as FloatField ).Value );
                    }
                    else if ( field.type == typeof( Vector2 ) )
                    {
                        s += string.Format( "\n\t\t{0}.{1} = new Vector2({2}f,{3}f);", m_indexTable[ node ], field.systemName, ( c as Vector2Field ).Value.x, ( c as Vector2Field ).Value.y );
                    }
                    else if ( field.type == typeof( Vector3 ) )
                    {
                        s += string.Format( "\n\t\t{0}.{1} = new Vector3({2}f,{3}f,{4}f);", m_indexTable[ node ], field.systemName, ( c as Vector3Field ).Value.x, ( c as Vector3Field ).Value.y, ( c as Vector3Field ).Value.z );
                    }
                    else if ( field.type == typeof( Vector4 ) )
                    {
                        s += string.Format( "\n\t\t{0}.{1} = new Vector4({2}f,{3}f,{4}f,{5}f);", m_indexTable[ node ], field.systemName, ( c as Vector4Field ).Value.x, ( c as Vector4Field ).Value.y, ( c as Vector4Field ).Value.z, ( c as Vector4Field ).Value.w );
                    }
                    else if ( field.type == typeof( string ) )
                    {
                        s += string.Format( "\n\t\t{0}.{1} = \"{2}\";", m_indexTable[ node ], field.systemName, ( c as TextField ).Value );
                    }
                    else if ( field.type == typeof( Rect ) )
                    {
                        s += string.Format( "\n\t\t{0}.{1} = new Rect({2}f,{3}f,{4}f,{5}f);", m_indexTable[ node ], field.systemName, ( c as RectField ).Value.xMin, ( c as RectField ).Value.yMin, ( c as RectField ).Value.width, ( c as RectField ).Value.height );                        
                    }
                    else if ( field.type == typeof( Color ) )
                    {
                        s += string.Format( "\n\t\t{0}.{1} = new Color({2},{3},{4},{5});", m_indexTable[ node ], field.systemName, ( c as ColorField ).Value.r, ( c as ColorField ).Value.g, ( c as ColorField ).Value.b, ( c as ColorField ).Value.a );
                    }
                    else if ( field.type == typeof( Bounds ) )
                    {
                        s += string.Format( "\n\t\t{0}.{1} = new Bounds( new Vector3({2}f,{3}f,{4}f),new Vector3({5}f,{6}f,{7}f));", m_indexTable[ node ], field.systemName, ( c as BoundsField ).Value.center.x, ( c as BoundsField ).Value.center.y, ( c as BoundsField ).Value.center.z, ( c as BoundsField ).Value.extents.x, ( c as BoundsField ).Value.extents.y, ( c as BoundsField ).Value.extents.z );
                    }
                    else if ( field.type.IsEnum )
                    {
                        s += string.Format( "\n\t\t{0}.{1} = {2}.{3};", m_indexTable[ node ], field.systemName, field.type.ToString().Replace("+","."), ( c as EnumDropdown ).Value );
                    }
                    else if ( field.type == typeof( bool ) )
                    {
                        s += string.Format( "\n\t\t{0}.{1} = {2};", m_indexTable[ node ], field.systemName, ( c as Toggle ).Value.ToString().ToLowerInvariant() );
                    }
                }
            }

            return s;
        }


        private string GenerateChildLinks( BehaviorNodeControl node )
        {
            string s = "";

            if ( node.m_outputs.Count > 0 )
            {
                s = "\n";
            }

            foreach( BehaviorNodeControl o in node.m_outputs )
            {
                s = string.Format( "{0}\t\t{1}.Children.Add({2});\n", s, m_indexTable[ node ], m_indexTable[ o ] );
            }

            return s;
        }
    }
}