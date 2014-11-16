using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UForms.Controls;
using UForms.Controls.Fields;
using UForms.Controls.Dropdowns;
using System;

namespace BehaviorTreesEditor
{
    public class BehaviorTreeXMLSerializer
    {
        private const string TEMPLATE = 
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<BehaviorTreeLayout id=""{0}"">
{1}
</BehaviorTreeLayout>
";

        private const string NODE_TEMPLATE =
@"  <Node id=""{0}"" position=""{1}"" type=""{2}"" {3} {4} />";


        private Dictionary< BehaviorNodeControl, string > m_indexTable;
        private Dictionary< BehaviorNodeControl, List< string > > m_childMapping;

        public string Serialize( string id, BehaviorNodeControl root )
        {
            m_indexTable = new BehaviorTreeNodeIndexer( root ).IndexTable;

            string nodes = "";
            foreach( BehaviorNodeControl node in m_indexTable.Keys )
            {
                nodes = string.Format( "{0}\n{1}", nodes, SerializeNode( node ) );
            }
            nodes = string.Format( "{0}\n", nodes );

            return string.Format( TEMPLATE, id, nodes );
        }

        private string SerializeNode( BehaviorNodeControl node )
        {
            EditorCachedNode data = node.GetData();

            string id           = m_indexTable[ node ]; ;
            string position     = string.Format( "{0},{1}", node.Position.x, node.Position.y );
            string type         = data.type.ToString();
            string attribs      = "";
            string children     = "";

            for ( int i = 0; i < data.fields.Length; i++ )
            {
                if ( i > 0 )
                {
                    attribs += " ";
                }

                attribs += SerializeAttribute( node, data.fields[ i ] );                
            }

            if ( node.m_outputs.Count > 0 )
            {
                children = "outputs=\"";
            }

            for ( int i = 0; i < node.m_outputs.Count; i++ )
            {
                if ( i > 0 )
                {
                    children += ",";
                }

                children += m_indexTable[ node.m_outputs[ i ] ];
            }

            if ( node.m_outputs.Count > 0 )
            {
                children += "\"";
            }

            return string.Format( NODE_TEMPLATE, id, position, type, attribs, children ); ;
        }

        private string SerializeAttribute( BehaviorNodeControl node, EditorCachedNodeField field )
        {
            if ( node.m_fieldControls.ContainsKey( field.name ) )
            {
                Control c = node.m_fieldControls[ field.name ];

                if ( field.type == typeof( int ) )
                {
                    return string.Format( "{0}=\"{1}\"", field.systemName, ( c as IntField ).Value );                    
                }
                else if ( field.type == typeof( float ) )
                {
                    return string.Format( "{0}=\"{1}\"", field.systemName, ( c as FloatField ).Value );                    
                }
                else if ( field.type == typeof( Vector2 ) )
                {
                    return string.Format( "{0}=\"{1},{2}\"", field.systemName, ( c as Vector2Field ).Value.x, ( c as Vector2Field ).Value.y );                      
                }
                else if ( field.type == typeof( Vector3 ) )
                {
                    return string.Format( "{0}=\"{1},{2},{3}\"", field.systemName, ( c as Vector3Field ).Value.x, ( c as Vector3Field ).Value.y, ( c as Vector3Field ).Value.z );                                          
                }
                else if ( field.type == typeof( Vector4 ) )
                {
                    return string.Format( "{0}=\"{1},{2},{3}\"", field.systemName, ( c as Vector4Field ).Value.x, ( c as Vector4Field ).Value.y, ( c as Vector4Field ).Value.z, ( c as Vector4Field ).Value.w );                                          
                }
                else if ( field.type == typeof( string ) )
                {
                    return string.Format( "{0}=\"{1}\"", field.systemName, ( c as TextField ).Value );                    
                }
                else if ( field.type == typeof( Rect ) )
                {
                    return string.Format( "{0}=\"{1},{2},{3},{4}\"", field.systemName, ( c as RectField ).Value.xMin, ( c as RectField ).Value.yMin, ( c as RectField ).Value.width, ( c as RectField ).Value.height );                    
                }
                else if ( field.type == typeof( Color ) )
                {
                    return string.Format( "{0}=\"{1},{2},{3}\"", field.systemName, ( c as ColorField ).Value.r, ( c as ColorField ).Value.g, ( c as ColorField ).Value.b, ( c as ColorField ).Value.a );
                }
                else if ( field.type == typeof( Bounds ) )
                {
                    return string.Format( "{0}=\"{1},{2},{3},{4},{5},{6}\"", field.systemName, ( c as BoundsField ).Value.center.x, ( c as BoundsField ).Value.center.y, ( c as BoundsField ).Value.center.z, ( c as BoundsField ).Value.extents.x, ( c as BoundsField ).Value.extents.y, ( c as BoundsField ).Value.extents.z );                    
                }
                else if ( field.type.IsEnum )
                {
                    return string.Format( "{0}=\"{1}\"", field.systemName, ( c as EnumDropdown ).Value );                    
                }
                else if ( field.type == typeof( bool ) )
                {
                    return string.Format( "{0}=\"{1}\"", field.systemName, ( c as Toggle ).Value );                    
                }
            }

            return "";
        }


