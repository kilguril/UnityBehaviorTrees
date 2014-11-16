using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using UForms;
using UForms.Application;
using UForms.Controls;
using UForms.Events;
using UForms.Decorators;
using UForms.Graphics;

using BehaviorTrees;

namespace BehaviorTreesEditor
{
    public class BehaviorTreeControl : Control
    {
        public EditorNodeTypeCache TypeCahce { get { return m_nodeCache; } }

        private Vector2     m_mousePosition;
        EditorNodeTypeCache m_nodeCache;

        private List<BehaviorNodeControl>  m_nodes;

        private BehaviorNodeControl  m_linkFrom;
        private int                  m_linkFromIndex;
        private BezierCurve          m_linkLine;

        private GraphicsCanvas       m_canvas;

        public BehaviorTreeControl( ) : base()
        {
            m_nodes = new List< BehaviorNodeControl >();

            m_nodeCache = new EditorNodeTypeCache();
            m_nodeCache.CacheAvailableNodes();

            AddDecorator( new Scrollbars() );
            m_canvas = ( GraphicsCanvas )AddDecorator( new GraphicsCanvas() );

            m_linkLine = ( BezierCurve )m_canvas.AddShape( new BezierCurve( Vector2.zero, Vector2.zero, Color.red, 1.0f, BezierCurve.TangentMode.AutoY, Vector2.zero, Vector2.zero ) );
            m_linkLine.Tangents = BezierCurve.TangentMode.AutoY;

            ContextMenuControl ctx = new ContextMenuControl();            
            foreach ( EditorCachedNode node in m_nodeCache.Cache )
            {
                ctx.Menu.AddItem( new GUIContent( node.displayName ), false, AddNode, node );
            }
            ctx.Positionless = true;
            AddChild( ctx );
        }


        public BehaviorNodeControl GetRootControl()
        {
            List< BehaviorNodeControl > candidates = new List<BehaviorNodeControl>( m_nodes );

            foreach( BehaviorNodeControl node in m_nodes )
            {
                foreach( BehaviorNodeControl output in node.m_outputs )
                {
                    if ( candidates.Contains( output ) )
                    {
                        candidates.Remove( output );
                    }
                }
            }

            if ( candidates.Count != 1 )
            {
                Debug.LogWarning( "Couldn't determine root node, make sure there is only 1" );
                return null;
            }

            return candidates[ 0 ];
        }


        public void AddNodeGroup( BehaviorNodeControl[] nodes )
        {
            foreach( BehaviorNodeControl node in nodes )
            {
                node.DragMoved += NodeDragMoved;
                node.OutputClicked += NodeOutputClicked;
                node.RemoveRequested += NodeRemoveRequested;

                AddChild( node );
                m_nodes.Add( node );
            }
        }


        public void ClearAll()
        {
            foreach( BehaviorNodeControl node in m_nodes )
            {
                node.Remove();
                node.DragMoved -= NodeDragMoved;
                node.OutputClicked -= NodeOutputClicked;
                node.RemoveRequested -= NodeRemoveRequested;

                RemoveChild( node );                
            }

            m_nodes.Clear();
        }


        private void AddNode( object cachedNode )
        {
            if ( cachedNode is EditorCachedNode )
            {
                BehaviorNodeControl node = new BehaviorNodeControl( ( EditorCachedNode )cachedNode );
                node.Position = m_mousePosition;

                node.DragMoved += NodeDragMoved;
                node.OutputClicked += NodeOutputClicked;
                node.RemoveRequested += NodeRemoveRequested;

                AddChild( node );
                m_nodes.Add( node );
            }
        }


        void NodeRemoveRequested( BehaviorNodeControl node )
        {
            if ( m_linkFrom == node )
            {
                m_linkFrom = null;
                m_linkLine.From = Vector2.zero;
                m_linkLine.To = Vector2.zero;
            }

            foreach( BehaviorNodeControl c in m_nodes )
            {
                if ( c != node )
                {
                    for ( int i =  c.m_outputs.Count - 1; i >= 0; i-- )
                    {
                        if ( c.m_outputs[ i ] == node )
                        {
                            c.ClearOutput( i );
                        }
                    }
                }
            }

            node.Remove();
            node.DragMoved       -= NodeDragMoved;
            node.OutputClicked   -= NodeOutputClicked;
            node.RemoveRequested -= NodeRemoveRequested;            

            RemoveChild( node );

            m_nodes.Remove( node );
        }


        void NodeOutputClicked( BehaviorNodeControl node, int index, MouseButton mouseButton )
        {
            switch ( mouseButton )
            {
                case MouseButton.Left:
                    m_linkFrom = node;
                    m_linkFromIndex = index;
                break;

                case MouseButton.Right:
                    if ( m_linkFrom == null )
                    {
                        node.ClearOutput( index );
                    }
                    else
                    {
                        m_linkFrom = null;
                        m_linkLine.From = Vector2.zero;
                        m_linkLine.To = Vector2.zero;
                    }
                break;
            }
        }

        private void NodeDragMoved( IDraggable sender, DragEventArgs args, Event nativeEvent )
        {
            if ( sender is BehaviorNodeControl )
            {
                BehaviorNodeControl node = sender as BehaviorNodeControl;

                switch( nativeEvent.button )
                {
                    case 0:
                    {
                        Vector2 newPos = node.Position + args.delta;

                        if ( newPos.x >= 0.0f && newPos.y >= 0.0f )
                        {
                            node.SetPosition( newPos );
                        }
                    }
                    break;

                    case 2:
                    {
                        Vector2 newPos = node.Position + args.delta;

                        if ( newPos.x >= 0.0f && newPos.y >= 0.0f )
                        {
                            DragHierarchy( node, args.delta );
                        }
                    }
                    break;
                }
            }
        }


        private void DragHierarchy( BehaviorNodeControl c, Vector2 delta )
        {
            c.SetPosition( c.Position + delta );

            foreach( BehaviorNodeControl o in c.m_outputs )
            {
                if ( o != null )
                {
                    DragHierarchy( o, delta );
                }
            }
        }


        protected override void OnMouseDown( Event e )
        {
            switch( e.button )
            {
                case 0:
                    if ( m_linkFrom != null )
                    {
                        BehaviorNodeControl node = IsPointOnNode( e.mousePosition );
                        
                        if ( node != null )
                        {
                            m_linkFrom.SetOutput( node, m_linkFromIndex );
                        }

                        m_linkFrom = null;
                        m_linkLine.From = Vector2.zero;
                        m_linkLine.To = Vector2.zero;

                        e.Use();
                    }
                break;

                case 1:
                    if ( m_linkFrom != null )
                    {
                        m_linkFrom = null;
                        m_linkLine.From = Vector2.zero;
                        m_linkLine.To = Vector2.zero;

                        e.Use();
                    }
                break;
            }
        }


        protected override void OnMouseMove( Event e )
        {
            m_mousePosition = e.mousePosition;
        }


        protected override void OnUpdate()
        {
            if ( m_linkFrom != null )
            {
                if ( m_linkLine != null )
                {
                    m_linkLine.From = m_linkFrom.GetOutputConnectorPosition( m_linkFromIndex ) - m_linkFrom.ViewportOffset;
                    m_linkLine.To = m_mousePosition;
                    Dirty = true;
                }
            }
        }


        private BehaviorNodeControl IsPointOnNode( Vector2 p )
        {
            foreach( BehaviorNodeControl node in m_nodes )
            {
                if ( node.PointInControl( p ) )
                {
                    return node;
                }
            }

            return null;
        }
    }

}