using UnityEngine;
using UnityEditor;
using System.Collections;

using UForms;
using UForms.Application;
using UForms.Controls;
using UForms.Events;
using UForms.Decorators;
using UForms.Graphics;

using BehaviorTrees;
using System.IO;

namespace BehaviorTreesEditor
{
    public class BehaviorTreeEditor : UFormsApplication
    {
        private const string TEMP_FILENAME = "TEMP_BTELayout.xml";
        private const string TEMP_FILE_KEY = "BTE_TempFileExists";

        [MenuItem( "282Productions/Behavior Editor" )]
        private static void Run()
        {
            EditorWindow window = EditorWindow.GetWindow<BehaviorTreeEditor>();
            window.title = "Behavior Editor";
        }

        BehaviorTreeControl                 m_editor;
        BehaviorEditorSettingsControl       m_tools;

        protected override void OnInitialize()
        {
            wantsMouseMove = true;

            m_editor = ( BehaviorTreeControl )AddChild( new BehaviorTreeControl() );
            m_editor.SetSize( 100.0f, 100.0f, Control.MetricsUnits.Percentage, Control.MetricsUnits.Percentage );

            m_tools = ( BehaviorEditorSettingsControl )AddChild( new BehaviorEditorSettingsControl() );
            m_tools.SetMargin( 10.0f, 10.0f, 10.0f, 10.0f );
            m_tools.SetHeight( 100.0f, Control.MetricsUnits.Percentage );

            m_tools.GenerateButton.Clicked  += GenerateButton_Clicked;
            m_tools.NewButton.Clicked       += NewButton_Clicked;
            m_tools.SaveButton.Clicked      += SaveButton_Clicked;
            m_tools.LoadButton.Clicked      += LoadButton_Clicked;

            if ( EditorPrefs.GetBool( TEMP_FILE_KEY, false ) )
            {
                LoadXML( Application.persistentDataPath + "/" + TEMP_FILENAME );
                EditorPrefs.SetBool( TEMP_FILE_KEY, false );
            }
        }

        void LoadButton_Clicked( IClickable sender, ClickEventArgs args, Event nativeEvent )
        {
            string path = EditorUtility.OpenFilePanel( "Load Behavior Layout", Application.dataPath, "xml" );

            if ( !string.IsNullOrEmpty( path ) )
            {
                LoadXML( path );               
            }
        }

        void SaveButton_Clicked( IClickable sender, ClickEventArgs args, Event nativeEvent )
        {
            BehaviorNodeControl root = m_editor.GetRootControl();

            if ( root != null )
            {
                string path = EditorUtility.SaveFilePanelInProject( "Save Behavior Layout", string.Format("Layout{0}",m_tools.TreeIdField.Value),"xml","");

                if ( !string.IsNullOrEmpty( path ) )
                {
                    SaveXML( path, root );
                }
            }       
        }


        private void LoadXML( string path )
        {
            BehaviorTreeXMLSerializer serializer = new BehaviorTreeXMLSerializer();

            string id;
            BehaviorNodeControl[] controls;

            if ( serializer.Deserialize( path, m_editor.TypeCahce, out id, out controls ) )
            {
                m_tools.TreeIdField.Value = id;

                // Quick hack to fix issue where manually setting field value does not raise value changed event
                if ( !string.IsNullOrEmpty( id ) )
                {
                    m_tools.GenerateButton.Enabled = true;
                }
                else
                {
                    m_tools.GenerateButton.Enabled = false;
                }

                m_editor.ClearAll();
                m_editor.AddNodeGroup( controls );
            }
        }


        private void SaveXML( string path, BehaviorNodeControl root )
        {
            BehaviorTreeXMLSerializer serializer = new BehaviorTreeXMLSerializer();
            string xml = serializer.Serialize( m_tools.TreeIdField.Value, root );
            File.WriteAllText( path, xml );
            AssetDatabase.Refresh();
        }


        void NewButton_Clicked( IClickable sender, ClickEventArgs args, Event nativeEvent )
        {
            bool confirm = EditorUtility.DisplayDialog( "Confirm New Behavior", "Are you sure you would like to create a new behavior? Current layout will be discarded!", "Ok", "Cancel" );

            if (confirm )
            {
                m_editor.ClearAll();
            }
        }

        void GenerateButton_Clicked( IClickable sender, ClickEventArgs args, Event nativeEvent )
        {
            BehaviorNodeControl root = m_editor.GetRootControl();

            if ( root != null )
            {
                SaveXML( Application.persistentDataPath + "/" + TEMP_FILENAME, root );
                EditorPrefs.SetBool( TEMP_FILE_KEY, true );

                string safeHandle = m_tools.TreeIdField.Value.Replace( " ", "" );

                BehaviorTreeGenerator generator = new BehaviorTreeGenerator();
                string output = generator.Generate( m_tools.TreeIdField.Value, safeHandle, root );

                SaveSourceFile( safeHandle, output );
            }            
        }


        private void SaveSourceFile( string name, string content )
        {
            string dir   = Application.dataPath + "/" + m_tools.SavePathField.Value;
            string fpath = Application.dataPath + "/" + m_tools.SavePathField.Value + "/" + "Behavior" + name + ".cs";

            // Create output directory
            if ( !Directory.Exists( dir ) )
            {
                Directory.CreateDirectory( dir );                
            }

            if ( File.Exists( fpath ) )
            {
                File.Delete( fpath );
            }

            File.WriteAllText( fpath , content );

            AssetDatabase.Refresh();
        }
    }
}