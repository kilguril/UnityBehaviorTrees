using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using UForms;
using UForms.Controls;
using UForms.Controls.Fields;
using UForms.Controls.Dropdowns;
using UForms.Events;
using UForms.Graphics;

namespace BehaviorTreesEditor
{
    public class BehaviorNodeControl : Control, IDraggable
    {
        private const float LINE_SIZE       = 18.0f;
        private const float BOX_WIDTH       = 200.0f;
        private const float INNER_PADDING   = 10.0f;
        private const float HEADER_SIZE     = 22.0f;
        private const float FOOTER_PADDING  = 8.0f;
        private const float BUTTON_SIZE     = 18.0f;

        protected override Vector2 DefaultSize
        {
            get
            {
                return new Vector2( BOX_WIDTH, LINE_SIZE );
            }
        }

        public delegate void OutputClick( BehaviorNodeControl node, int index, MouseButton mouseButton );
        public delegate void RemoveRequest( BehaviorNodeControl node );

        public event OutputClick OutputClicked;
        public event RemoveRequest RemoveRequested;

        public event Drag DragEnded;
        public event Drag DragMoved;
        public event Drag DragStarted;

        private EditorCachedNode    m_node;
        private bool                m_dragging;
        private Vector2             m_dragStartPosition;

        public  List< BehaviorNodeControl > m_outputs;
        public  Dictionary< string, Control > m_fieldControls;

        private List< Button >              m_outputButtons;
        private List< BezierCurve >         m_outputLines;

        private GraphicsCanvas              m_canvas;

        public BehaviorNodeControl( EditorCachedNode node )
        {
            m_fieldControls = new Dictionary<string, Control>();
            m_node = node;            

            int     lines    = 0;

            foreach( EditorCachedNodeField field in node.fields )
            {
                Control fieldControl = null;
                int l = 0;

                // Create fields per type
                if ( field.type == typeof( int ) )
                {
                    fieldControl = AddChild( new IntField( default( int ), field.name ) );
                    l = 1;
                }
                else if ( field.type == typeof( float ) )
                {
                    fieldControl = AddChild( new FloatField( default( float ), field.name ) );
                    l = 1;
                }
                else if ( field.type == typeof( Vector2 ) )
                {
                    fieldControl = AddChild( new Vector2Field( default( Vector2 ), field.name ) );
                    l = 2;
                }
                else if ( field.type == typeof( Vector3 ) )
                {
                    fieldControl = AddChild( new Vector3Field( default( Vector3 ), field.name ) );
                    l = 2;
                }
                else if ( field.type == typeof( Vector4 ) )
                {
                    fieldControl = AddChild( new Vector4Field( default( Vector4 ), field.name ) );
                    l = 2;
                }
                else if ( field.type == typeof( string ) )
                {
                    fieldControl = AddChild( new TextField( "", field.name ) );
                    l = 1;
                }
                else if ( field.type == typeof( Rect ) )
                {
                    fieldControl = AddChild( new RectField( default( Rect ), field.name ) );
                    l = 3;
                }
                else if ( field.type == typeof( Color ) )
                {
                    fieldControl = AddChild( new ColorField( default( Color ), field.name ) );
                    l = 1;
                }
                else if ( field.type == typeof( Bounds ) )
                {
                    fieldControl = AddChild( new BoundsField( default( Bounds ), field.name ) );
                    l = 3;
                }
                else if ( field.type.IsEnum )
                {
                    fieldControl = AddChild( new EnumDropdown( ( System.Enum )Activator.CreateInstance( field.type ), field.name ) );
                    l = 1;
                }
                else if ( field.type == typeof( bool ) )
                {
                    fieldControl = AddChild( new Toggle( field.name, false, false ) );
                    l = 1;
                }
                else
                {
                    Debug.LogWarning( string.Format( "Unsupported field type `{0}({1})` encountered in node `{2}`", field.name, field.type, node.displayName ) );
                }

                if ( fieldControl != null )
                {
                    m_fieldControls.Add( field.name, fieldControl );

                    fieldControl
                    .SetPosition( INNER_PADDING, HEADER_SIZE + LINE_SIZE * lines )
                    .SetSize( BOX_WIDTH - INNER_PADDING - INNER_PADDING, LINE_SIZE );

                    lines += l;
                }
            }

            if ( m_node.nodeType != BehaviorTrees.BehaviorNodeAttribute.NodeType.Leaf )
            { 
                lines++;    // Add extra line of padding at the bottom
            }

            m_canvas = (GraphicsCanvas)AddDecorator( new GraphicsCanvas() );

            SetHeight( HEADER_SIZE + FOOTER_PADDING + LINE_SIZE * lines );

            // Create outputs
            m_outputs = new List<BehaviorNodeControl>();
            m_outputButtons = new List<Button>();
            m_outputLines   = new List<BezierCurve>();

            UpdateOutputs();
        }                

        public EditorCachedNode GetData()
        {
            return m_node;
        }


        public void SetOutput( BehaviorNodeControl target, int index )
        {
            // Not self
            if ( target == this )
            {
                return;
            }

            // Make sure connection does not already exist
            foreach( BehaviorNodeControl o in m_outputs )
            {
                if ( o == target )
                {
                    return;
                }
            }

            // Make sure there are no circular references
            foreach( BehaviorNodeControl o in target.m_outputs )
            {
                if ( CheckCircularReference( this, o ) )
                {
                    return;
                }
            }

            if ( index < m_outputs.Count )
            {
                // Replace
                m_outputs[ index ] = target;                
            }
            else
            {
                // New connection

                m_outputs.Add( target );
                BezierCurve line = (BezierCurve)m_canvas.AddShape( new BezierCurve( GetOutputConnectorPosition( index ), target.GetInputConnectorPosition(), Color.red, 1.0f, BezierCurve.TangentMode.AutoY, Vector2.zero, Vector2.zero ) );
                m_outputLines.Add( line );

                UpdateOutputs();
            }
        }


