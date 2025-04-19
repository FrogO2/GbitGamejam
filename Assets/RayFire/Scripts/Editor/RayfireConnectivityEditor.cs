using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireConnectivity))]
    public class RayfireConnectivityEditor : Editor
    {
        RayfireConnectivity   conn;
        static readonly Color stressColor = Color.green;

        // Foldout
        static bool fld_filters;
        static bool fld_props_clp;
        static bool fld_props_str;
        static bool fld_joints;
        
        // Minimum & Maximum ranges
        const float expand_min      = 0;
        const float expand_max      = 1f;
        const float minimumArea_min = 0;
        const float minimumArea_max = 1f;
        const float minimumSize_min = 0;
        const float minimumSize_max = 10f;
        const int   percentage_min  = 0;
        const int   percentage_max  = 100;
        const int   seed_min        = 0;
        const int   seed_max        = 100;
        const int   sub_thr_min     = 10;
        const int   sub_thr_max     = 999;
        const int   sub_cls_min     = 2;
        const int   sub_cls_max     = 99;
        const int   sub_deb_min     = 0;
        const int   sub_deb_max     = 60;
        const int   integrity_min   = 1;
        const int   integrity_max   = 99;
        const int   damage_min   = 0;
        const int   damage_max   = 10000;
        const int   start_min       = 0;
        const int   start_max       = 99;
        const int   end_min         = 1;
        const int   end_max         = 100;
        const int   steps_min       = 1;
        const int   steps_max       = 100;
        const float duration_min    = 0;
        const float duration_max    = 60f;
        const int   var_min         = 0;
        const int   var_max         = 100;
        const int   seed_clp_min    = 0;
        const int   seed_clp_max    = 99;
        const int   threshold_min   = 1;
        const int   threshold_max   = 1000;
        const float erosion_min     = 0;
        const float erosion_max     = 10f;
        const float interval_min    = 0.1f;
        const float interval_max    = 10f;
        const int   support_min     = 0;
        const int   support_max     = 90;
        const int   debris_min      = 0;
        const int   debris_max      = 50;

        // Connectivity Serialized properties
        SerializedProperty sp_show_giz;
        SerializedProperty sp_show_con;
        SerializedProperty sp_show_nod;
        SerializedProperty sp_type;
        SerializedProperty sp_expand;
        SerializedProperty sp_minimumArea;
        SerializedProperty sp_minimumSize;
        SerializedProperty sp_percentage;
        SerializedProperty sp_seed;
        SerializedProperty sp_clusterize;
        SerializedProperty sp_sub_enable;
        SerializedProperty sp_sub_thr;
        SerializedProperty sp_sub_min;
        SerializedProperty sp_sub_max;
        SerializedProperty sp_sub_deb;
        SerializedProperty sp_demolishable;
        SerializedProperty sp_triggerCollider;
        SerializedProperty sp_triggerDebris;
        
        // Collapse Serialized properties
        SerializedProperty sp_startCollapse;
        SerializedProperty sp_clp_integrity;
        SerializedProperty sp_clp_dmg_max;
        SerializedProperty sp_clp_dmg_cur;
        SerializedProperty sp_clp_type;
        SerializedProperty sp_start;
        SerializedProperty sp_end;
        SerializedProperty sp_steps;
        SerializedProperty sp_duration;
        SerializedProperty sp_var;
        SerializedProperty sp_clp_seed;

        // Stress Serialized properties
        SerializedProperty sp_showStress;
        SerializedProperty sp_startStress;
        SerializedProperty sp_str_integrity;
        SerializedProperty sp_enable;
        SerializedProperty sp_threshold;
        SerializedProperty sp_erosion;
        SerializedProperty sp_interval;
        SerializedProperty sp_support;
        SerializedProperty sp_exposed;
        SerializedProperty sp_bySize;
        
        void OnEnable()
        {
            // Get component
            conn = (RayfireConnectivity)target;
            
            // Find Connectivity properties
            sp_show_giz        = serializedObject.FindProperty(nameof(conn.showGizmo));
            sp_show_con        = serializedObject.FindProperty(nameof(conn.showConnections));
            sp_show_nod        = serializedObject.FindProperty(nameof(conn.showNodes));
            sp_type            = serializedObject.FindProperty(nameof(conn.type));
            sp_expand          = serializedObject.FindProperty(nameof(conn.expand));
            sp_minimumArea     = serializedObject.FindProperty(nameof(conn.minimumArea));
            sp_minimumSize     = serializedObject.FindProperty(nameof(conn.minimumSize));
            sp_percentage      = serializedObject.FindProperty(nameof(conn.percentage));
            sp_seed            = serializedObject.FindProperty(nameof(conn.seed));
            sp_clusterize      = serializedObject.FindProperty(nameof(conn.clusterize));
            sp_sub_enable      = serializedObject.FindProperty(nameof(conn.subEnable));
            sp_sub_thr         = serializedObject.FindProperty(nameof(conn.subThr));
            sp_sub_min         = serializedObject.FindProperty(nameof(conn.subMin));
            sp_sub_max         = serializedObject.FindProperty(nameof(conn.subMax));
            sp_sub_deb         = serializedObject.FindProperty(nameof(conn.subDeb)); 
            sp_demolishable    = serializedObject.FindProperty(nameof(conn.demolishable));
            sp_triggerCollider = serializedObject.FindProperty(nameof(conn.triggerCollider));
            sp_triggerDebris   = serializedObject.FindProperty(nameof(conn.triggerDebris));
        
            // Find Collapse properties
            sp_startCollapse = serializedObject.FindProperty(nameof(conn.startCollapse));
            sp_clp_integrity = serializedObject.FindProperty(nameof(conn.collapseByIntegrity));
            sp_clp_dmg_max   = serializedObject.FindProperty(nameof(conn.collapseByDamageMax));
            sp_clp_dmg_cur   = serializedObject.FindProperty(nameof(conn.collapseByDamageCur));
            sp_clp_type      = serializedObject.FindProperty(nameof(conn.collapse) + "." + nameof(conn.collapse.type));
            sp_start         = serializedObject.FindProperty(nameof(conn.collapse) + "." + nameof(conn.collapse.start));
            sp_end           = serializedObject.FindProperty(nameof(conn.collapse) + "." + nameof(conn.collapse.end));
            sp_steps         = serializedObject.FindProperty(nameof(conn.collapse) + "." + nameof(conn.collapse.steps));
            sp_duration      = serializedObject.FindProperty(nameof(conn.collapse) + "." + nameof(conn.collapse.duration));
            sp_var           = serializedObject.FindProperty(nameof(conn.collapse) + "." + nameof(conn.collapse.var));
            sp_clp_seed      = serializedObject.FindProperty(nameof(conn.collapse) + "." + nameof(conn.collapse.seed));
            
            // Find Stress properties
            sp_showStress    = serializedObject.FindProperty(nameof(conn.showStress));
            sp_startStress   = serializedObject.FindProperty(nameof(conn.startStress));
            sp_str_integrity = serializedObject.FindProperty(nameof(conn.stressByIntegrity));
            sp_enable        = serializedObject.FindProperty(nameof(conn.stress) + "." + nameof(conn.stress.enable));
            sp_threshold     = serializedObject.FindProperty(nameof(conn.stress) + "." + nameof(conn.stress.threshold));
            sp_erosion       = serializedObject.FindProperty(nameof(conn.stress) + "." + nameof(conn.stress.erosion));
            sp_interval      = serializedObject.FindProperty(nameof(conn.stress) + "." + nameof(conn.stress.interval));
            sp_support       = serializedObject.FindProperty(nameof(conn.stress) + "." + nameof(conn.stress.support));
            sp_exposed       = serializedObject.FindProperty(nameof(conn.stress) + "." + nameof(conn.stress.exposed));
            sp_bySize        = serializedObject.FindProperty(nameof(conn.stress) + "." + nameof(conn.stress.bySize));
            
            // Foldouts
            if (EditorPrefs.HasKey (TextKeys.con_fld_flt) == true) fld_filters   = EditorPrefs.GetBool (TextKeys.con_fld_flt);
            if (EditorPrefs.HasKey (TextKeys.con_fld_clp) == true) fld_props_clp = EditorPrefs.GetBool (TextKeys.con_fld_clp);
            if (EditorPrefs.HasKey (TextKeys.con_fld_str) == true) fld_props_str = EditorPrefs.GetBool (TextKeys.con_fld_str);
            if (EditorPrefs.HasKey (TextKeys.con_fld_jnt) == true) fld_joints    = EditorPrefs.GetBool (TextKeys.con_fld_jnt);
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
            
            GUI_Preview();
            GUI_Info();
            GUI_Connectivity();
            
            if (conn.joints.enable == false)
            {
                GUI_Cluster();
                GUI_Collapse();
                GUI_Stress();
                GUI_Trigger();
            }

            // TODO update
            UI_Joints();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Top
        /// /////////////////////////////////////////////////////////
        
        void GUI_Preview()
        {
            EditorGUI.BeginChangeCheck();
            RFUI.Toggle (sp_show_giz, TextCnt.gui_btn_gizmo, RFUI.color_btn_blue);
            EditorGUILayout.BeginHorizontal();
            RFUI.Toggle (sp_show_con, TextCnt.gui_btn_cnt,   RFUI.color_btn_blue);
            RFUI.Toggle (sp_show_nod, TextCnt.gui_btn_nodes, RFUI.color_btn_blue);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }
        
        void GUI_Info()
        {
            if (conn.cluster.shards.Count > 0)
            {
                RFUI.Space();
                GUILayout.Label (TextCnt.str_setup, EditorStyles.boldLabel);
                RFUI.Space();
                GUILayout.Label (TextCnt.str_shards + conn.cluster.shards.Count + "/" + conn.initShardAmount);
                RFUI.Space ();
                GUILayout.Label (TextCnt.str_amount + conn.AmountIntegrity + "%");
                RFUI.Space ();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Connectivity
        /// /////////////////////////////////////////////////////////

        void GUI_Connectivity () 
        {
            RFUI.CaptionBox (TextCnt.gui_cap_conn);
            RFUI.PropertyField (sp_type, TextCnt.gui_type);
            if (conn.type != ConnectivityType.ByTriangles && conn.type != ConnectivityType.ByPolygons)
                RFUI.Slider (sp_expand, expand_min, expand_max, TextCnt.gui_expand);
            RFUI.Foldout (ref fld_filters, TextKeys.con_fld_flt, TextCnt.gui_fld_filters.text);
            if (fld_filters == true)
            {
                EditorGUI.indentLevel++;
                if (conn.type != ConnectivityType.ByBoundingBox)
                    RFUI.Slider (sp_minimumArea, minimumArea_min, minimumArea_max, TextCnt.gui_minimumArea);
                RFUI.Slider (sp_minimumSize, minimumSize_min, minimumSize_max, TextCnt.gui_minimumSize);
                RFUI.IntSlider (sp_percentage, percentage_min, percentage_max, TextCnt.gui_percentage);
                if (conn.percentage > 0)
                    RFUI.IntSlider (sp_seed, seed_min, seed_max, TextCnt.gui_seed);
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Cluster
        /// /////////////////////////////////////////////////////////
        
        void GUI_Cluster()
        {
            RFUI.CaptionBox (TextCnt.gui_cap_cluster);
            RFUI.PropertyField (sp_clusterize, TextCnt.gui_clusterize);
            if (conn.clusterize == true)
            {
                RFUI.PropertyField (sp_sub_enable, TextCnt.gui_sub_enable);
                if (conn.subEnable == true)
                {
                    EditorGUI.indentLevel++;
                    RFUI.IntSlider (sp_sub_thr, sub_thr_min, sub_thr_max, TextCnt.gui_sub_thr);
                    RFUI.IntSlider (sp_sub_min, sub_cls_min, sub_cls_max, TextCnt.gui_sub_min);
                    RFUI.IntSlider (sp_sub_max, sub_cls_min, sub_cls_max, TextCnt.gui_sub_max);
                    RFUI.IntSlider (sp_sub_deb, sub_deb_min, sub_deb_max, TextCnt.gui_sub_deb);
                    
                    EditorGUI.indentLevel--;
                }
                RFUI.PropertyField (sp_demolishable, TextCnt.gui_demolishable);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Collapse
        /// /////////////////////////////////////////////////////////
        
        void GUI_Collapse()
        {
            RFUI.CaptionBox (TextCnt.gui_cap_collapse);
            RFUI.PropertyField (sp_startCollapse, TextCnt.gui_startCollapse);
            if (conn.startCollapse == RayfireConnectivity.RFConnInitType.ByIntegrity)
                RFUI.IntSlider (sp_clp_integrity, integrity_min, integrity_max, TextCnt.gui_integrity);
            else if (conn.startCollapse == RayfireConnectivity.RFConnInitType.ByDamage)
            {
                RFUI.PropertyField (sp_clp_dmg_cur, TextCnt.gui_dmg_cur);
                RFUI.PropertyField (sp_clp_dmg_max, TextCnt.gui_dmg_max);
            }
            RFUI.Foldout (ref fld_props_clp, TextKeys.con_fld_clp, TextCnt.gui_fld_props.text);
            if (fld_props_clp == true)
            {
                EditorGUI.indentLevel++;
                RFUI.PropertyField (sp_clp_type, TextClp.gui_type);
                RFUI.IntSlider (sp_start, start_min, start_max, TextClp.gui_start);
                RFUI.IntSlider (sp_end,   end_min,   end_max,   TextClp.gui_end);
                RFUI.IntSlider (sp_steps, steps_min, steps_max, TextClp.gui_steps);
                RFUI.Slider (sp_duration, duration_min, duration_max, TextClp.gui_duration);
                if (conn.collapse.type != RFCollapse.RFCollapseType.Random)
                    RFUI.IntSlider (sp_var, var_min, var_max, TextClp.gui_var);
                RFUI.IntSlider (sp_clp_seed, seed_clp_min, seed_clp_max, TextClp.gui_seed);
                EditorGUI.indentLevel--;
            }
            
            GUI_Collapse_Buttons();
            GUI_Collapse_Sliders();
        }

        void GUI_Collapse_Buttons()
        {
            // Only runtime
            if (Application.isPlaying == false)
                return;

            RFUI.Space ();
                
            // Show start collapse if not Start by default
            if (conn.collapse.inProgress == false)
            {
                if (GUILayout.Button (TextCnt.gui_btn_clp_start, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireConnectivity != null)
                            RFCollapse.StartCollapse (targ as RayfireConnectivity);
            }

            // Show stop collapse if not Start by default
            if (conn.collapse.inProgress == true)
            {
                if (GUILayout.Button (TextCnt.gui_btn_clp_stop, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireConnectivity != null)
                            RFCollapse.StopCollapse (targ as RayfireConnectivity);
            }
        }

        void GUI_Collapse_Sliders()
        {
            // Only runtime
            if (Application.isPlaying == false)
                return;
            
            RFUI.Space ();
            
            GUILayout.BeginHorizontal();

            GUILayout.Label (TextClp.str_area, GUILayout.Width (55));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            conn.cluster.areaCollapse = EditorGUILayout.Slider(conn.cluster.areaCollapse, conn.cluster.minimumArea, conn.cluster.maximumArea);
            if (EditorGUI.EndChangeCheck() == true)
                RFCollapse.AreaCollapse (conn, conn.cluster.areaCollapse);

            EditorGUILayout.EndHorizontal();

            RFUI.Space ();
            
            GUILayout.BeginHorizontal();

            GUILayout.Label (TextClp.str_size, GUILayout.Width (55));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            conn.cluster.sizeCollapse = EditorGUILayout.Slider(conn.cluster.sizeCollapse, conn.cluster.minimumSize, conn.cluster.maximumSize);
            if (EditorGUI.EndChangeCheck() == true)
                RFCollapse.SizeCollapse (conn, conn.cluster.sizeCollapse);

            EditorGUILayout.EndHorizontal();

            RFUI.Space ();
            
            GUILayout.BeginHorizontal();

            GUILayout.Label (TextClp.str_rand, GUILayout.Width (55));

            // Start check for slider change
            EditorGUI.BeginChangeCheck();
            conn.cluster.randomCollapse = EditorGUILayout.IntSlider(conn.cluster.randomCollapse, 0, 100);
            if (EditorGUI.EndChangeCheck() == true)
                RFCollapse.RandomCollapse (conn, conn.cluster.randomCollapse);
            
            EditorGUILayout.EndHorizontal();
        }

        /// /////////////////////////////////////////////////////////
        /// Stress
        /// /////////////////////////////////////////////////////////

        void GUI_Stress()
        {
            RFUI.CaptionBox (TextCnt.gui_cap_stress);
            RFUI.PropertyField (sp_enable, TextStr.gui_enable);
            if (conn.stress.enable == true)
            {
                RFUI.PropertyField (sp_showStress,  TextCnt.gui_showStress);
                RFUI.PropertyField (sp_startStress, TextCnt.gui_startStress);
                if (conn.startStress == RayfireConnectivity.RFConnInitType.ByIntegrity)
                    RFUI.IntSlider (sp_str_integrity, integrity_min, integrity_max, TextCnt.gui_integrity);
                RFUI.Foldout (ref fld_props_str, TextKeys.con_fld_str, TextCnt.gui_fld_props.text);
                if (fld_props_str == true)
                    GUI_Stress_Properties();
                GUI_Stress_Buttons();
            }
        }
        
        void GUI_Stress_Properties()
        {
            EditorGUI.indentLevel++;
            RFUI.Caption (TextStr.gui_cap_conn);
            RFUI.IntSlider (sp_threshold, threshold_min, threshold_max, TextStr.gui_threshold);
            RFUI.Slider (sp_erosion,  erosion_min,  erosion_max,  TextStr.gui_erosion);
            RFUI.Slider (sp_interval, interval_min, interval_max, TextStr.gui_interval);
            RFUI.Caption (TextStr.gui_cap_shards);
            RFUI.IntSlider (sp_support, support_min, support_max, TextStr.gui_support);
            RFUI.PropertyField (sp_exposed, TextStr.gui_exposed);
            RFUI.PropertyField (sp_bySize,  TextStr.gui_bySize);
            EditorGUI.indentLevel--;
        }

        void GUI_Stress_Buttons()
        {
            if (Application.isPlaying == true)
            {
                RFUI.Space();

                // Show start stress if not Start by default
                if (conn.stress.inProgress == false)
                {
                    if (GUILayout.Button (TextCnt.gui_btn_str_start, GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireConnectivity != null)
                                RFStress.StartStress (targ as RayfireConnectivity);
                }

                // Show stop collapse if not Start by default
                if (conn.stress.inProgress == true)
                {
                    if (GUILayout.Button (TextCnt.gui_btn_str_stop, GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireConnectivity != null)
                                RFStress.StopStress (targ as RayfireConnectivity);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Trigger
        /// /////////////////////////////////////////////////////////

        // TODO update
        void GUI_Trigger()
        {
            RFUI.CaptionBox (TextCnt.gui_cap_fracture);
            RFUI.PropertyField (sp_triggerCollider, TextCnt.gui_triggerCollider);
            if (conn.clusterize == true)
                RFUI.IntSlider (sp_triggerDebris, debris_min, debris_max, TextCnt.gui_triggerDebris);

            if (Application.isPlaying == true && conn.triggerCollider != null)
            {
                RFUI.Space ();
                if (GUILayout.Button (TextCnt.gui_btn_fracture, GUILayout.Height (25)))
                    if (Application.isPlaying == true)
                        conn.Fracture (conn.triggerCollider, conn.triggerDebris);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Joints
        /// /////////////////////////////////////////////////////////

        void UI_Joints()
        {
            RFUI.CaptionBox (TextCnt.gui_cap_joint);

            EditorGUI.BeginChangeCheck();
            bool enable = EditorGUILayout.Toggle ("Enable", conn.joints.enable);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects (targets, "Enable");
                foreach (RayfireConnectivity scr in targets)
                {
                    scr.joints.enable = enable;
                    RFUI.SetDirty (scr.gameObject);
                }
            }

            if (conn.joints.enable == true)
            {
                RFUI.Space ();

                //exp_joints = EditorGUILayout.Foldout (exp_joints, "Properties", true);
                //if (exp_joints == true)
                {
                    EditorGUI.indentLevel++;

                    GUILayout.Label ("  Break", EditorStyles.boldLabel);
                    
                    EditorGUI.BeginChangeCheck();
                    conn.joints.breakType = (RFJointProperties.RFJointBreakType)EditorGUILayout.EnumPopup ("Type", conn.joints.breakType);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.breakType = conn.joints.breakType;
                            RFUI.SetDirty (scr.gameObject);
                        }
                    
                    RFUI.Space ();

                    EditorGUI.BeginChangeCheck();
                    conn.joints.breakForce = EditorGUILayout.IntSlider ("Force", conn.joints.breakForce, 0, 5000);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.breakForce = conn.joints.breakForce;
                            RFUI.SetDirty (scr.gameObject);

                            if (Application.isPlaying == true)
                                RFJointProperties.SetBreakForce (scr.joints.breakForce, scr.joints.breakForceVar, scr.joints.jointList, scr.joints.forceByMass);
                        }

                    RFUI.Space ();

                    EditorGUI.BeginChangeCheck();
                    conn.joints.breakForceVar = EditorGUILayout.IntSlider ("Variation", conn.joints.breakForceVar, 0, 100);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.breakForceVar = conn.joints.breakForceVar;
                            RFUI.SetDirty (scr.gameObject);

                            if (Application.isPlaying == true)
                                RFJointProperties.SetBreakForce (scr.joints.breakForce, scr.joints.breakForceVar, scr.joints.jointList, scr.joints.forceByMass);
                        }
                    
                    RFUI.Space ();
                    
                    EditorGUI.BeginChangeCheck();
                    conn.joints.forceByMass = EditorGUILayout.Toggle ("Force By Mass", conn.joints.forceByMass);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.forceByMass = conn.joints.forceByMass;
                            RFUI.SetDirty (scr.gameObject);
                        }
                    
                    RFUI.Space ();
                    
                    GUILayout.Label ("  Angular", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    conn.joints.angleLimit = EditorGUILayout.IntSlider ("Limit", conn.joints.angleLimit, 0, 50);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.angleLimit = conn.joints.angleLimit;
                            RFUI.SetDirty (scr.gameObject);

                            if (Application.isPlaying == true)
                                RFJointProperties.SetAngularMotion (scr.joints.angleLimit, scr.joints.angleLimitVar, scr.joints.jointList);
                        }

                    RFUI.Space ();
                    
                    EditorGUI.BeginChangeCheck();
                    conn.joints.angleLimitVar = EditorGUILayout.IntSlider ("Variation", conn.joints.angleLimitVar, 0, 100);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.angleLimitVar = conn.joints.angleLimitVar;
                            RFUI.SetDirty (scr.gameObject);

                            if (Application.isPlaying == true)
                                RFJointProperties.SetAngularMotion (scr.joints.angleLimit, scr.joints.angleLimitVar, scr.joints.jointList);
                        }

                    RFUI.Space ();

                    EditorGUI.BeginChangeCheck();
                    conn.joints.damper = EditorGUILayout.IntSlider ("Damper", conn.joints.damper, 0, 10000);
                    if (EditorGUI.EndChangeCheck() == true)
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.damper = conn.joints.damper;
                            RFUI.SetDirty (scr.gameObject);

                            if (Application.isPlaying == true)
                                RFJointProperties.SetSpring (scr.joints.damper, scr.joints.jointList);
                        }

                    RFUI.Space ();
                    
                    GUILayout.Label ("  Deformation", EditorStyles.boldLabel);
                    
                    EditorGUI.BeginChangeCheck();
                    conn.joints.deformEnable = EditorGUILayout.Toggle ("Enable", conn.joints.deformEnable);
                    if (EditorGUI.EndChangeCheck())
                        foreach (RayfireConnectivity scr in targets)
                        {
                            scr.joints.deformEnable = conn.joints.deformEnable;
                            RFUI.SetDirty (scr.gameObject);
                        }

                    if (conn.joints.deformEnable == true)
                    {
                        RFUI.Space ();

                        // Stiffness
                        if (conn.joints.breakType == RFJointProperties.RFJointBreakType.Breakable)
                        {
                            EditorGUI.BeginChangeCheck();
                            conn.joints.stiffFrc = EditorGUILayout.Slider ("Stiffness", conn.joints.stiffFrc, 0.05f, 0.95f);
                            if (EditorGUI.EndChangeCheck() == true)
                                foreach (RayfireConnectivity scr in targets)
                                {
                                    scr.joints.stiffFrc = conn.joints.stiffFrc;
                                    RFUI.SetDirty (scr.gameObject);
                                }
                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();
                            conn.joints.stiffAbs = EditorGUILayout.IntSlider ("Stiffness", conn.joints.stiffAbs, 1, 500);
                            if (EditorGUI.EndChangeCheck() == true)
                                foreach (RayfireConnectivity scr in targets)
                                {
                                    scr.joints.stiffAbs = conn.joints.stiffAbs;
                                    RFUI.SetDirty (scr.gameObject);
                                }
                        }

                        RFUI.Space ();

                        EditorGUI.BeginChangeCheck();
                        conn.joints.bend = EditorGUILayout.IntSlider ("Bending", conn.joints.bend, 0, 10);
                        if (EditorGUI.EndChangeCheck() == true)
                            foreach (RayfireConnectivity scr in targets)
                            {
                                scr.joints.bend = conn.joints.bend;
                                RFUI.SetDirty (scr.gameObject);
                            }

                        RFUI.Space ();

                        EditorGUI.BeginChangeCheck();
                        conn.joints.weakening = EditorGUILayout.Slider ("Weakening", conn.joints.weakening, 0, 0.9f);
                        if (EditorGUI.EndChangeCheck() == true)
                            foreach (RayfireConnectivity scr in targets)
                            {
                                scr.joints.weakening = conn.joints.weakening;
                                RFUI.SetDirty (scr.gameObject);
                            }

                        RFUI.Space ();

                        EditorGUI.BeginChangeCheck();
                        conn.joints.percentage = EditorGUILayout.IntSlider ("Percentage", conn.joints.percentage, 1, 100);
                        if (EditorGUI.EndChangeCheck() == true)
                            foreach (RayfireConnectivity scr in targets)
                            {
                                scr.joints.percentage = conn.joints.percentage;
                                RFUI.SetDirty (scr.gameObject);
                            }

                        RFUI.Space ();

                        EditorGUI.BeginChangeCheck();
                        conn.joints.deformCount = EditorGUILayout.IntSlider ("Iterations", conn.joints.deformCount, 1, 100);
                        if (EditorGUI.EndChangeCheck() == true)
                            foreach (RayfireConnectivity scr in targets)
                            {
                                scr.joints.deformCount = conn.joints.deformCount;
                                RFUI.SetDirty (scr.gameObject);
                            }

                        if (conn.joints.HasDeforms == true)
                        {
                            RFUI.Space ();
                            GUILayout.Label ("Deformable joints: " + conn.joints.deformList.Count + "/" + conn.joints.jointList.Count);
                        }
                           
                    }

           
                    
                    // GUILayout.Label ("CurrentForce");
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        // Draw Connections and Nodes by act and uny states
        static void ClusterDraw(RFCluster cluster, bool showNodes, bool showConnections)
        {
            if (showNodes == true || showConnections == true)
            {
                if (cluster != null && cluster.shards.Count > 0)
                {
                    for (int i = 0; i < cluster.shards.Count; i++)
                    {
                        if (cluster.shards[i].tm != null)
                        {
                            // Color
                            if (cluster.shards[i].rigid == null)
                                SetColor (cluster.shards[i].uny, cluster.shards[i].act);
                            else 
                            {
                                if (cluster.shards[i].rigid.objTp == ObjectType.Mesh)
                                    SetColor (cluster.shards[i].rigid.activation.uny, cluster.shards[i].rigid.activation.atb);
                                else if (cluster.shards[i].rigid.objTp == ObjectType.MeshRoot ||
                                         cluster.shards[i].rigid.objTp == ObjectType.ConnectedCluster)
                                    SetColor (cluster.shards[i].uny, cluster.shards[i].act);
                            }

                            // Nodes
                            if (showNodes == true)
                                Gizmos.DrawWireSphere (cluster.shards[i].tm.position, cluster.shards[i].sz / 12f);
                            
                            // Connection
                            if (showConnections == true)
                            {
                                //Debug.Log (cluster.shards[i].nIds.Count);
                                
                                // Has no neibs
                                if (cluster.shards[i].nIds.Count == 0)
                                    continue;
                                
                                // Shard has neibs but neib shards not initialized by nIds
                                if (cluster.shards[i].neibShards == null)
                                    cluster.shards[i].neibShards = new List<RFShard>();
                                
                                // Reinit
                                if (cluster.shards[i].neibShards.Count == 0)
                                    for (int n = 0; n < cluster.shards[i].nIds.Count; n++)
                                        cluster.shards[i].neibShards.Add (cluster.shards[cluster.shards[i].nIds[n]]);
                                
                                // Preview
                                for (int j = 0; j < cluster.shards[i].neibShards.Count; j++)
                                    if (cluster.shards[i].neibShards[j].tm != null)
                                    {
                                        Gizmos.DrawLine (cluster.shards[i].tm.position, 
                                            (cluster.shards[i].neibShards[j].tm.position - cluster.shards[i].tm.position) / 2f + cluster.shards[i].tm.position);
                                    }
                            }
                        }
                    }
                }
            }
        }
        
        // Draw stressed connections
        static void StressDraw (RayfireConnectivity targ)
        {
            if (targ.showStress == true && targ.stress != null && targ.stress.inProgress == true)
            {
                if (targ.cluster != null && targ.cluster.shards.Count > 0)
                {
                    Vector3 pos;
                    for (int i = 0; i < targ.cluster.shards.Count; i++)
                    {
                        if (targ.cluster.shards[i].tm != null)
                        {
                            // Show Path stress
                            /*
                            if (false)
                                if (targ.stress.bySize == true)
                                {
                                    Gizmos.color = ColorByValue (stressColor, targ.cluster.shards[i].sSt, 1f);
                                    Gizmos.DrawWireSphere (targ.cluster.shards[i].tm.position, targ.cluster.shards[i].sz / 12f);
                                }
                            */
                            
                            if (targ.cluster.shards[i].StressState == true)
                            {
                                for (int n = 0; n < targ.cluster.shards[i].nSt.Count / 3; n++)
                                {
                                    if (targ.cluster.shards[i].uny == true)
                                    {
                                        Gizmos.color = Color.yellow;
                                    }
                                    else
                                    {
                                        Gizmos.color = targ.cluster.shards[i].sIds.Count > 0 
                                            ? Color.yellow 
                                            : ColorByValue (stressColor, targ.cluster.shards[i].nSt[n * 3], targ.stress.threshold);
                                    }
                                    
                                    pos = (targ.cluster.shards[i].neibShards[n].tm.position - targ.cluster.shards[i].tm.position) / 2.5f + targ.cluster.shards[i].tm.position;
                                    Gizmos.DrawLine (targ.cluster.shards[i].tm.position, pos);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Set gizmo color by uny and act states
        static void SetColor (bool uny, bool act)
        {
            if (uny == false)
                Gizmos.color = Color.green;
            else
                Gizmos.color = act == true ? Color.magenta : Color.red;
        }
        
        // Color by value
        static Color ColorByValue(Color color, float val, float threshold)
        {
            val     /= threshold;
            color.g =  1f - val;
            color.r =  val;
            return color;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireConnectivity targ, GizmoType gizmoType)
        {
            // Draw bounding gizmo
            GizmoDraw (targ);
            
            // Has no shards
            if (targ.cluster == null)
                return;
            
            /*
            // Missing shards
            // if (targ.cluster.shards.Count > 0)
            {
                if (RFCluster.IntegrityCheck (targ.cluster) == false)
                    Debug.Log ("RayFire Connectivity: " + targ.name + " has missing shards. Reset or Setup cluster.", targ.gameObject);
                else
                    targ.integrityCheck = false;
            }
            */
            
            // Draw for MeshRoot and RigidRoot in runtime
            if (Application.isPlaying == true || targ.meshRootHost != null)
            {
                ClusterDraw (targ.cluster, targ.showNodes, targ.showConnections);
            }

            // Draw for RigidRoot because Connectivity do not store same shard list
            else if (targ.rigidRootHost != null)
                ClusterDraw (targ.rigidRootHost.cluster, targ.showNodes, targ.showConnections);

            // Draw stresses connections
            StressDraw (targ);
        }

        static void GizmoDraw (RayfireConnectivity targ)
        {
            if (targ.showGizmo == true)
            {
                // Gizmo properties
                Gizmos.color = RFUI.color_blue;
                if (targ.transform.childCount > 0)
                {
                    Bounds bound = RFCluster.GetChildrenBound (targ.transform);
                    Gizmos.DrawWireCube (bound.center, bound.size);
                }
            }
        }
    }
}

        /*
        
        /// /////////////////////////////////////////////////////////
        /// Handle selection
        /// /////////////////////////////////////////////////////////
        
        static         Vector2Int currentShardConnection;
		private static int        s_ButtonHash = "ConnectionHandle".GetHashCode();
        
        void OnSceneGUI()
		{
            var targ = conn;
			if (targ == null)
				return;
            
			if (targ.showConnections == true)
			{
				if (targ.cluster != null && targ.cluster.shards.Count > 0)
				{
					int count = targ.cluster.shards.Count;
					for (int i = 0; i < count; i++)
					{
						if (targ.cluster.shards[i].tm != null)
						{
							if (targ.cluster.shards[i].nIds.Count == 0)
								continue;

							if (targ.cluster.shards[i].neibShards != null && targ.cluster.shards[i].neibShards.Count != 0)
							{
								int nCount = targ.cluster.shards[i].neibShards.Count;
								for (int j = 0; j < nCount; j++)
								{
									if (targ.cluster.shards[i].neibShards[j].tm != null)
									{
										Vector3 start = targ.cluster.shards[i].tm.position;
										Vector3 end = start + (targ.cluster.shards[i].neibShards[j].tm.position - start) * 0.5f;
										HandleClick(start, end, targ.cluster.shards[i].id, targ.cluster.shards[i].neibShards[j].id);
                                        
                                        
									}
								}
							}
						}
					}
				}
			}
		}
        
		private static void HandleClick(Vector3 start, Vector3 end, int id1, int id2)
		{
			int id = GUIUtility.GetControlID(s_ButtonHash, FocusType.Passive);
			Event evt = Event.current;

			switch (evt.GetTypeForControl(id))
			{
				case EventType.Layout:
				{
					HandleUtility.AddControl(id, HandleUtility.DistanceToLine(start, end));
					break;
				}
                case EventType.MouseMove:
                {
                    if (id == HandleUtility.nearestControl)
                        HandleUtility.Repaint();
                    break;
                }
                case EventType.MouseDown:
				{
					if (HandleUtility.nearestControl == id && evt.button == 0)
					{
						GUIUtility.hotControl = id; // Grab mouse focus
						HandleClickSelection(evt, id1, id2);
						evt.Use();
					}
					break;
				}
			}
		}

		public static void HandleClickSelection(Event evt, int id1, int id2)
		{
			currentShardConnection.x = id1;
			currentShardConnection.y = id2;
            
            
		}
        
        private void DeleteSelectedConnection()
        {
            var targ = conn;
            if (targ.showConnections == true)
            {
                if (targ.cluster != null && targ.cluster.shards.Count > 0)
                {
                    int count = targ.cluster.shards.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (targ.cluster.shards[i].tm != null)
                        {
                            if (targ.cluster.shards[i].nIds.Count == 0)
                                continue;

                            if (targ.cluster.shards[i].neibShards != null && targ.cluster.shards[i].neibShards.Count != 0)
                            {
                                int nCount = targ.cluster.shards[i].neibShards.Count - 1;
                                for (int j = nCount; j >= 0; --j)
                                {
                                    if (targ.cluster.shards[i].neibShards[j].tm != null)
                                    {
                                        var id  = targ.cluster.shards[i].id;
                                        var nId = targ.cluster.shards[i].neibShards[j].id;
                                        if (currentShardConnection.x == id && currentShardConnection.y == nId || currentShardConnection.y == id && currentShardConnection.x == nId)
                                            targ.cluster.shards[i].RemoveNeibAt(j);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        */

/*
 if (targ.cluster.shards[i].uny == true)
    {
        Gizmos.color = Color.yellow;
    }
    else
    {
        //if (targ.cluster.shards[i].sIds.Count > 0)
        //{
            if (targ.cluster.shards[i].neibShards[n].sIds.Contains (targ.cluster.shards[i].id) == true || targ.cluster.shards[i].sIds.Contains (targ.cluster.shards[i].neibShards[n].id) == true)
            {
                Gizmos.color = Color.yellow;
            }
        //}
            else
                Gizmos.color     = ColorByValue (stressColor, targ.cluster.shards[i].nStr[n * 3], targ.stress.threshold);
    }




                                    if (targ.cluster.shards[i].uny == true || targ.cluster.shards[i].sIds.Count > 0)
                                        Gizmos.color = Color.yellow;
                                    else
                                        Gizmos.color = ColorByValue (stressColor, targ.cluster.shards[i].nStr[n*3], targ.stress.threshold);

*/