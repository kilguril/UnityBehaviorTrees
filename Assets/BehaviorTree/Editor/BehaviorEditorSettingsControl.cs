using UnityEngine;
using UnityEditor;
using System.Collections;

using UForms;
using UForms.Controls;
using UForms.Controls.Fields;
using UForms.Controls.Dropdowns;
using UForms.Events;
using UForms.Graphics;
using UForms.Decorators;

using BehaviorTrees;

namespace BehaviorTreesEditor
{    
    public class BehaviorEditorSettingsControl : Control
    {
        protected override Vector2 DefaultSize
        {
            get
            {
                return new Vector2( 200.0f, 200.0f );
            }
        }

        public Button NewButton { get { return m_newBehaviorButton; } }
        public Button SaveButton { get { return m_saveXmlButton; } }
        public Button LoadButton { get { return m_loadXmlButton; } }

        public Button GenerateButton { get { return m_generateButton; } }
        public TextField TreeIdField { get { return m_treeIdField; } }
        public TextField SavePathField { get { return m_savePathField; } }

        private Button m_newBehaviorButton;

        private Button m_saveXmlButton;
        private Button m_loadXmlButton;

        private Button m_generateButton;
        private TextField m_treeIdField;
        private TextField m_savePathField;

        public BehaviorEditorSettingsControl() : base()
        {
            AddDecorator( new StackContent( StackContent.StackMode.Vertical, StackContent.OverflowMode.Flow ) );

            AddChild( new Label( "Behavior ID:" ).SetMargin( 5.0f, 5.0f, 5.0f, 5.0f ).SetHeight( 24.0f ) );            
            m_treeIdField = (TextField)AddChild( new TextField().SetMargin( 5.0f, 0, 5.0f, 0 ).SetWidth( 100.0f, MetricsUnits.Percentage ) );

            AddChild( new Label( "Output Path:" ).SetMargin( 5.0f, 5.0f, 5.0f, 5.0f ).SetHeight( 24.0f ) );
            m_savePathField = ( TextField )AddChild( new TextField( EditorPrefs.HasKey( "BTE_SavePath" ) ? EditorPrefs.GetString( "BTE_SavePath" ) : "" ).SetMargin( 5.0f, 0.0f, 5.0f, 0.0f ).SetWidth( 100.0f, MetricsUnits.Percentage ) );

            m_generateButton = (Button )AddChild( new Button( "Generate Behavior" ).SetMargin( 5.0f, 5.0f, 5.0f, 5.0f ).SetWidth( 100.0f, MetricsUnits.Percentage ) );
            m_loadXmlButton = ( Button )AddChild( new Button( "Load Layout" ).SetMargin( 5.0f, 5.0f, 5.0f, 5.0f ).SetWidth( 100.0f, MetricsUnits.Percentage ) );
            m_saveXmlButton = ( Button )AddChild( new Button( "Save Layout" ).SetMargin( 5.0f, 5.0f, 5.0f, 5.0f ).SetWidth( 100.0f, MetricsUnits.Percentage ) );
            m_newBehaviorButton = ( Button )AddChild( new Button( "New Behavior" ).SetMargin( 5.0f, 5.0f, 5.0f, 5.0f ).SetWidth( 100.0f, MetricsUnits.Percentage ) );

            m_generateButton.Enabled = false;
            m_treeIdField.ValueChange += m_treeIdField_ValueChange;
            m_savePathField.ValueChange += m_savePathField_ValueChange;
        }

        void m_savePathField_ValueChange( IEditable sender, EditEventArgs args, Event nativeEvent )
        {
            EditorPrefs.SetString( "BTE_SavePath", ( string )args.newValue );            
        }

        void m_treeIdField_ValueChange( IEditable sender, EditEventArgs args, Event nativeEvent )
        {
            string s = args.newValue as string;

            if ( string.IsNullOrEmpty( s ) )
            {
                m_generateButton.Enabled = false;
            }
            else
            {
                m_generateButton.Enabled = true;
            }
        }        

        protected override void OnBeforeDraw()
        {
            GUI.Box( ScreenRect, "" );
        }
    }
}