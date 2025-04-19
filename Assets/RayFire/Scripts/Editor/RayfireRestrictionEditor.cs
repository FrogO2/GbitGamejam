using UnityEditor;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireRestriction))]
    public class RayfireRestrictionEditor : Editor
    {
        RayfireRestriction rest;
        
        // Minimum & Maximum ranges
        const float delay_min    = 0;
        const float delay_max    = 60f;
        const float interval_min = 0.1f;
        const float interval_max = 60f;
        const float distance_min = 0f;
        const float distance_max = 90f;
        
        // Serialized properties
        SerializedProperty sp_rigid;
        SerializedProperty sp_prp_en;
        SerializedProperty sp_prp_act;
        SerializedProperty sp_prp_del;
        SerializedProperty sp_prp_int;
        SerializedProperty sp_dst_pos;
        SerializedProperty sp_dst_val;
        SerializedProperty sp_dst_trg;
        SerializedProperty sp_tri_reg;
        SerializedProperty sp_tri_col;

        private void OnEnable()
        {
            // Get component
            rest = (RayfireRestriction)target;

            // Find properties
            sp_rigid   = serializedObject.FindProperty(nameof(rest.rigid));
            sp_prp_en  = serializedObject.FindProperty(nameof(rest.enable));
            sp_prp_act = serializedObject.FindProperty(nameof(rest.breakAction));
            sp_prp_del = serializedObject.FindProperty(nameof(rest.actionDelay));
            sp_prp_int = serializedObject.FindProperty(nameof(rest.checkInterval));
            sp_dst_pos = serializedObject.FindProperty(nameof(rest.position));
            sp_dst_val = serializedObject.FindProperty(nameof(rest.distance));
            sp_dst_trg = serializedObject.FindProperty(nameof(rest.target));
            sp_tri_reg = serializedObject.FindProperty(nameof(rest.region));
            sp_tri_col = serializedObject.FindProperty(nameof(rest.coll));
        }

        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            RFUI.PropertyField (sp_rigid, TextRst.gui_rigid);

            GUI_Prop();
            GUI_Dist();
            GUI_Trig();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////

        void GUI_Prop()
        {
            RFUI.CaptionBox (TextRst.gui_cap_props);
            RFUI.PropertyField (sp_prp_en,  TextRst.gui_prp_en);
            RFUI.PropertyField (sp_prp_act, TextRst.gui_prp_act);
            RFUI.Slider (sp_prp_del, delay_min,    delay_max, TextRst.gui_prp_del);
            RFUI.Slider (sp_prp_int, interval_min, interval_max, TextRst.gui_prp_int);
        }

        void GUI_Dist()
        {
            RFUI.CaptionBox (TextRst.gui_cap_dst);
            RFUI.PropertyField (sp_dst_pos, TextRst.gui_dst_pos);
            RFUI.Slider (sp_dst_val, distance_min, distance_max, TextRst.gui_dst_val);
            if (rest.position == RayfireRestriction.RFDistanceType.TargetPosition)
                RFUI.PropertyField (sp_dst_trg, TextRst.gui_dst_trg);
        }

        void GUI_Trig()
        {
            RFUI.CaptionBox (TextRst.gui_cap_tri);
            RFUI.PropertyField (sp_tri_reg, TextRst.gui_tri_reg);
            RFUI.PropertyField (sp_tri_col, TextRst.gui_tri_col);
        }
    }
}