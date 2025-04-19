using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireActivator))]
    public class RayfireActivatorEditor : Editor
    {
        RayfireActivator activator;
        
        // Handles
        BoxBoundsHandle    m_BoxHandle;
        SphereBoundsHandle m_SphHandle;
        
        // Foldout
        static bool expand;
        
        // Minimum & Maximum ranges
        const float radius_min         = 0.01f;
        const float radius_max         = 100f;
        const float delay_min          = 0f;
        const float delay_max          = 30f;
        const int   var_min            = 0;
        const int   var_max            = 100;
        const float duration_min       = 0.1f;
        const float duration_max       = 100f;
        const float scaleAnimation_min = 1f;
        const float scaleAnimation_max = 50f;
        
        // Serialized properties
        SerializedProperty sp_checkRigid;
        SerializedProperty sp_checkRigidRoot;
        SerializedProperty sp_showGizmo;
        SerializedProperty sp_gizmoType;
        SerializedProperty sp_sphereRadius;
        SerializedProperty sp_boxSize;
        SerializedProperty sp_type;
        SerializedProperty sp_speed;
        SerializedProperty sp_delay;
        SerializedProperty sp_demolishCluster;
        SerializedProperty sp_apply;
        SerializedProperty sp_velocity;
        SerializedProperty sp_velVar;
        SerializedProperty sp_spin;
        SerializedProperty sp_mode;
        SerializedProperty sp_coord;
        SerializedProperty sp_showAnimation;
        SerializedProperty sp_duration;
        SerializedProperty sp_scaleAnimation;
        SerializedProperty sp_positionAnimation;
        SerializedProperty sp_line;
        SerializedProperty sp_tag;
        SerializedProperty sp_mask;
        
        void OnEnable()
        {
            // Get component
            activator = (RayfireActivator)target;
            
            // Set tag list
            RFUI.SetTags();
                        
            // Collect layers
            RFUI.SetLayers();
            
            // Box handle
            m_BoxHandle = new BoxBoundsHandle
            {
                wireframeColor = RFUI.color_blue,
                handleColor    = RFUI.color_orange,
            };

            // Sphere handle
            m_SphHandle = new SphereBoundsHandle
            {
                wireframeColor = RFUI.color_blue,
                handleColor    = RFUI.color_orange,
            };
            
            // Find properties
            sp_checkRigid        = serializedObject.FindProperty(nameof(activator.checkRigid));
            sp_checkRigidRoot    = serializedObject.FindProperty(nameof(activator.checkRigidRoot));
            sp_showGizmo         = serializedObject.FindProperty(nameof(activator.showGizmo));
            sp_gizmoType         = serializedObject.FindProperty(nameof(activator.gizmoType));
            sp_sphereRadius      = serializedObject.FindProperty(nameof(activator.sphereRadius));
            sp_boxSize           = serializedObject.FindProperty(nameof(activator.boxSize));
            sp_type              = serializedObject.FindProperty(nameof(activator.type));
            sp_speed             = serializedObject.FindProperty(nameof(activator.spdMin));
            sp_delay             = serializedObject.FindProperty(nameof(activator.delay));
            sp_demolishCluster   = serializedObject.FindProperty(nameof(activator.demolishCluster));
            sp_apply             = serializedObject.FindProperty(nameof(activator.apply));
            sp_velocity          = serializedObject.FindProperty(nameof(activator.velocity));
            sp_velVar            = serializedObject.FindProperty(nameof(activator.velVar));
            sp_spin              = serializedObject.FindProperty(nameof(activator.spin));
            sp_mode              = serializedObject.FindProperty(nameof(activator.mode));
            sp_coord             = serializedObject.FindProperty(nameof(activator.coord));
            sp_showAnimation     = serializedObject.FindProperty(nameof(activator.showAnimation));
            sp_duration          = serializedObject.FindProperty(nameof(activator.duration));
            sp_scaleAnimation    = serializedObject.FindProperty(nameof(activator.scaleAnimation));
            sp_positionAnimation = serializedObject.FindProperty(nameof(activator.positionAnimation));
            sp_line              = serializedObject.FindProperty(nameof(activator.line));
            sp_tag               = serializedObject.FindProperty(nameof(activator.tagFilter));
            sp_mask              = serializedObject.FindProperty(nameof(activator.mask));
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            // GUI
            GUI_Components();
            GUI_Gizmo();
            GUI_Activation();
            GUI_Force();
            GUI_Filters();
            GUI_Animation();

            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Components
        /// /////////////////////////////////////////////////////////
        
        void GUI_Components()
        {
            RFUI.CaptionBox (TextAct.gui_cap_components);
            RFUI.PropertyField (sp_checkRigid,     TextAct.gui_checkRigid);
            RFUI.PropertyField (sp_checkRigidRoot, TextAct.gui_checkRigidRoot);
            if (sp_checkRigid.boolValue == false && sp_checkRigidRoot.boolValue == false)
                RFUI.HelpBox (TextAct.hlp_select, MessageType.Warning, true);
        }

        void GUI_Gizmo()
        {
            RFUI.CaptionBox (TextAct.gui_cap_gizmo);
            RFUI.PropertyField (sp_gizmoType, TextAct.gui_gizmoType);
            
            // Sphere and box gizmo types ops
            if (activator.gizmoType == RayfireActivator.GizmoType.Box || activator.gizmoType == RayfireActivator.GizmoType.Sphere)
            {
                if (activator.gizmoType == RayfireActivator.GizmoType.Sphere)
                    RFUI.Slider (sp_sphereRadius, radius_min, radius_max, TextAct.gui_sphereRadius);
                if (activator.gizmoType == RayfireActivator.GizmoType.Box)
                    RFUI.PropertyField (sp_boxSize, TextAct.gui_boxSize);
                
                // Runtime size change
                if (Application.isPlaying == true && activator.activatorCollider != null)
                {
                    if (activator.gizmoType == RayfireActivator.GizmoType.Sphere)
                    {
                        if (activator.activatorCollider is SphereCollider == true)
                            ((SphereCollider)activator.activatorCollider).radius = activator.sphereRadius;
                    }
                    else if (activator.gizmoType == RayfireActivator.GizmoType.Box)
                    {
                        if (activator.activatorCollider is BoxCollider == true) 
                            ((BoxCollider)activator.activatorCollider).size = activator.boxSize;
                    }                
                }
            }

            RFUI.PropertyField (sp_showGizmo, TextAct.gui_showGizmo);
        }
        
        void GUI_Activation()
        {
            RFUI.CaptionBox (TextAct.gui_cap_act);
            
            // Enter Exit not supported by particle system
            if (activator.gizmoType != RayfireActivator.GizmoType.ParticleSystem)
                RFUI.PropertyField (sp_type, TextAct.gui_type);
            
            EditorGUI.BeginChangeCheck();
            RFUI.PropertyField (sp_speed, TextAct.gui_speed);
            if (EditorGUI.EndChangeCheck())
                foreach (var targ in targets)
                    if (targ as RayfireActivator != null)
                        if (sp_speed.floatValue > 0)
                            (targ as RayfireActivator).StartSpeedCor();
 
            if (Application.isPlaying && sp_speed.floatValue > 0)
                RFUI.Label ("Current Speed:   " + activator.spdNow.ToString("F1"));
            
            RFUI.Slider (sp_delay, delay_min, delay_max, TextAct.gui_delay);
            RFUI.PropertyField (sp_demolishCluster, TextAct.gui_demolishCluster);
        }

        void GUI_Filters()
        {
            RFUI.CaptionBox (TextGun.gui_cap_filt);
            RFUI.TagField (sp_tag, TextGun.gui_tag);
            RFUI.MaskField (sp_mask, TextGun.gui_mask);
        }

        void GUI_Force()
        {
            RFUI.CaptionBox (TextAct.gui_cap_frc);
            RFUI.PropertyField (sp_apply, TextAct.gui_apply);
            
            if (sp_apply.boolValue == true)
            {
                RFUI.PropertyField (sp_coord,    TextAct.gui_coord);
                RFUI.PropertyField (sp_velocity, TextAct.gui_velocity);
                RFUI.IntSlider (sp_velVar, var_min, var_max, TextAct.gui_velVar);
                RFUI.PropertyField (sp_spin,     TextAct.gui_spin);
                RFUI.PropertyField (sp_mode,     TextAct.gui_mode);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Animation
        /// /////////////////////////////////////////////////////////
        
        void GUI_Animation()
        {
            RFUI.CaptionBox (TextAct.gui_cap_anim);
            RFUI.PropertyField (sp_showAnimation, TextAct.gui_showAnimation);
            
            if (sp_showAnimation.boolValue == true)
            {
                RFUI.Slider (sp_duration,       duration_min,       duration_max,       TextAct.gui_duration);
                RFUI.Slider (sp_scaleAnimation, scaleAnimation_min, scaleAnimation_max, TextAct.gui_scaleAnimation);
                RFUI.PropertyField (sp_positionAnimation, TextAct.gui_positionAnimation);

                if (activator.ByLine == true)
                    RFUI.PropertyField (sp_line, TextAct.gui_line);

                if (activator.ByPositions == true)
                {
                    expand = EditorGUILayout.Foldout (expand, TextAct.gui_positionList, true);
                    if (expand == true && activator.positionList != null && activator.positionList.Count > 0)
                    {
                        for (int i = 0; i < activator.positionList.Count; i++)
                        {
                            activator.positionList[i] = EditorGUILayout.Vector3Field ("  " + i, activator.positionList[i]);
                            RFUI.Space();
                        }
                    }
                }
                
                if (Application.isPlaying == false)
                {
                    if (activator.positionAnimation == RayfireActivator.AnimationType.ByGlobalPositionList ||
                        activator.positionAnimation == RayfireActivator.AnimationType.ByLocalPositionList)
                    {
                        RFUI.Space ();
                        EditorGUI.BeginChangeCheck();
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button (RFUI.str_add, GUILayout.Height (25)))
                        {
                            activator.AddPosition (activator.transform.position);
                            expand = true;
                        }
                        if (GUILayout.Button (RFUI.str_remove, GUILayout.Height (25)))
                        {
                            if (activator.positionList != null && activator.positionList.Count > 0)
                                activator.positionList.RemoveAt (activator.positionList.Count - 1);
                        }
                        if (GUILayout.Button (RFUI.str_clear, GUILayout.Height (25)))
                        {
                            if (activator.positionList != null)
                                activator.positionList.Clear();
                            expand = false;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (EditorGUI.EndChangeCheck() == true)
                            RFUI.SetDirty (activator.gameObject);
                    }
                }
                if (Application.isPlaying == true)
                {
                    GUILayout.Label (TextAct.gui_cap_play, EditorStyles.boldLabel);
                
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button (TextAct.gui_start, GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireActivator != null)
                                (targ as RayfireActivator).StartAnimation();
                    if (GUILayout.Button (TextAct.gui_stop, GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireActivator != null)
                                (targ as RayfireActivator).StopAnimation();
                    if (GUILayout.Button (TextAct.gui_reset, GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireActivator != null)
                                (targ as RayfireActivator).ResetAnimation();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireActivator targ, GizmoType gizmoType)
        {
            if (targ.enabled && targ.showGizmo == true)
                DrawGizmo (targ);
        }
        
        void OnSceneGUI()
        {
            activator = target as RayfireActivator;
            if (activator == null)
                return;

            if (activator.enabled == true && activator.showGizmo == true)
            {
                if (activator.gizmoType == RayfireActivator.GizmoType.Sphere)
                {
                    Handles.matrix             = activator.transform.localToWorldMatrix;
                    m_SphHandle.wireframeColor = RFUI.color_blue;
                    m_SphHandle.center         = Vector3.zero;
                    m_SphHandle.radius         = activator.sphereRadius;
                    
                    EditorGUI.BeginChangeCheck();
                    m_SphHandle.DrawHandle();
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        RFUI.SetDirty (activator.gameObject);
                        Undo.RecordObject (activator, TextAct.str_rad);
                        activator.sphereRadius = m_SphHandle.radius;
                    }
                }

                if (activator.gizmoType == RayfireActivator.GizmoType.Box)
                {
                    Handles.matrix             = activator.transform.localToWorldMatrix;
                    m_BoxHandle.wireframeColor = RFUI.color_blue;
                    m_BoxHandle.center         = Vector3.zero;
                    m_BoxHandle.size           = activator.boxSize;
                    
                    EditorGUI.BeginChangeCheck();
                    m_BoxHandle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        RFUI.SetDirty (activator.gameObject);
                        Undo.RecordObject (activator, TextAct.str_box);
                        activator.boxSize = m_BoxHandle.size;
                    }
                }
            }
        }
        
        static void DrawGizmo (RayfireActivator targ)
        {
            // Gizmo properties
            Gizmos.color  = RFUI.color_blue;
            Gizmos.matrix = targ.transform.localToWorldMatrix;

            // Box gizmo
            if (targ.gizmoType == RayfireActivator.GizmoType.Box)
                Gizmos.DrawWireCube (Vector3.zero, targ.boxSize);

            // Sphere gizmo
            if (targ.gizmoType == RayfireActivator.GizmoType.Sphere)
            {
                // Vars
                float rate   = 0f;
                int   size   = 45;
                float scale  = 1f / size;
                float radius = targ.sphereRadius;

                Vector3 previousPoint = Vector3.zero;
                Vector3 nextPoint     = Vector3.zero;

                // Draw top eye
                nextPoint.y     = 0f;
                previousPoint.y = 0f;
                previousPoint.x = radius * Mathf.Cos (rate);
                previousPoint.z = radius * Mathf.Sin (rate);
                for (int i = 0; i < size; i++)
                {
                    rate        += 2.0f * Mathf.PI * scale;
                    nextPoint.x =  radius * Mathf.Cos (rate);
                    nextPoint.z =  radius * Mathf.Sin (rate);
                    Gizmos.DrawLine (previousPoint, nextPoint);
                    previousPoint = nextPoint;
                }

                // Draw top eye
                rate            = 0f;
                nextPoint.x     = 0f;
                previousPoint.x = 0f;
                previousPoint.y = radius * Mathf.Cos (rate);
                previousPoint.z = radius * Mathf.Sin (rate);
                for (int i = 0; i < size; i++)
                {
                    rate        += 2.0f * Mathf.PI * scale;
                    nextPoint.y =  radius * Mathf.Cos (rate);
                    nextPoint.z =  radius * Mathf.Sin (rate);
                    Gizmos.DrawLine (previousPoint, nextPoint);
                    previousPoint = nextPoint;
                }

                // Draw top eye
                rate            = 0f;
                nextPoint.z     = 0f;
                previousPoint.z = 0f;
                previousPoint.y = radius * Mathf.Cos (rate);
                previousPoint.x = radius * Mathf.Sin (rate);
                for (int i = 0; i < size; i++)
                {
                    rate        += 2.0f * Mathf.PI * scale;
                    nextPoint.y =  radius * Mathf.Cos (rate);
                    nextPoint.x =  radius * Mathf.Sin (rate);
                    Gizmos.DrawLine (previousPoint, nextPoint);
                    previousPoint = nextPoint;
                }

                // Selectable sphere
                float sphereSize = radius * 0.07f;
                if (sphereSize < 0.1f)
                    sphereSize = 0.1f;
                Gizmos.color = RFUI.color_orange;
                Gizmos.DrawSphere (new Vector3 (0f,      radius,  0f),      sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f,      -radius, 0f),      sphereSize);
                Gizmos.DrawSphere (new Vector3 (radius,  0f,      0f),      sphereSize);
                Gizmos.DrawSphere (new Vector3 (-radius, 0f,      0f),      sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f,      0f,      radius),  sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f,      0f,      -radius), sphereSize);
            }
        }
    }
}