        private bool CheckCircularReference( BehaviorNodeControl target, BehaviorNodeControl source )
        {
            if ( source == target )
            {
                return true;
            }

            foreach( BehaviorNodeControl child in source.m_outputs )
            {
                if ( CheckCircularReference( target, child ) )
                {
                    return true;
                }
            }

            return false;
        }


        public void Remove()
        {
            for ( int i = m_outputs.Count - 1; i >= 0; i-- )
            {
                ClearOutput( i );
            }
        }


        public void ClearOutput( int index )
        {
            if ( index < m_outputs.Count )
            {
                m_outputs.RemoveAt( index );
                m_canvas.RemoveShape( m_outputLines[ index ] );
                m_outputLines.RemoveAt( index );

                if ( m_node.nodeType == BehaviorTrees.BehaviorNodeAttribute.NodeType.Composite )
                {
                    m_outputButtons[ index ].Clicked -= OnOutputClicked;
                    RemoveChild( m_outputButtons[ index ] );
                    m_outputButtons.RemoveAt( index );

                    UpdateOutputs();
                }
            }
        }


        public Vector2 GetInputConnectorPosition()
        {
            return Position + new Vector2(
                BOX_WIDTH / 2.0f,
                0.0f
            );
        }


        public Vector2 GetOutputConnectorPosition( int index )
        {
            float offset = BOX_WIDTH / 2.0f - m_outputButtons.Count * BUTTON_SIZE + BUTTON_SIZE / 2.0f;

            return Position + new Vector2(
                offset + BUTTON_SIZE * 2 * index + BUTTON_SIZE / 2.0f,
                Size.y
            );
        }

        protected override void OnBeforeDraw()
        {
            GUI.Box( ScreenRect, m_node.displayName );
        }

        protected override void OnDraw()
        {
            for ( int i = 0; i < m_outputLines.Count; i++ )
            {
                m_outputLines[ i ].From = GetOutputConnectorPosition( i );
                m_outputLines[ i ].To   = m_outputs[ i ].GetInputConnectorPosition();
            }            
        }


        protected override void OnMouseDown( Event e )
        {
            if ( PointInControl( e.mousePosition ) && !m_dragging )
            {
                switch( e.button )
                { 
                    case 0:
                    case 2:

                        m_dragging = true;
                        m_dragStartPosition = Position;

                        if ( DragStarted != null )
                        {
                            DragStarted( this, new DragEventArgs( m_dragStartPosition, Position, Vector2.zero ), e );
                        }                    

                        e.Use();

                    break;                    

                    case 1:
                        if ( RemoveRequested != null )
                        {
                            RemoveRequested( this );
                        }

                        m_dragging = true;

                        e.Use();
                    break;
                }
            }
        }


        protected override void OnMouseDrag( Event e )
        {
            if ( m_dragging )
            {
                if ( DragMoved != null )
                {
                    DragMoved( this, new DragEventArgs( m_dragStartPosition, Position, e.delta ), e );
                }

                e.Use();
            }
        }


        protected override void OnMouseUp( Event e )
        {
            if ( m_dragging )
            {
                m_dragging = false;

                if ( DragEnded != null )
                {
                    DragEnded( this, new DragEventArgs( m_dragStartPosition, Position, e.delta ), e );
                }

                e.Use();
            }

        }


        private void UpdateOutputs()
        {
            int targetOutputs = 0;

            switch( m_node.nodeType )
            {
                case BehaviorTrees.BehaviorNodeAttribute.NodeType.Leaf:
                    targetOutputs = 0;
                break;

                case BehaviorTrees.BehaviorNodeAttribute.NodeType.Decorator:
                    targetOutputs = 1;
                break;

                case BehaviorTrees.BehaviorNodeAttribute.NodeType.Composite:
                    targetOutputs = m_outputs.Count + 1;
                break;
            }

            while ( m_outputButtons.Count < targetOutputs )
            {
                AddButton();
            }

            // Do layout
            float offset = BOX_WIDTH / 2.0f - m_outputButtons.Count * BUTTON_SIZE + BUTTON_SIZE / 2.0f;

            for ( int i = 0; i< m_outputButtons.Count; i++ )
            {
                m_outputButtons[ i ].SetPosition( offset + BUTTON_SIZE * 2 * i, Size.y - BUTTON_SIZE / 2.0f );
            }
        }
        

        private void AddButton()
        {
            Button b = new Button();

            b.SetSize( BUTTON_SIZE, BUTTON_SIZE );

            AddChild( b );
            m_outputButtons.Add( b );

            b.Clicked += OnOutputClicked;
        }


        void OnOutputClicked( IClickable sender, ClickEventArgs args, Event nativeEvent )
        {
            if ( sender is Button )
            {
                Button b = sender as Button;
                int index = m_outputButtons.IndexOf( b );

                if ( index >= 0 )
                {
                    if ( OutputClicked != null )
                    {
                        OutputClicked( this, index, args.button );
                    }
                }
            }
        }

    }

}