        public bool Deserialize( string path, EditorNodeTypeCache typeCache, out string id, out BehaviorNodeControl[] controls )
        {
            id = "";
            controls = new BehaviorNodeControl[ 0 ];

            m_childMapping = new Dictionary<BehaviorNodeControl, List<string>>();

            try
            {
                m_indexTable = new Dictionary<BehaviorNodeControl, string>();

                XmlDocument xml = new XmlDocument();
                xml.Load( path );

                XmlNode xmlRoot = xml.SelectSingleNode( "BehaviorTreeLayout" );
                id = xmlRoot.Attributes[ "id" ].InnerText;

                List< BehaviorNodeControl > deserialized = new List<BehaviorNodeControl>();
                XmlNodeList xmlNodes = xmlRoot.SelectNodes( "Node" );
                foreach( XmlNode xmlNode in xmlNodes )
                {
                    string nodeId;
                    List< string > children;
                    BehaviorNodeControl c = DeserializeNode( xmlNode, typeCache, out nodeId, out children );

                    if ( c != null )
                    {
                        m_indexTable.Add( c, nodeId );
                        m_childMapping.Add( c, children );
                        deserialized.Add( c );
                    }
                    else
                    {
                        Debug.LogWarning( "Invalid node encountered, not all nodes have been deserialized" );
                    }
                }

                // Map outputs
                foreach( BehaviorNodeControl c in deserialized )
                {
                    if ( m_childMapping.ContainsKey( c ) )
                    {
                        int i = 0;
                        foreach( string cid in m_childMapping[ c ] )
                        {
                            foreach( KeyValuePair< BehaviorNodeControl, string > kvp in m_indexTable )
                            {
                                if ( kvp.Value == cid )
                                {
                                    c.SetOutput( kvp.Key, i );
                                    i++;
                                    break;
                                }
                            }
                        }
                    }
                }

                controls = deserialized.ToArray();

                return true;
            }
            catch( System.Exception e )
            {
                Debug.LogError( string.Format( "Could not load layout at `{0}` - {1}", path, e.Message ) );
            }

            return false;
        }


        private BehaviorNodeControl DeserializeNode( XmlNode xml, EditorNodeTypeCache typeCache, out string nodeId, out List<string> childIds )
        {
            childIds = new List<string>();

            nodeId           = xml.Attributes[ "id" ].InnerText;
            string type      = xml.Attributes[ "type" ].InnerText;
            string[] posStr  = xml.Attributes[ "position" ].InnerText.Split(',');
            Vector2 position = new Vector2( float.Parse( posStr[ 0 ] ), float.Parse( posStr[ 1 ] ) );

            if ( xml.Attributes[ "outputs" ] != null )
            {
                childIds.AddRange( xml.Attributes[ "outputs" ].InnerText.Split( ',' ) );
            }

            bool found = false;
            EditorCachedNode data = default(EditorCachedNode);

            foreach( EditorCachedNode cachedType in typeCache.Cache )
            {
                if ( cachedType.type.ToString() == type )
                {
                    found = true;
                    data = cachedType;
                    break;
                }
            }

            if ( !found )
            {                
                return null;
            }

            BehaviorNodeControl control = new BehaviorNodeControl( data );
            control.SetPosition( position );
            DeserializeFields( xml, data, control );

            return control;
        }


