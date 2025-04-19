using UnityEditor;
using RayFire;
using UnityEngine;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireShell))]
    public class RayfireShellEditor : Editor
    {
        RayfireShell shell;
        
        // Serialized properties
        SerializedProperty sp_awakeCrt;
        SerializedProperty sp_bridge;
        SerializedProperty sp_thickness;
        SerializedProperty sp_material;
        SerializedProperty sp_mergeSub;
        SerializedProperty sp_awakeBake;
        
        string str = "WIP. Shell mesh generation supported only in Editor mode on Windows platform for now. " +
                     "Support for other platforms will be added later. " +
                     "Bake Shell mesh in order to use shell mesh in your builds.";
        
        private void OnEnable()
        {
            // Get component
            shell = (RayfireShell)target;
            
            // Find properties
            sp_awakeCrt  = serializedObject.FindProperty(nameof(shell.awakeCreate));
            sp_bridge    = serializedObject.FindProperty(nameof(shell.bridge));
            sp_thickness = serializedObject.FindProperty(nameof(shell.thickness));
            sp_material  = serializedObject.FindProperty(nameof(shell.material));
            sp_mergeSub  = serializedObject.FindProperty(nameof(shell.subMerge));
            sp_awakeBake = serializedObject.FindProperty (nameof(shell.awakeBake));
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            // Set style
            RFUI.SetToggleButtonStyle();
            
            GUI_Create();
            GUI_Props();
            GUI_Bake();
            GUI_Export();
            GUI_Info();
            
            // Support warning
            RFUI.HelpBox (str, MessageType.Info, true);
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void GUI_Create()
        {
            RFUI.Space();

            // Create shell
            if (shell.created == true)
            {
                if (GUILayout.Button ("Destroy Shell", RFUI.buttonStyle, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireShell != null)
                            (targ as RayfireShell).PreviewOff();
            }

            // Destroy shell
            if (shell.created == false)
            {
                if (GUILayout.Button ("Create Shell", RFUI.buttonStyle, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireShell != null)
                            (targ as RayfireShell).PreviewOn();
            }
        }
        
        void GUI_Info()
        {
            if (shell.shellObj != null)
            {
                RFUI.CaptionBox (TextShl.gui_cap_info);
                GUILayout.Label ("Shell Object:   " + shell.shellObj.name);
            }
        }

        void GUI_Props()
        {
            RFUI.CaptionBox (TextShl.gui_cap_prp);
            RFUI.PropertyField (sp_awakeCrt, TextShl.gui_awakeCreate);
            RFUI.PropertyField (sp_bridge, TextShl.gui_edges);
            
            EditorGUI.BeginChangeCheck();
            RFUI.Slider (sp_thickness, 0.001f, 1, TextShl.gui_thickness);
            if (EditorGUI.EndChangeCheck())
                shell.EditShell();
            
            RFUI.PropertyField (sp_material, TextShl.gui_material);
        }

        void GUI_Bake()
        {
            if (Application.isPlaying == true)
                return;
            
            RFUI.CaptionBox (TextShl.gui_cap_bake);
            RFUI.PropertyField (sp_mergeSub,  TextShl.gui_mergeSub);
            RFUI.PropertyField (sp_awakeBake, TextShl.gui_awakeBake);

            RFUI.Space();

            if (shell.created == true || shell.backed == true)
            {
                if (shell.shellObj != null)
                {
                    if (GUILayout.Button ("Bake Shell to MeshFilter", RFUI.buttonStyle, GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireShell != null)
                                (targ as RayfireShell).BakeShell();

                    RFUI.Space();
                    RFUI.Space();
                    RFUI.HelpBox (TextShl.hlp_bake, MessageType.Info, true);
                }
            }
        }
        
        void GUI_Export()
        {
            if (Application.isPlaying == true)
                return;
            
            if (shell.created == true || shell.backed == true)
            {
                RFUI.CaptionBox (TextShl.gui_cap_exp);
                
                if (GUILayout.Button ("Export Mesh", RFUI.buttonStyle, GUILayout.Height (25)))
                {
                    MeshFilter mf = shell.GetComponent<MeshFilter>();
                    RFMeshAsset.SaveMesh (mf, shell.name);
                }
                RFUI.Space();
            }
        }
    }
}