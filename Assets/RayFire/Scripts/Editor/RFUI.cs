using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace RayFireEditor
{
    /// <summary>
    /// Rayfire Editor UI class.
    /// </summary>
    public static class RFUI
    {

        static          string[] tags;
        static          string[] layerNames = new string[32];
        static readonly int      space      = 3;
        
        public const    string   str_add    = "Add";
        public const    string   str_remove = "Remove";
        public const    string   str_clear  = "Clear";
        public const    string   str_start  = "Start";
        public const    string   str_stop   = "Stop";
        public const    string   str_reset  = "Reset";

        // Colors
        public static readonly Color color_blue     = new Color (0.58f, 0.77f, 1f);
        public static readonly Color color_orange   = new Color (1.0f,  0.60f, 0f);
        public static readonly Color color_btn_blue = new Color (0.6f,  0.7f,  0.9f);

        // Styles
        static        Texture2D btn_texture;
        public static GUIStyle  buttonStyle;
        public static GUIStyle  buttonStylePressed;
        public static GUIStyle SetToggleButtonStyle ()
        {
            //SetButtonTexture (Color.grey);
            if (buttonStyle == null)
            {
                buttonStyle                      = new GUIStyle (GUI.skin.button);
                //buttonStyle.normal.background    = btn_texture;;
                //buttonStyle.onNormal.background  = btn_texture;;
                //buttonStyle.onActive.background  = btn_texture;
                //buttonStyle.active.background    = btn_texture;
                //buttonStyle.onFocused.background = btn_texture;
                //buttonStyle.focused.background   = btn_texture;
            }
            
            return buttonStyle;
        }

        static void SetButtonTexture (Color col)
        {
            if (btn_texture == null)
            {
                Color[] pix = new Color[2 * 2];
                for (int i = 0; i < pix.Length; ++i)
                {
                    pix[i] = col;
                }
                btn_texture = new Texture2D (2, 2);
                btn_texture.SetPixels (pix);
                btn_texture.Apply();
            }
        }
        
        // Space between properties
        public static void Space()
        {
            GUILayout.Space (space);
        }
        
        // Properties caption
        public static void Caption(GUIContent caption)
        {
            GUILayout.Space (space);
            GUILayout.Space (space);
            GUILayout.Label (caption, EditorStyles.boldLabel);
            GUILayout.Space (space);
        }
        
        // Properties caption
        public static void CaptionBox(GUIContent caption)
        {
            GUILayout.Space (space);
            GUILayout.Space (space);
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label (caption, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space (space);
        }
        
        public static void BeginWindow()
        {
            EditorGUILayout.BeginVertical("window");
        }
        
        public static void EndWindow()
        {
            EditorGUILayout.EndVertical();
        }

        public static void PropertyField(SerializedProperty sp, GUIContent content)
        {
            EditorGUILayout.PropertyField (sp, content);
            Space();
        }

        public static void Slider(SerializedProperty sp, float min, float max, GUIContent content)
        {
            EditorGUILayout.Slider (sp, min, max, content);
            Space();
        }

        public static void IntSlider(SerializedProperty sp, int min, int max, GUIContent content)
        {
            EditorGUILayout.IntSlider (sp, min, max, content);
            Space();
        }

        public static void Foldout(ref bool val, string pref, string caption)
        {
            EditorGUI.BeginChangeCheck();
            val = EditorGUILayout.Foldout (val, caption, true);
            if (EditorGUI.EndChangeCheck() == true)
                EditorPrefs.SetBool (pref, val);
            Space();
        }

        public static void Foldout(ref bool val, string caption)
        {
            val = EditorGUILayout.Foldout (val, caption, true);
            Space();
        }
        
        public static void FoldoutHeader(ref bool val, string caption)
        {
            val = EditorGUILayout.Foldout (val, caption, true, EditorStyles.foldoutHeader);
            Space();
        }

        public static void HelpBox(string str, MessageType type, bool wide)
        {
            EditorGUILayout.HelpBox (str, type, wide);
            Space();
        }

        public static void MaskField(SerializedProperty sp, GUIContent content)
        {
            EditorGUI.BeginChangeCheck();
            int mask = EditorGUILayout.MaskField (content, sp.intValue, layerNames);
            if (EditorGUI.EndChangeCheck())
                sp.intValue = mask;
            Space();
        }
        
        public static void LayerField(SerializedProperty sp, GUIContent content)
        {
            EditorGUI.BeginChangeCheck();
            int layer = EditorGUILayout.LayerField (content, sp.intValue);
            if (EditorGUI.EndChangeCheck())
                sp.intValue = layer;
            Space();
        }

        public static void SetTags()
        {
            tags = InternalEditorUtility.tags;
        }
        
        public static void SetLayers()
        {
            for (int i = 0; i <= 31; i++)
                layerNames[i] = (i + ": " + LayerMask.LayerToName (i));
        }

        public static void TagField(SerializedProperty sp, GUIContent content)
        {
            int tagIndex = System.Array.IndexOf(tags, sp.stringValue);
            if (tagIndex == -1)
            {
                sp.stringValue = "Untagged";
                tagIndex       = 0;
            }

            int newIndex = EditorGUILayout.Popup(content, tagIndex, tags);
            if (newIndex != tagIndex)
                sp.stringValue = tags[newIndex];
            Space();
        }
        
        public static void Toggle (SerializedProperty sp, GUIContent content, int height = 22)
        {
            bool newState = GUILayout.Toggle (sp.boolValue, content, "Button", GUILayout.Height (height));
            sp.boolValue = newState;
            Space();
        }

        public static bool Toggle (SerializedProperty sp, GUIContent content, Color color, int height = 22)
        {
            Color col = GUI.backgroundColor;
            
            if (sp.boolValue == true)
                GUI.backgroundColor = color;
            
            bool newState = GUILayout.Toggle (sp.boolValue, content, buttonStyle, GUILayout.Height (height));
            sp.boolValue = newState;
            
            if (sp.boolValue == true)
                GUI.backgroundColor = col;
            
            Space();

            return newState;
        }
        
        public static void Label (string str)
        {
            GUILayout.Label (str);
            Space();
        }
        
        public static void SetDirty (GameObject go)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (go);
                EditorSceneManager.MarkSceneDirty (go.scene);
                SceneView.RepaintAll();
            }
        }
    }
}



                   
/*
if (prop.hasMultipleDifferentValues == true)
    ProgressBar (prop.intValue / 100.0f, "Armor");

prop.hasMultipleDifferentValues;
prop.boolValue;

// Custom GUILayout progress bar.
void ProgressBar (float value, string label)
{
    // Get a rect for the progress bar using the same margins as a textfield:
    Rect rect = GUILayoutUtility.GetRect (18, 18, "TextField");
    EditorGUI.ProgressBar (rect, value, label);
    EditorGUILayout.Space ();
}
*/





/*

EditorGUILayout.PrefixLabel ("MinMax");
EditorGUILayout.FloatField (wind.minimum, GUILayout.Width (50));

EditorGUI.BeginChangeCheck();
EditorGUILayout.MinMaxSlider (ref wind.minimum, ref wind.maximum, -5f, 5, GUILayout.Width (EditorGUIUtility.currentViewWidth - 400f));
if (EditorGUI.EndChangeCheck() == true)
{
    foreach (RayfireWind scr in targets)
    {
        scr.minimum = wind.minimum;
        scr.maximum = wind.maximum;
        SetDirty (scr);
    }
}

EditorGUILayout.FloatField (wind.maximum, GUILayout.Width (50));
GUILayout.EndHorizontal ();

*/