        private void DeserializeFields( XmlNode xml, EditorCachedNode data, BehaviorNodeControl control )
        {
            foreach( KeyValuePair< string, Control > kvp in control.m_fieldControls )
            {
                string fieldName  = kvp.Key;
                string systemName = "";
                Control c         = kvp.Value;

                bool found = false;
                EditorCachedNodeField field = default(EditorCachedNodeField);

                foreach( EditorCachedNodeField f in data.fields )
                {
                    if ( f.name == fieldName )
                    {
                        found = true;
                        field = f;
                        systemName = f.systemName;
                        break;
                    }
                }

                if ( !found )
                {
                    continue;
                }

                if ( xml.Attributes[ systemName ] == null )
                {
                    continue;
                }

                string sval = xml.Attributes[ systemName ].InnerText;

                if ( field.type == typeof( int ) )
                {
                    ( c as IntField ).Value = int.Parse( sval );
                }
                else if ( field.type == typeof( float ) )
                {
                    ( c as FloatField ).Value = float.Parse( sval );
                }
                else if ( field.type == typeof( Vector2 ) )
                {
                    string[] split = sval.Split( ',' );
                    ( c as Vector2Field ).Value = new Vector2( float.Parse( split[ 0 ] ), float.Parse( split[ 1 ] ) );                    
                }
                else if ( field.type == typeof( Vector3 ) )
                {
                    string[] split = sval.Split( ',' );
                    ( c as Vector3Field ).Value = new Vector3( float.Parse( split[ 0 ] ), float.Parse( split[ 1 ] ), float.Parse( split[ 2 ] ) );
                }
                else if ( field.type == typeof( Vector4 ) )
                {
                    string[] split = sval.Split( ',' );
                    ( c as Vector4Field ).Value = new Vector4( float.Parse( split[ 0 ] ), float.Parse( split[ 1 ] ), float.Parse( split[ 2 ] ), float.Parse( split[ 3 ] ) );
                }
                else if ( field.type == typeof( string ) )
                {
                    ( c as TextField ).Value = sval;
                }
                else if ( field.type == typeof( Rect ) )
                {
                    string[] split = sval.Split( ',' );
                    ( c as RectField ).Value = new Rect( float.Parse( split[ 0 ] ), float.Parse( split[ 1 ] ), float.Parse( split[ 2 ] ), float.Parse( split[ 3 ] ) );                    
                }
                else if ( field.type == typeof( Color ) )
                {
                    string[] split = sval.Split( ',' );
                    ( c as ColorField ).Value = new Color( float.Parse( split[ 0 ] ), float.Parse( split[ 1 ] ), float.Parse( split[ 2 ] ) );
                }
                else if ( field.type == typeof( Bounds ) )
                {
                    string[] split = sval.Split( ',' );
                    ( c as BoundsField ).Value = new Bounds( new Vector3( float.Parse( split[ 0 ] ), float.Parse( split[ 1 ] ), float.Parse( split[ 2 ] ) ),  new Vector3( float.Parse( split[ 3 ] ), float.Parse( split[ 4 ] ), float.Parse( split[ 5 ] ) ) );
                }
                else if ( field.type.IsEnum )
                {
                    System.Enum e = (System.Enum)System.Enum.Parse( field.type, sval );
                    ( c as EnumDropdown ).Value = e;
                }
                else if ( field.type == typeof( bool ) )
                {
                    ( c as Toggle ).Value = bool.Parse( sval );
                }
            }
        }
    }
}