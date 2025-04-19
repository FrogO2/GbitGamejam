using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireShatter))]
    public class RayfireShatterEditor : Editor
    {
        RayfireShatter  shatter;
        Transform       transForm;
        Vector3         centerWorldPos;
        Quaternion      centerWorldQuat;
        ReorderableList rl_tp_cus_tms;
        ReorderableList rl_tp_cus_list;
        ReorderableList rl_tp_slc_list;
        EditorWindow    uvwEditor;
        Texture2D       uvTexture;
        
        // Foldout
        static bool        exp_deb;
        static bool        exp_lim;
        static bool        exp_fil;
        
        // Frag Types Minimum & Maximum ranges
        const float bias_min               = 0;
        const float bias_max               = 1f;
        const float strength_min           = 0;
        const float strength_max           = 1f;
        const float rad_radius_min         = 0.01f;
        const float rad_radius_max         = 30f;
        const float rad_div_min            = 0f;
        const float rad_div_max            = 1f;
        const int   rad_rings_min          = 3;
        const int   rad_rings_max          = 60;
        const int   rad_rand_min           = 0;
        const int   rad_rand_max           = 100;
        const int   rad_focus_min          = 0;
        const int   rad_focus_max          = 100;
        const int   rad_twist_min          = -90;
        const int   rad_twist_max          = 90;
        const float hex_size_min           = 0.01f;
        const float hex_size_max           = 10f;
        const int   hex_row_min            = 3;
        const int   hex_row_max            = 200;
        const int   cus_amount_min         = 3;
        const int   cus_amount_max         = 999;
        const float cus_radius_min         = 0.01f;
        const float cus_radius_max         = 10f;
        const float cus_size_min           = 0.01f;
        const float cus_size_max           = 0.4f;
        const float brick_mult_min         = 0.1f;
        const float brick_mult_max         = 10f;
        const int   brick_amount_min       = 0;
        const int   brick_amount_max       = 50;
        const float brick_size_min         = 0.01f;
        const float brick_size_max         = 10f;
        const int   brick_var_min          = 0;
        const int   brick_var_max          = 100;
        const float brick_offset_min       = 0f;
        const float brick_offset_max       = 1f;
        const int   brick_prob_min         = 0;
        const int   brick_prob_max         = 100;
        const int   brick_rotation_min     = 0;
        const int   brick_rotation_max     = 90;
        const float brick_split_offset_min = 0f;
        const float brick_split_offset_max = 0.95f;
        const float vox_size_min           = 0.05f;
        const float vox_size_max           = 10f;
        const int   tet_dens_min           = 1;
        const int   tet_dens_max           = 50;
        const int   tet_noise_min          = 0;
        const int   tet_noise_max          = 100;
        
        // Material Minimum & Maximum ranges
        const float mat_scale_min = 0.01f;
        const float mat_scale_max = 5f;
        
        // Clusters Minimum & Maximum ranges
        const int   cls_count_min  = 2;
        const int   cls_count_max  = 200;
        const int   cls_seed_min   = 0;
        const int   cls_seed_max   = 100;
        const float cls_relax_min  = 0;
        const float cls_relax_max  = 1f;
        const int   cls_amount_min = 0;
        const int   cls_amount_max = 100;
        const int   cls_layers_min = 0;
        const int   cls_layers_max = 5; 
        const float cls_scale_min  = 0;
        const float cls_scale_max  = 1f;
        const int   cls_frags_min  = 0;
        const int   cls_frags_max  = 5; 
        
        // Advanced Minimum & Maximum ranges
        const float adv_size_min    = 0.1f;
        const float adv_size_max    = 100f;
        const int   adv_vert_min    = 100;
        const int   adv_vert_max    = 1900;
        const int   adv_tris_min    = 100;
        const int   adv_tris_max    = 1900;
        const int   adv_rel_min     = 0;
        const int   adv_rel_max     = 10;
        const float adv_abs_min     = 0;
        const float adv_abs_max     = 1f;
        const int   adv_element_min = 1;
        const int   adv_element_max = 100;
        
        // Shell Minimum & Maximum ranges
        const float shl_thick_min = 0.005f;
        const float shl_thick_max = 10;
        
        // Frag Types Serialized properties
        SerializedProperty sp_engine;
        SerializedProperty sp_tp;
        SerializedProperty sp_interactive;
        SerializedProperty sp_tp_vor_amount;
        SerializedProperty sp_tp_vor_bias;
        SerializedProperty sp_tp_spl_axis;
        SerializedProperty sp_tp_spl_amount;
        SerializedProperty sp_tp_spl_str;
        SerializedProperty sp_tp_spl_bias;
        SerializedProperty sp_tp_slb_axis;
        SerializedProperty sp_tp_slb_amount;
        SerializedProperty sp_tp_slb_str;
        SerializedProperty sp_tp_slb_bias;
        SerializedProperty sp_tp_rad_axis;
        SerializedProperty sp_tp_rad_radius;
        SerializedProperty sp_tp_rad_div;
        SerializedProperty sp_tp_rad_rest;
        SerializedProperty sp_tp_rad_rings;
        SerializedProperty sp_tp_rad_focus;
        SerializedProperty sp_tp_rad_str;
        SerializedProperty sp_tp_rad_randRing;
        SerializedProperty sp_tp_rad_rays;
        SerializedProperty sp_tp_rad_randRay;
        SerializedProperty sp_tp_rad_twist;
        SerializedProperty sp_tp_hex_size;
        SerializedProperty sp_tp_hex_en;
        SerializedProperty sp_tp_hex_pl;
        SerializedProperty sp_tp_hex_row;
        SerializedProperty sp_tp_hex_col;
        SerializedProperty sp_tp_hex_div;
        SerializedProperty sp_tp_hex_rest;
        SerializedProperty sp_tp_cus_src;
        SerializedProperty sp_tp_cus_use;
        SerializedProperty sp_tp_cus_am;
        SerializedProperty sp_tp_cus_rad;
        SerializedProperty sp_tp_cus_en;
        SerializedProperty sp_tp_cus_sz;
        SerializedProperty sp_tp_cus_list;
        SerializedProperty sp_tp_cus_tms;
        SerializedProperty sp_tp_slc_pl;
        SerializedProperty sp_tp_slc_list;
        SerializedProperty sp_tp_brk_type;
        SerializedProperty sp_tp_brk_mult;
        SerializedProperty sp_tp_brk_am_X;
        SerializedProperty sp_tp_brk_am_Y;
        SerializedProperty sp_tp_brk_am_Z;
        SerializedProperty sp_tp_brk_lock;
        SerializedProperty sp_tp_brk_sz_X;
        SerializedProperty sp_tp_brk_sz_Y;
        SerializedProperty sp_tp_brk_sz_Z;
        SerializedProperty sp_tp_brk_sz_var_X;
        SerializedProperty sp_tp_brk_sz_var_Y;
        SerializedProperty sp_tp_brk_sz_var_Z;
        SerializedProperty sp_tp_brk_of_X;
        SerializedProperty sp_tp_brk_of_Y;
        SerializedProperty sp_tp_brk_of_Z;
        SerializedProperty sp_tp_brk_sp_X;
        SerializedProperty sp_tp_brk_sp_Y;
        SerializedProperty sp_tp_brk_sp_Z;
        SerializedProperty sp_tp_brk_sp_prob;
        SerializedProperty sp_tp_brk_sp_offs;
        SerializedProperty sp_tp_brk_sp_rot;
        SerializedProperty sp_tp_vxl_sz;
        SerializedProperty sp_tp_tet_dn;
        SerializedProperty sp_tp_tet_ns;
        
        // Material Serialized properties
        SerializedProperty sp_mat_scl;
        SerializedProperty sp_mat_in;
        SerializedProperty sp_mat_col_en;
        SerializedProperty sp_mat_uve_en;
        SerializedProperty sp_mat_col;
        SerializedProperty sp_mat_uve;
        SerializedProperty sp_mat_uvr;
        
        // Clusters Serialized properties
        SerializedProperty sp_cls_en;
        SerializedProperty sp_cls_cnt;
        SerializedProperty sp_cls_seed;
        SerializedProperty sp_cls_rel;
        SerializedProperty sp_cls_amount;
        SerializedProperty sp_cls_layers;
        SerializedProperty sp_cls_scale;
        SerializedProperty sp_cls_min;
        SerializedProperty sp_cls_max;
        
        // Advanced Serialized properties
        SerializedProperty sp_mode;
        SerializedProperty sp_adv_seed;
        SerializedProperty sp_adv_copy;
        SerializedProperty sp_adv_smooth;
        SerializedProperty sp_adv_children;
        SerializedProperty sp_adv_col;
        SerializedProperty sp_adv_dec;
        SerializedProperty sp_adv_input;
        SerializedProperty sp_adv_output;
        SerializedProperty sp_adv_lim;
        SerializedProperty sp_adv_size_lim;
        SerializedProperty sp_adv_size_am;
        SerializedProperty sp_adv_vert_lim;
        SerializedProperty sp_adv_vert_am;
        SerializedProperty sp_adv_tri_lim;
        SerializedProperty sp_adv_tri_am;
        SerializedProperty sp_adv_inner;
        SerializedProperty sp_adv_planar;
        SerializedProperty sp_adv_rel;
        SerializedProperty sp_adv_abs;
        SerializedProperty sp_adv_element;
        SerializedProperty sp_adv_remove;

        SerializedProperty sp_adv_hierarchy;
        SerializedProperty sp_adv_separate;
        SerializedProperty sp_adv_combine;
        SerializedProperty sp_adv_slc;
        SerializedProperty sp_adv_scl;
        
        SerializedProperty sp_shl_en;
        SerializedProperty sp_shl_fr;
        SerializedProperty sp_shl_br;
        SerializedProperty sp_shl_sub;
        SerializedProperty sp_shl_thick;
        
        SerializedProperty sp_aabb_en;
        SerializedProperty sp_aabb_sep;
        SerializedProperty sp_aabb_obj;
        
        SerializedProperty sp_cn_pos;
        SerializedProperty sp_cn_sh;
        SerializedProperty sp_cn_set;
        SerializedProperty sp_cn_obj;
        
        private void OnEnable()
        {
            // Get component
            shatter = (RayfireShatter)target;
            
            // Find Frag Types Serialized properties
            sp_engine          = serializedObject.FindProperty(nameof(shatter.engine));
            sp_tp              = serializedObject.FindProperty(nameof(shatter.type));
            sp_interactive     = serializedObject.FindProperty(nameof(shatter.interactive));
            sp_tp_vor_amount   = serializedObject.FindProperty(nameof(shatter.voronoi) + "." + nameof(shatter.voronoi.amount));
            sp_tp_vor_bias     = serializedObject.FindProperty(nameof(shatter.voronoi) + "." + nameof(shatter.voronoi.centerBias));
            sp_tp_spl_axis     = serializedObject.FindProperty(nameof(shatter.splinters) + "." + nameof(shatter.splinters.axis));
            sp_tp_spl_amount   = serializedObject.FindProperty(nameof(shatter.splinters) + "." + nameof(shatter.splinters.amount));
            sp_tp_spl_str      = serializedObject.FindProperty(nameof(shatter.splinters) + "." + nameof(shatter.splinters.strength));
            sp_tp_spl_bias     = serializedObject.FindProperty(nameof(shatter.splinters) + "." + nameof(shatter.splinters.centerBias));
            sp_tp_slb_axis     = serializedObject.FindProperty(nameof(shatter.slabs) + "." + nameof(shatter.slabs.axis));
            sp_tp_slb_amount   = serializedObject.FindProperty(nameof(shatter.slabs) + "." + nameof(shatter.slabs.amount));
            sp_tp_slb_str      = serializedObject.FindProperty(nameof(shatter.slabs) + "." + nameof(shatter.slabs.strength));
            sp_tp_slb_bias     = serializedObject.FindProperty(nameof(shatter.slabs) + "." + nameof(shatter.slabs.centerBias));
            sp_tp_rad_axis     = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.centerAxis));
            sp_tp_rad_radius   = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.radius));
            sp_tp_rad_div      = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.divergence));
            sp_tp_rad_rest     = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.restrictToPlane));
            sp_tp_rad_rings    = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.rings));
            sp_tp_rad_focus    = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.focus));
            sp_tp_rad_str      = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.focusStr));
            sp_tp_rad_randRing = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.randomRings));
            sp_tp_rad_rays     = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.rays));
            sp_tp_rad_randRay  = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.randomRays));
            sp_tp_rad_twist    = serializedObject.FindProperty(nameof(shatter.radial) + "." + nameof(shatter.radial.twist));
            sp_tp_hex_size     = serializedObject.FindProperty(nameof(shatter.hexagon) + "." + nameof(shatter.hexagon.size));
            sp_tp_hex_en       = serializedObject.FindProperty(nameof(shatter.hexagon) + "." + nameof(shatter.hexagon.enable));
            sp_tp_hex_pl       = serializedObject.FindProperty(nameof(shatter.hexagon) + "." + nameof(shatter.hexagon.plane));
            sp_tp_hex_row      = serializedObject.FindProperty(nameof(shatter.hexagon) + "." + nameof(shatter.hexagon.row));
            sp_tp_hex_col      = serializedObject.FindProperty(nameof(shatter.hexagon) + "." + nameof(shatter.hexagon.col));
            sp_tp_hex_div      = serializedObject.FindProperty(nameof(shatter.hexagon) + "." + nameof(shatter.hexagon.div));
            sp_tp_hex_rest     = serializedObject.FindProperty(nameof(shatter.hexagon) + "." + nameof(shatter.hexagon.rest));
            sp_tp_cus_src      = serializedObject.FindProperty(nameof(shatter.custom) + "." + nameof(shatter.custom.source));
            sp_tp_cus_use      = serializedObject.FindProperty(nameof(shatter.custom) + "." + nameof(shatter.custom.useAs));
            sp_tp_cus_am       = serializedObject.FindProperty(nameof(shatter.custom) + "." + nameof(shatter.custom.amount));
            sp_tp_cus_rad      = serializedObject.FindProperty(nameof(shatter.custom) + "." + nameof(shatter.custom.radius));
            sp_tp_cus_en       = serializedObject.FindProperty(nameof(shatter.custom) + "." + nameof(shatter.custom.enable));
            sp_tp_cus_sz       = serializedObject.FindProperty(nameof(shatter.custom) + "." + nameof(shatter.custom.size));
            sp_tp_cus_list     = serializedObject.FindProperty(nameof(shatter.custom) + "." + nameof(shatter.custom.vector3));
            sp_tp_cus_tms      = serializedObject.FindProperty(nameof(shatter.custom) + "." + nameof(shatter.custom.transforms));
            sp_tp_slc_pl       = serializedObject.FindProperty(nameof(shatter.slice) + "." + nameof(shatter.slice.plane));
            sp_tp_slc_list     = serializedObject.FindProperty(nameof(shatter.slice) + "." + nameof(shatter.slice.sliceList));
            sp_tp_brk_type     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.amountType));
            sp_tp_brk_mult     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.mult));
            sp_tp_brk_am_X     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.amount_X));
            sp_tp_brk_am_Y     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.amount_Y));
            sp_tp_brk_am_Z     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.amount_Z));
            sp_tp_brk_lock     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.size_Lock));
            sp_tp_brk_sz_X     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.size_X));
            sp_tp_brk_sz_Y     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.size_Y));
            sp_tp_brk_sz_Z     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.size_Z));
            sp_tp_brk_sz_var_X = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.sizeVar_X));
            sp_tp_brk_sz_var_Y = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.sizeVar_Y));
            sp_tp_brk_sz_var_Z = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.sizeVar_Z));
            sp_tp_brk_of_X     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.offset_X));
            sp_tp_brk_of_Y     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.offset_Y));
            sp_tp_brk_of_Z     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.offset_Z));
            sp_tp_brk_sp_X     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.split_X));
            sp_tp_brk_sp_Y     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.split_Y));
            sp_tp_brk_sp_Z     = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.split_Z));
            sp_tp_brk_sp_prob  = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.split_probability));
            sp_tp_brk_sp_offs  = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.split_offset));
            sp_tp_brk_sp_rot   = serializedObject.FindProperty(nameof(shatter.bricks) + "." + nameof(shatter.bricks.split_rotation));
            sp_tp_vxl_sz       = serializedObject.FindProperty(nameof(shatter.voxels) + "." + nameof(shatter.voxels.size));
            sp_tp_tet_dn       = serializedObject.FindProperty(nameof(shatter.tets) + "." + nameof(shatter.tets.density));
            sp_tp_tet_ns       = serializedObject.FindProperty(nameof(shatter.tets) + "." + nameof(shatter.tets.noise));
            
            // Find Material Serialized properties
            sp_mat_scl    = serializedObject.FindProperty(nameof(shatter.material) + "." + nameof(shatter.material.mScl));
            sp_mat_in     = serializedObject.FindProperty(nameof(shatter.material) + "." + nameof(shatter.material.iMat));
            sp_mat_col_en = serializedObject.FindProperty(nameof(shatter.material) + "." + nameof(shatter.material.cE));
            sp_mat_uve_en = serializedObject.FindProperty(nameof(shatter.material) + "." + nameof(shatter.material.uvE));
            sp_mat_col    = serializedObject.FindProperty(nameof(shatter.material) + "." + nameof(shatter.material.cC));
            sp_mat_uve    = serializedObject.FindProperty(nameof(shatter.material) + "." + nameof(shatter.material.uvC));
            sp_mat_uvr    = serializedObject.FindProperty(nameof(shatter.material) + "." + nameof(shatter.material.uvR));
            
            // Clusters Serialized properties
            sp_cls_en     = serializedObject.FindProperty(nameof(shatter.clusters) + "." + nameof(shatter.clusters.enable));
            sp_cls_cnt    = serializedObject.FindProperty(nameof(shatter.clusters) + "." + nameof(shatter.clusters.count));
            sp_cls_seed   = serializedObject.FindProperty(nameof(shatter.clusters) + "." + nameof(shatter.clusters.seed));
            sp_cls_rel    = serializedObject.FindProperty(nameof(shatter.clusters) + "." + nameof(shatter.clusters.relax));
            sp_cls_amount = serializedObject.FindProperty(nameof(shatter.clusters) + "." + nameof(shatter.clusters.amount));
            sp_cls_layers = serializedObject.FindProperty(nameof(shatter.clusters) + "." + nameof(shatter.clusters.layers));
            sp_cls_scale  = serializedObject.FindProperty(nameof(shatter.clusters) + "." + nameof(shatter.clusters.scale));
            sp_cls_min    = serializedObject.FindProperty(nameof(shatter.clusters) + "." + nameof(shatter.clusters.min));
            sp_cls_max    = serializedObject.FindProperty(nameof(shatter.clusters) + "." + nameof(shatter.clusters.max));
            
            // Advanced Serialized properties
            sp_mode         = serializedObject.FindProperty(nameof(shatter.mode));
            sp_adv_seed     = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.seed));
            sp_adv_copy     = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.copy));
            sp_adv_smooth   = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.smooth));
            sp_adv_children = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.children));
            sp_adv_col      = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.coll));
            sp_adv_dec      = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.dec));
            sp_adv_input    = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.inpCap));
            sp_adv_output   = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.outCap));
            sp_adv_size_lim = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.szeLim));
            sp_adv_size_am  = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.szeAm));
            sp_adv_vert_lim = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.vrtLim));
            sp_adv_vert_am  = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.vrtAm));
            sp_adv_tri_lim  = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.triLim));
            sp_adv_tri_am   = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.triAm));
            sp_adv_inner    = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.inner));
            sp_adv_planar   = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.planar));
            sp_adv_rel      = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.relSze));
            sp_adv_abs      = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.absSze));
            sp_adv_element  = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.element));
            sp_adv_remove   = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.dbl));
            
            sp_adv_hierarchy = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.hierarchy));
            sp_adv_separate  = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.separate));
            sp_adv_combine   = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.combine));
            sp_adv_slc       = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.sliceType));
            sp_adv_scl       = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.origScale));
            sp_aabb_en       = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.aabbEnable));
            sp_aabb_sep      = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.aabbSeparate));
            sp_aabb_obj      = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.aabbObject));
            sp_cn_set        = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.centerSet));
            sp_cn_obj        = serializedObject.FindProperty(nameof(shatter.advanced) + "." + nameof(shatter.advanced.centerBias));
            
            sp_shl_en    = serializedObject.FindProperty(nameof(shatter.shell) + "." + nameof(shatter.shell.enable));
            sp_shl_fr    = serializedObject.FindProperty(nameof(shatter.shell) + "." + nameof(shatter.shell.first));
            sp_shl_br    = serializedObject.FindProperty(nameof(shatter.shell) + "." + nameof(shatter.shell.bridge));
            sp_shl_sub   = serializedObject.FindProperty(nameof(shatter.shell) + "." + nameof(shatter.shell.submesh));
            sp_shl_thick = serializedObject.FindProperty(nameof(shatter.shell) + "." + nameof(shatter.shell.thickness));
            
            sp_cn_pos  = serializedObject.FindProperty(nameof(shatter.centerPosition));
            sp_cn_sh   = serializedObject.FindProperty(nameof(shatter.showCenter));
            
            // Reorderable lists
            rl_tp_cus_tms = new ReorderableList (serializedObject, sp_tp_cus_tms, true, true, true, true)
            {
                drawElementCallback = DrawCustTmListItems,
                drawHeaderCallback  = DrawCustTmHeader,
                onAddCallback       = AddCustTm,
                onRemoveCallback    = RemoveCustTm
            };
            rl_tp_cus_list = new ReorderableList (serializedObject, sp_tp_cus_list, true, true, true, true)
            {
                drawElementCallback = DrawCustPointListItems,
                drawHeaderCallback  = DrawCustPointHeader,
                onAddCallback       = AddCustPoint,
                onRemoveCallback    = RemoveCustPoint
            };
            rl_tp_slc_list = new ReorderableList (serializedObject, sp_tp_slc_list, true, true, true, true)
            {
                drawElementCallback = DrawSliceTmListItems,
                drawHeaderCallback  = DrawSliceTmHeader,
                onAddCallback       = AddSliceTm,
                onRemoveCallback    = RemoveSliceTm
            };
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            if (shatter == null)
                return;

            // Update changed properties
            serializedObject.Update();

            // Set style
            RFUI.SetToggleButtonStyle();
            
            // Space
            GUILayout.Space (8);
            
            GUI_Fragment();
            GUI_Interactive();
            GUI_Batches();
            GUI_Preview();

            // Reset scale if fragments were deleted
            ResetScale (shatter, shatter.previewScale);

            RFUI.Space ();
            GUI_Types();
            RFUI.Space ();
            GUI_Material();
            RFUI.Space ();
            GUI_Advanced();
            RFUI.Space ();
            GUI_ClusterV1();
            GUI_ClusterV2();
            RFUI.Space ();
            GUI_Center();
            RFUI.Space ();
            GUI_Info();
            
            // Support warning
            if (shatter.engine == RayfireShatter.RFEngineType.V2Beta)
                RFUI.HelpBox ("WIP. V2 engine fragmentation supported only in Editor mode on Windows and Mac platforms for now. " +
                                   "Support for other platforms will be added later. ", MessageType.Info, true);

            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Buttons
        /// /////////////////////////////////////////////////////////
        
        void GUI_Fragment()
        {
            if (GUILayout.Button (TextSht.gui_btn_frag, RFUI.buttonStyle, GUILayout.Height (25)))
            {
                foreach (var targ in targets)
                {
                    if (targ as RayfireShatter != null)
                    {
                        if ((targ as RayfireShatter).interactive == false)
                            (targ as RayfireShatter).Fragment();
                        else
                            (targ as RayfireShatter).InteractiveFragment();

                        // TODO APPLY LOCAL SHATTER PREVIEW PROPS TO ALL SELECTED
                    }
                }
                // Scale preview if preview turn on
                if (shatter.previewScale > 0 && shatter.scalePreview == true)
                    ScalePreview (shatter);
            }
            
            GUILayout.Space (1);
        }
        
        void GUI_Interactive()
        {
            // TODO TEMP
            if (shatter.engine == RayfireShatter.RFEngineType.V2Beta) return;

            EditorGUI.BeginChangeCheck();

            shatter.interactive = GUILayout.Toggle (shatter.interactive, TextSht.gui_btn_inter, "Button", GUILayout.Height (25));
            if (EditorGUI.EndChangeCheck() == true)
            {
                if (shatter.interactive == true)
                    shatter.InteractiveStart();
                else
                    shatter.InteractiveStop();
                RFUI.SetDirty (shatter.gameObject);
            }
            RFUI.Space ();
        }
        
        void GUI_Batches()
        {
            if (shatter.batches.Count == 0)
                return;
            
            CleanBatches();
            
            RFUI.Space();
            
            for (int i = 0; i < shatter.batches.Count; i++)
            {
                if (shatter.batches.Count == 0)
                    return;
                
                GUILayout.BeginHorizontal ();
                
                /*
                if (GUILayout.Button ("Load", GUILayout.Height (18), GUILayout.Width (45)))
                {
                    // TODO set shatter properties
                }
                */
                
                if (GUILayout.Button ("Delete", RFUI.buttonStyle, GUILayout.Height (18), GUILayout.Width (60)))
                {
                    DestroyImmediate (shatter.batches[i].fragRoot.gameObject);
                    shatter.gameObject.SetActive (true);
                    GUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.ObjectField (shatter.batches[i].fragRoot.gameObject, typeof(GameObject), false);
                
                //shatter.batches[i].preview = EditorGUILayout.Toggle (shatter.batches[i].preview, GUILayout.Width (15));
                
                if (GUILayout.Button ("Load", RFUI.buttonStyle, GUILayout.Height (18), GUILayout.Width (50)))
                {
                    shatter.batches[i].LoadData (shatter);
                }
                
                if (shatter.batches[i].exported == false)
                {
                    if (GUILayout.Button ("Export", RFUI.buttonStyle, GUILayout.Height (18), GUILayout.Width (60)))
                    {
                        if (RFMeshAsset.ExportBatch (shatter, shatter.batches[i], null) == true)
                            shatter.batches[i].exported = true;
                    }
                }

                if (GUILayout.Button ("-", RFUI.buttonStyle, GUILayout.Height (18), GUILayout.Width (18)))
                {
                    shatter.batches[i].fragRoot = null;
                    GUILayout.EndHorizontal();
                    break;
                }
                
                GUILayout.EndHorizontal();
                GUILayout.Space (1);
            }
        }

        void CleanBatches()
        {
            for (int i = shatter.batches.Count - 1; i >= 0; i--)
                if (shatter.batches[i].fragRoot == null)
                    shatter.batches.RemoveAt (i);
        }
        
        void GUI_Preview()
        {
            // Preview
            if (shatter.batches.Count == 0 && shatter.interactive == false)
                return;

            RFUI.CaptionBox (TextSht.gui_cap_prv);

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            shatter.scalePreview = GUILayout.Toggle (shatter.scalePreview, TextSht.gui_btn_scale, "Button");
            if (EditorGUI.EndChangeCheck() == true)
            {
                if (shatter.scalePreview == true)
                    ScalePreview (shatter);
                else
                {
                    shatter.resetState = true;
                    ResetScale (shatter, 0f);
                }

                RFUI.SetDirty (shatter.gameObject);
                InteractiveChange();
            }
            
            // Color preview toggle
            EditorGUI.BeginChangeCheck();
            shatter.colorPreview = GUILayout.Toggle (shatter.colorPreview, TextSht.gui_btn_color, "Button");
            if (EditorGUI.EndChangeCheck() == true)
            {
                RFUI.SetDirty (shatter.gameObject);
                SceneView.RepaintAll();
            }
            
            EditorGUILayout.EndHorizontal();
            
            RFUI.Space ();
            RFUI.Space ();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label (TextSht.gui_prv_scl, GUILayout.Width (90));
            EditorGUI.BeginChangeCheck();
            shatter.previewScale = GUILayout.HorizontalSlider (shatter.previewScale, 0f, 0.99f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                if (shatter.scalePreview == true)
                    ScalePreview (shatter);
                RFUI.SetDirty (shatter.gameObject);
                InteractiveChange(); // TODO only change scale, do not refrag. LIB update
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Types
        /// /////////////////////////////////////////////////////////
        
        void GUI_Types()
        {
            EditorGUI.BeginChangeCheck();
            RFUI.CaptionBox (TextSht.gui_cap_frg);
            RFUI.PropertyField (sp_tp, TextSht.gui_tp);
            EditorGUI.indentLevel++;
            if (shatter.type == FragType.Voronoi)        GUI_Type_Voronoi();
            else if (shatter.type == FragType.Splinters) GUI_Type_Splinters();
            else if (shatter.type == FragType.Slabs)     GUI_Type_Slabs();
            else if (shatter.type == FragType.Radial)    GUI_Type_Radial();
            else if (shatter.type == FragType.Hexagon)   GUI_Type_HexGrid();
            else if (shatter.type == FragType.Custom)    GUI_Type_Custom();
            else if (shatter.type == FragType.Slices)    GUI_Type_Slices();
            else if (shatter.type == FragType.Bricks)    GUI_Type_Bricks();
            else if (shatter.type == FragType.Voxels)    GUI_Type_Voxels();
            else if (shatter.type == FragType.Tets)      GUI_Type_Tets();
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck() == true)
                InteractiveChange();
        }
        
        void GUI_Type_Voronoi()
        {
            RFUI.Caption (TextSht.gui_cap_vor);
            RFUI.PropertyField (sp_tp_vor_amount, TextSht.gui_tp_vor_amount);
            RFUI.Slider (sp_tp_vor_bias, bias_min, bias_max, TextSht.gui_tp_vor_bias);
        }
        
        void GUI_Type_Splinters()
        {
            RFUI.Caption (TextSht.gui_cap_spl);
            RFUI.PropertyField (sp_tp_spl_axis, TextSht.gui_tp_spl_axis);
            RFUI.PropertyField (sp_tp_spl_amount, TextSht.gui_tp_vor_amount);
            RFUI.Slider (sp_tp_spl_str,  strength_min, strength_max, TextSht.gui_tp_spl_str);
            RFUI.Slider (sp_tp_spl_bias, bias_min,     bias_max, TextSht.gui_tp_vor_bias);
        }
        
        void GUI_Type_Slabs()
        {
            RFUI.Caption (TextSht.gui_cap_slb);
            RFUI.PropertyField (sp_tp_slb_axis,   TextSht.gui_tp_spl_axis);
            RFUI.PropertyField (sp_tp_slb_amount, TextSht.gui_tp_vor_amount);
            RFUI.Slider (sp_tp_slb_str,  strength_min, strength_max, TextSht.gui_tp_spl_str);
            RFUI.Slider (sp_tp_slb_bias, bias_min,     bias_max, TextSht.gui_tp_vor_bias);
        }
        
        void GUI_Type_Radial()
        {
            RFUI.Caption (TextSht.gui_cap_rad);
            RFUI.PropertyField (sp_tp_rad_axis, TextSht.gui_tp_rad_axis);
            RFUI.Slider (sp_tp_rad_radius, rad_radius_min, rad_radius_max, TextSht.gui_tp_rad_radius);
            RFUI.Slider (sp_tp_rad_div,    rad_div_min,    rad_div_max,    TextSht.gui_tp_rad_div);
            if (sp_tp_rad_div.floatValue > 0)
                RFUI.PropertyField (sp_tp_rad_rest, TextSht.gui_tp_rad_rest);

            RFUI.Caption (TextSht.gui_cap_rings);
            RFUI.IntSlider (sp_tp_rad_rings,    rad_rings_min, rad_rings_max, TextSht.gui_tp_rad_rings);
            RFUI.IntSlider (sp_tp_rad_randRing, rad_rand_min,  rad_rand_max,  TextSht.gui_tp_rad_randRing);
            RFUI.IntSlider (sp_tp_rad_focus,    rad_focus_min, rad_focus_max, TextSht.gui_tp_rad_focus);
            RFUI.IntSlider (sp_tp_rad_str,      rad_focus_min, rad_focus_max, TextSht.gui_tp_rad_str);
            
            RFUI.Caption (TextSht.gui_cap_rays);
            RFUI.IntSlider (sp_tp_rad_rays,    rad_rings_min, rad_rings_max, TextSht.gui_tp_rad_rays);
            RFUI.IntSlider (sp_tp_rad_randRay, rad_rand_min,  rad_rand_max,  TextSht.gui_tp_rad_randRay);
            RFUI.IntSlider (sp_tp_rad_twist,   rad_twist_min, rad_twist_max, TextSht.gui_tp_rad_twist);
        }

        void GUI_Type_HexGrid()
        {
            RFUI.Caption (TextSht.gui_cap_hex);
            RFUI.Slider (sp_tp_hex_size, hex_size_min, hex_size_max, TextSht.gui_tp_hex_size);
            RFUI.PropertyField (sp_tp_hex_en, TextSht.gui_tp_cus_en);

            RFUI.Caption (TextSht.gui_cap_hex_grd);
            RFUI.PropertyField (sp_tp_hex_pl, TextSht.gui_tp_slc_pl);
            RFUI.IntSlider (sp_tp_hex_row, hex_row_min, hex_row_max, TextSht.gui_tp_hex_am);
            RFUI.IntSlider (sp_tp_hex_col, hex_row_min, hex_row_max, TextSht.gui_tp_hex_emp);
            RFUI.Slider (sp_tp_hex_div, 0f, shatter.hexagon.size, TextSht.gui_tp_rad_div);
            if (sp_tp_hex_div.floatValue > 0)
                RFUI.PropertyField (sp_tp_hex_rest, TextSht.gui_tp_rad_rest);
        }
        
        void GUI_Type_Custom()
        {
            RFUI.Caption (TextSht.gui_cap_cus);
            RFUI.PropertyField (sp_tp_cus_src, TextSht.gui_tp_cus_src);
            RFUI.PropertyField (sp_tp_cus_use, TextSht.gui_tp_cus_use);
            if (sp_tp_cus_src.intValue == (int)RFCustom.RFPointCloudSourceType.TransformList)
                rl_tp_cus_tms.DoLayoutList();
            if (sp_tp_cus_src.intValue == (int)RFCustom.RFPointCloudSourceType.Vector3List)
                rl_tp_cus_list.DoLayoutList();
            if (sp_tp_cus_use.intValue == (int)RFCustom.RFPointCloudUseType.VolumePoints)
            {
                RFUI.Caption (TextSht.gui_cap_vol);
                RFUI.IntSlider (sp_tp_cus_am,  cus_amount_min, cus_amount_max, TextSht.gui_tp_cus_am);
                RFUI.Slider (sp_tp_cus_rad, cus_radius_min, cus_radius_max, TextSht.gui_tp_cus_rad);
                
                if (shatter.custom.inBoundPoints.Count > 0)
                {
                    RFUI.Space ();
                    GUILayout.Label (TextSht.str_points + shatter.custom.inBoundPoints.Count + "/" + shatter.custom.outBoundPoints.Count);
                }
            }

            RFUI.Caption (TextSht.gui_cap_prev);
            RFUI.PropertyField (sp_tp_cus_en, TextSht.gui_tp_cus_en);
            if (sp_tp_cus_en.boolValue == true)
                RFUI.Slider (sp_tp_cus_sz, cus_size_min, cus_size_max, TextSht.gui_tp_cus_sz);
        }
        
        void GUI_Type_Slices()
        {
            RFUI.Caption (TextSht.gui_cap_slc);
            RFUI.PropertyField (sp_tp_slc_pl, TextSht.gui_tp_slc_pl);
            rl_tp_slc_list.DoLayoutList();
        }

        void GUI_Type_Bricks()
        {
            RFUI.Caption (TextSht.gui_cap_brk);
            RFUI.PropertyField (sp_tp_brk_type, TextSht.gui_tp_brk_type);
            RFUI.Slider (sp_tp_brk_mult, brick_mult_min, brick_mult_max, TextSht.gui_tp_brk_mult);
            if (sp_tp_brk_type.intValue == (int)RFBricks.RFBrickType.ByAmount)
            //if (shat.bricks.amountType == RFBricks.RFBrickType.ByAmount)
            {
                RFUI.Caption (TextSht.gui_cap_am);
                RFUI.IntSlider (sp_tp_brk_am_X, brick_amount_min, brick_amount_max, TextSht.gui_tp_brk_am_X);
                RFUI.IntSlider (sp_tp_brk_am_Y, brick_amount_min, brick_amount_max, TextSht.gui_tp_brk_am_Y);
                RFUI.IntSlider (sp_tp_brk_am_Z, brick_amount_min, brick_amount_max, TextSht.gui_tp_brk_am_Z);
            }
            else
            {
                RFUI.Caption (TextSht.gui_cap_size);
                
                EditorGUI.BeginChangeCheck();
                RFUI.Slider (sp_tp_brk_sz_X, brick_size_min, brick_size_max, TextSht.gui_tp_brk_am_X);
                if (EditorGUI.EndChangeCheck() == true && sp_tp_brk_lock.boolValue == true)
                {
                    sp_tp_brk_sz_Y.floatValue = sp_tp_brk_sz_X.floatValue;
                    sp_tp_brk_sz_Z.floatValue = sp_tp_brk_sz_X.floatValue;
                }

                EditorGUI.BeginChangeCheck();
                RFUI.Slider (sp_tp_brk_sz_Y, brick_size_min, brick_size_max, TextSht.gui_tp_brk_am_Y);
                if (EditorGUI.EndChangeCheck() == true && sp_tp_brk_lock.boolValue == true)
                {
                    sp_tp_brk_sz_X.floatValue = sp_tp_brk_sz_Y.floatValue;
                    sp_tp_brk_sz_Z.floatValue = sp_tp_brk_sz_Y.floatValue;
                }

                EditorGUI.BeginChangeCheck();
                RFUI.Slider (sp_tp_brk_sz_Z, brick_size_min, brick_size_max, TextSht.gui_tp_brk_am_Z);
                if (EditorGUI.EndChangeCheck() == true && sp_tp_brk_lock.boolValue == true)
                {
                    sp_tp_brk_sz_X.floatValue = sp_tp_brk_sz_Z.floatValue;
                    sp_tp_brk_sz_Y.floatValue = sp_tp_brk_sz_Z.floatValue;
                }
                
                RFUI.PropertyField (sp_tp_brk_lock, TextSht.gui_tp_brk_lock);
            }

            RFUI.Caption (TextSht.gui_cap_var);
            RFUI.IntSlider (sp_tp_brk_sz_var_X, brick_var_min, brick_var_max, TextSht.gui_tp_brk_am_X);
            RFUI.IntSlider (sp_tp_brk_sz_var_Y, brick_var_min, brick_var_max, TextSht.gui_tp_brk_am_Y);
            RFUI.IntSlider (sp_tp_brk_sz_var_Z, brick_var_min, brick_var_max, TextSht.gui_tp_brk_am_Z);
            
            RFUI.Caption (TextSht.gui_cap_ofs);
            RFUI.Slider (sp_tp_brk_of_X, brick_offset_min, brick_offset_max, TextSht.gui_tp_brk_am_X);
            RFUI.Slider (sp_tp_brk_of_Y, brick_offset_min, brick_offset_max, TextSht.gui_tp_brk_am_Y);
            RFUI.Slider (sp_tp_brk_of_Z, brick_offset_min, brick_offset_max, TextSht.gui_tp_brk_am_Z);
            
            RFUI.Caption (TextSht.gui_cap_sp);
            RFUI.PropertyField (sp_tp_brk_sp_X, TextSht.gui_tp_brk_am_X);
            RFUI.PropertyField (sp_tp_brk_sp_Y, TextSht.gui_tp_brk_am_Y);
            RFUI.PropertyField (sp_tp_brk_sp_Z, TextSht.gui_tp_brk_am_Z);
            RFUI.IntSlider (sp_tp_brk_sp_prob, brick_prob_min,     brick_prob_max,     TextSht.gui_tp_brk_sp_prob);
            RFUI.IntSlider (sp_tp_brk_sp_rot,  brick_rotation_min, brick_rotation_max, TextSht.gui_tp_brk_sp_rot);
            RFUI.Slider (sp_tp_brk_sp_offs, brick_split_offset_min, brick_split_offset_max, TextSht.gui_tp_brk_sp_offs);
        }

        void GUI_Type_Voxels()
        {
            RFUI.Caption (TextSht.gui_cap_vxl);
            RFUI.Slider (sp_tp_vxl_sz, vox_size_min, vox_size_max, TextSht.gui_tp_cus_sz);
        }

        void GUI_Type_Tets()
        {
            RFUI.Caption (TextSht.gui_cap_tet);
            RFUI.IntSlider (sp_tp_tet_dn, tet_dens_min,  tet_dens_max, TextSht.gui_tp_tetDn);
            RFUI.IntSlider (sp_tp_tet_ns, tet_noise_min, tet_noise_max, TextSht.gui_tp_tetNs);
        }

        void GUI_Material()
        {
            if (sp_tp.intValue == (int)FragType.Decompose)
                return;

            RFUI.CaptionBox (TextSht.gui_cap_mat);
            RFUI.Slider (sp_mat_scl, mat_scale_min, mat_scale_max, TextSht.gui_mat_scl);
            RFUI.PropertyField (sp_mat_in,  TextSht.gui_mat_in);
            RFUI.PropertyField (sp_mat_col_en, TextSht.gui_mat_col);
            if (sp_mat_col_en.boolValue == true)
            {
                EditorGUILayout.PropertyField (sp_mat_col, TextSht.gui_tp_hex_emp);
                RFUI.Space ();
            }
            RFUI.PropertyField (sp_mat_uve_en, TextSht.gui_mat_uve);
            if (sp_mat_uve_en.boolValue == true)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField (sp_mat_uve, TextSht.gui_tp_hex_emp);
                if (EditorGUI.EndChangeCheck())
                    if (uvwEditor != null)
                        uvwEditor.Repaint();
                
                RFUI.Space ();
                if (shatter.engine == RayfireShatter.RFEngineType.V2Beta)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField (sp_mat_uvr, TextSht.gui_tp_hex_emp);
                    if (EditorGUI.EndChangeCheck())
                        if (uvwEditor != null)
                            uvwEditor.Repaint();
                    
                    RFUI.Space ();
                }
                if (shatter.engine == RayfireShatter.RFEngineType.V2Beta)
                    if (GUILayout.Button (TextSht.gui_btn_uvedt, RFUI.buttonStyle, GUILayout.Height (25)))
                        OpenUvEditor();
            }
        }
        
        void OpenUvEditor()
        {
            // Get inner material
            Material mat = shatter.material.iMat;
            
            // Use object material if inner material not defined
            if (mat == null)
            {
                Renderer rnd = shatter.gameObject.GetComponent<Renderer>();
                if (rnd != null)
                    mat = rnd.sharedMaterial;
            }
            
            // Get material texture
            if (mat != null)
                uvTexture = (Texture2D)mat.mainTexture;
            
            // Open editor
            uvwEditor = UvRegionEditor.ShowWindow (uvTexture, shatter);
        }

        void GUI_ClusterV1()
        {
            // NOT SUPPORTED YET 
            if (sp_engine.intValue == 1)
                return;
            
            // Not for bricks, slices and decompose
            if (shatter.type == FragType.Bricks || 
                shatter.type == FragType.Decompose || 
                shatter.type == FragType.Voxels || 
                shatter.type == FragType.Slices)
                return;

            RFUI.CaptionBox (TextSht.gui_cap_cls);
            RFUI.PropertyField (sp_cls_en, TextSht.gui_cls_en);
            if (sp_cls_en.boolValue == true)
            {
                RFUI.IntSlider (sp_cls_cnt,  cls_count_min, cls_count_max, TextSht.gui_cls_cnt);
                RFUI.IntSlider (sp_cls_seed, cls_seed_min,  cls_seed_max,  TextSht.gui_cls_seed);
                RFUI.Slider (sp_cls_rel, cls_relax_min, cls_relax_max, TextSht.gui_cls_rel);
                RFUI.Foldout (ref exp_deb, TextSht.gui_cls_debris.text);
                if (exp_deb == true)
                {
                    EditorGUI.indentLevel++;
                    RFUI.IntSlider (sp_cls_amount, cls_amount_min, cls_amount_max, TextSht.gui_cls_amount);
                    RFUI.IntSlider (sp_cls_layers, cls_layers_min, cls_layers_max, TextSht.gui_cls_layers);
                    RFUI.Slider (sp_cls_scale, cls_scale_min, cls_scale_max, TextSht.gui_cls_scale);
                    RFUI.IntSlider (sp_cls_min, cls_frags_min, cls_frags_max, TextSht.gui_cls_min);
                    RFUI.IntSlider (sp_cls_max, cls_frags_min, cls_frags_max, TextSht.gui_cls_max);
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        void GUI_ClusterV2()
        {
            if (sp_engine.intValue == 0)
                return;
            
            // Not for bricks, slices and decompose
            if (shatter.type == FragType.Decompose ||
                shatter.type == FragType.Slices)
                return;

            RFUI.CaptionBox (TextSht.gui_cap_cls);
            RFUI.PropertyField (sp_cls_en, TextSht.gui_cls_en);
            if (sp_cls_en.boolValue == true)
            {
                RFUI.IntSlider (sp_cls_cnt,    cls_count_min,  cls_count_max,  TextSht.gui_cls_cnt);
                RFUI.IntSlider (sp_cls_layers, cls_layers_min, cls_layers_max, TextSht.gui_cls_debris);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void GUI_Advanced()
        {
            if (sp_engine.intValue == 0)
                GUI_AdvancedV1();
            else
            {
                GUI_AdvancedV2();
                GUI_CenterV2();
                GUI_Shell();
                GUI_Part();
            }
        }
        
        void GUI_AdvancedV1()
        {
            RFUI.CaptionBox (TextSht.gui_cap_prp);
            RFUI.PropertyField (sp_engine, TextSht.gui_engine);
            RFUI.PropertyField (sp_mode,   TextSht.gui_mode);
            RFUI.IntSlider (sp_adv_seed, cls_seed_min, cls_seed_max, TextSht.gui_adv_seed);
            RFUI.PropertyField (sp_adv_copy,     TextSht.gui_adv_copy);
            RFUI.PropertyField (sp_adv_smooth,   TextSht.gui_adv_smooth);
            RFUI.PropertyField (sp_adv_children, TextSht.gui_adv_children);
            RFUI.PropertyField (sp_adv_col,      TextSht.gui_adv_col);
            RFUI.PropertyField (sp_adv_dec,      TextSht.gui_adv_dec);
            RFUI.PropertyField (sp_adv_input,    TextSht.gui_adv_input);
            if (sp_adv_input.boolValue == true)
                RFUI.PropertyField (sp_adv_output, TextSht.gui_adv_output);
            
            GUI_Limitation();
            GUI_Filters();
            
            if (sp_mode.intValue == (int)FragmentMode.Editor)
            {
                RFUI.CaptionBox (TextSht.gui_cap_edt);
                RFUI.IntSlider (sp_adv_element, adv_element_min, adv_element_max, TextSht.gui_adv_element);
                RFUI.PropertyField (sp_adv_remove, TextSht.gui_adv_remove);
            }
        }
        
        void GUI_Limitation()
        {
            RFUI.Foldout (ref exp_lim, TextSht.gui_adv_lim.text);
            if (exp_lim == true)
            {
                EditorGUI.indentLevel++;
                RFUI.PropertyField (sp_adv_size_lim, TextSht.gui_adv_size_lim);
                if (shatter.advanced.szeLim == true)
                    RFUI.Slider (sp_adv_size_am, adv_size_min, adv_size_max, TextSht.gui_adv_size_am);
                RFUI.PropertyField (sp_adv_vert_lim, TextSht.gui_adv_vert_lim);
                if (shatter.advanced.vrtLim == true)
                    RFUI.IntSlider (sp_adv_vert_am, adv_vert_min, adv_vert_max, TextSht.gui_adv_vert_am);
                RFUI.PropertyField (sp_adv_tri_lim, TextSht.gui_adv_tri_lim);
                if (shatter.advanced.triLim == true)
                    RFUI.IntSlider (sp_adv_tri_am, adv_tris_min, adv_tris_max, TextSht.gui_adv_tri_am);
                EditorGUI.indentLevel--;
            }
        }

        void GUI_Filters()
        {
            RFUI.Foldout (ref exp_fil, TextSht.gui_adv_flt.text);
            if (exp_fil == true)
            {
                EditorGUI.indentLevel++;
                RFUI.PropertyField (sp_adv_inner,  TextSht.gui_adv_inner);
                RFUI.PropertyField (sp_adv_planar, TextSht.gui_adv_planar);
                if (sp_engine.intValue == 0)
                {
                    RFUI.IntSlider (sp_adv_rel, adv_rel_min, adv_rel_max, TextSht.gui_adv_rel);
                    RFUI.Slider (sp_adv_abs, adv_abs_min, adv_abs_max, TextSht.gui_adv_abs);
                }
                EditorGUI.indentLevel--;
            }
        }
        
        void GUI_AdvancedV2()
        {
            RFUI.CaptionBox (TextSht.gui_cap_prp);
            RFUI.PropertyField (sp_engine,        TextSht.gui_engine);
            RFUI.PropertyField (sp_adv_hierarchy, TextSht.gui_adv_hierarchy);
            RFUI.PropertyField (sp_adv_slc,       TextSht.gui_adv_slice);
            RFUI.IntSlider (sp_adv_seed,    cls_seed_min,    cls_seed_max,    TextSht.gui_adv_seed);
            RFUI.PropertyField (sp_adv_input,    TextSht.gui_adv_input);
            RFUI.PropertyField (sp_adv_separate, TextSht.gui_adv_separate);
            if (sp_adv_separate.boolValue == true)
            {
                RFUI.IntSlider (sp_adv_element, adv_element_min, adv_element_max, TextSht.gui_adv_element);
                RFUI.PropertyField (sp_adv_combine, TextSht.gui_adv_combine);
            }
            RFUI.PropertyField (sp_adv_scl,    TextSht.gui_adv_scl);
            RFUI.PropertyField (sp_adv_smooth, TextSht.gui_adv_smooth);
            GUI_Filters();
        }

        void GUI_Shell()
        {
            RFUI.CaptionBox (TextSht.gui_cap_shl);
            RFUI.PropertyField (sp_shl_en,  TextSht.gui_shl_en);
            if (sp_shl_en.boolValue == true)
            {
                // GUICommon.PropertyField (sp_shl_fr,  TextSht.gui_shl_fr);
                RFUI.PropertyField (sp_shl_br,  TextSht.gui_shl_br);
                RFUI.PropertyField (sp_shl_sub, TextSht.gui_shl_sub);
                RFUI.Slider (sp_shl_thick, shl_thick_min, shl_thick_max, TextSht.gui_shl_thick);
            }
        }

        void GUI_Part()
        {
            RFUI.CaptionBox (TextSht.gui_aabb_cap);
            RFUI.PropertyField (sp_aabb_en, TextSht.gui_aabb_en);
            if (sp_aabb_en.boolValue == true)
            {
                RFUI.PropertyField (sp_aabb_sep, TextSht.gui_aabb_sep);
                RFUI.PropertyField (sp_aabb_obj, TextSht.gui_aabb_obj);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Center
        /// /////////////////////////////////////////////////////////
        
        void GUI_Center()
        {
            if (shatter.engine == RayfireShatter.RFEngineType.V1)
                GUI_CenterV1();
        }
        
        void GUI_CenterV1()
        {
            if ((int)shatter.type <= 5)
            {
                RFUI.CaptionBox (TextSht.gui_cap_cent);
                GUILayout.BeginHorizontal();
                
                EditorGUI.BeginChangeCheck();
                RFUI.Toggle (sp_cn_sh, TextSht.gui_btn_show, RFUI.color_btn_blue, 18);
                if (EditorGUI.EndChangeCheck())
                    SceneView.RepaintAll();
                
                if (GUILayout.Button (TextSht.gui_cn_res, RFUI.buttonStyle))
                {
                    foreach (var targ in targets)
                    {
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).ResetCenter();
                            RFUI.SetDirty ((targ as RayfireShatter).gameObject);
                        }
                    }
                    InteractiveChange();
                    SceneView.RepaintAll();
                }
                
                EditorGUILayout.EndHorizontal();
                RFUI.Space ();
                RFUI.Space ();
                RFUI.PropertyField (sp_cn_pos, TextSht.gui_cn_pos);
            }
        }
        
        void GUI_CenterV2()
        {
            RFUI.CaptionBox (TextSht.gui_cap_cent);
            RFUI.PropertyField (sp_cn_set, TextSht.gui_cn_set);
            if (sp_cn_set.boolValue == true)
                RFUI.PropertyField (sp_cn_obj, TextSht.gui_cn_obj);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Info
        /// /////////////////////////////////////////////////////////
        
        void GUI_Info()
        {
            if (shatter.batches.Count > 0)
            {
                // TODO GUICommon.CaptionBox (TextSht.gui_cap_info);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Preview
        /// /////////////////////////////////////////////////////////

        static void ColorPreview (RayfireShatter scr)
        {
            if (scr.batches.Count > 0)
            {
                Random.InitState (1);
                foreach (var batch in scr.batches)
                    ColorRoot (batch.fragRoot);
            }

            SceneView.RepaintAll();
        }

        static void ColorRoot(Transform tm)
        {
            if (tm != null)
            {
                MeshFilter[] meshFilters = tm.GetComponentsInChildren<MeshFilter>();
                foreach (var mf in meshFilters)
                {
                    Gizmos.color = new Color (Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f));
                    Gizmos.DrawMesh (mf.sharedMesh, mf.transform.position, mf.transform.rotation, mf.transform.lossyScale * 1.01f);
                }
            }
        }

        static void ScalePreview (RayfireShatter scr)
        {
            if (scr.batches.Count > 0 && scr.previewScale > 0f)
            {
                foreach (var batch in scr.batches)
                {
                    if (batch.sourceTm != null)
                        batch.sourceTm.gameObject.SetActive (false);
                    BatchTransformScale (batch.fragments, Vector3.one * scr.PreviewScale());
                }

                scr.resetState = true;
            }

            if (scr.previewScale == 0f)
                ResetScale (scr, 0f);
        }
        
        // Reset original object and fragments scale
        static void ResetScale(RayfireShatter scr, float scaleValue)
        {
            // Reset scale
            if (scr.resetState == true && scaleValue == 0f)
            {
                foreach (var batch in scr.batches)
                {
                    if (batch.sourceTm != null)
                        batch.sourceTm.gameObject.SetActive (true);
                    BatchTransformScale (batch.fragments, Vector3.one);
                }
                
                scr.resetState = false;
            }
        }
        
        static void BatchTransformScale(List<Transform> tms, Vector3 scale)
        {
            if (tms != null && tms.Count > 0)
                foreach (var tm in tms)
                    if (tm != null)
                        tm.localScale = scale;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireShatter shatter, GizmoType gizmoType)
        {
            // Color preview
            if (shatter.colorPreview == true)
                ColorPreview (shatter);

            // HexGrid cloud preview
            if (shatter.type == FragType.Hexagon && shatter.hexagon.enable == true)
            {
                // Get bounds for preview
                Bounds bound = shatter.GetBound();
                
                if (shatter.engine == RayfireShatter.RFEngineType.V1)
                {
                    if (bound.size.magnitude > 0)
                    {
                        // Center position from local to global
                        Vector3 centerPos = shatter.transform.TransformPoint (shatter.centerPosition);

                        // Collect point cloud and draw
                        RFHexagon.GetHexPointCLoudV1 (shatter.hexagon, shatter.transform, centerPos, shatter.centerDirection, shatter.advanced.seed, bound);
                        
                        DrawSpheres (shatter.hexagon.pcBndIn, shatter.hexagon.pcBndOut, shatter.hexagon.size / 4f);
                    }
                }
                
                if (shatter.engine == RayfireShatter.RFEngineType.V2Beta)
                {
                    // Collect point cloud and draw
                    RFHexagon.GetHexPointCLoudV2 (shatter.hexagon, shatter.CenterPos, shatter.CenterDir, bound);
                    
                    DrawSpheres (shatter.hexagon.pcBndIn, shatter.hexagon.pcBndOut, shatter.hexagon.size / 4f);
                }
            }

            // Custom point cloud preview
            if (shatter.type == FragType.Custom && shatter.custom.enable == true)
            {
                // Get bounds for preview
                Bounds bound = shatter.GetBound();
                if (bound.size.magnitude > 0)
                {
                    // Collect point cloud and draw
                    RFCustom.GetCustomPointCLoud (shatter.custom, shatter.transform, shatter.advanced.seed, bound);
                    DrawSpheres (shatter.custom.inBoundPoints, shatter.custom.outBoundPoints, shatter.custom.size);
                }
            }
        }

        // Draw In/Out points
        static void DrawSpheres(List<Vector3> inBoundPoints, List<Vector3> outBoundPoints, float size)
        {
            if (inBoundPoints != null && inBoundPoints.Count > 0)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < inBoundPoints.Count; i++)
                    Gizmos.DrawSphere (inBoundPoints[i], size);
            }
            if (outBoundPoints != null && outBoundPoints.Count > 0)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < outBoundPoints.Count; i++)
                    Gizmos.DrawSphere (outBoundPoints[i], size / 2f);
            }
        }

        // Show center move handle
        private void OnSceneGUI()
        {
            // Get shatter
            shatter = (RayfireShatter)target;
            if (shatter == null)
                return;
            
            // Point3 handle
            if (shatter.engine == RayfireShatter.RFEngineType.V1 && shatter.showCenter == true)
            {
                transForm       = shatter.transform;
                
                // TODO fix little change at any property change
                centerWorldPos  = transForm.TransformPoint (shatter.centerPosition);
                centerWorldQuat = transForm.rotation * shatter.centerDirection;
                
                EditorGUI.BeginChangeCheck();
                centerWorldPos = Handles.PositionHandle (centerWorldPos, centerWorldQuat.RFNormalize());
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObject (shatter, TextSht.str_move);
                    InteractiveChange();
                    RFUI.SetDirty (shatter.gameObject);
                }

                EditorGUI.BeginChangeCheck();
                centerWorldQuat = Handles.RotationHandle (centerWorldQuat, centerWorldPos);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObject (shatter, TextSht.str_rotate);
                    InteractiveChange();
                    RFUI.SetDirty (shatter.gameObject);
                }
                
                shatter.centerDirection = Quaternion.Inverse (transForm.rotation) * centerWorldQuat;
                shatter.centerPosition  = transForm.InverseTransformPoint (centerWorldPos);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList Custom Transform
        /// /////////////////////////////////////////////////////////
        
        void DrawCustTmListItems (Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_tp_cus_tms.serializedProperty.GetArrayElementAtIndex (index);
            EditorGUI.PropertyField (new Rect (rect.x, rect.y + 2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        void DrawCustTmHeader (Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField (rect, TextSht.gui_tp_cus_tms);
        }

        void AddCustTm (ReorderableList list)
        {
            if (shatter.custom.transforms == null)
                shatter.custom.transforms = new List<Transform>();
            shatter.custom.transforms.Add (null);
            list.index = list.count;
            InteractiveChange();
        }

        void RemoveCustTm (ReorderableList list)
        {
            if (shatter.custom.transforms != null)
            {
                shatter.custom.transforms.RemoveAt (list.index);
                list.index = list.index - 1;
                InteractiveChange();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// ReorderableList Custom Point 3
        /// /////////////////////////////////////////////////////////
      
        void DrawCustPointListItems (Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_tp_cus_list.serializedProperty.GetArrayElementAtIndex (index);
            EditorGUI.PropertyField (new Rect (rect.x, rect.y + 2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        void DrawCustPointHeader (Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField (rect, TextSht.gui_tp_cus_vec);
        }

        void AddCustPoint (ReorderableList list)
        {
            if (shatter.custom.vector3 == null)
                shatter.custom.vector3 = new List<Vector3>();
            shatter.custom.vector3.Add (Vector3.zero);
            list.index = list.count;
            InteractiveChange();
        }

        void RemoveCustPoint (ReorderableList list)
        {
            if (shatter.custom.vector3 != null)
            {
                shatter.custom.vector3.RemoveAt (list.index);
                list.index = list.index - 1;
                InteractiveChange();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// ReorderableList Slice Transform
        /// /////////////////////////////////////////////////////////
       
        void DrawSliceTmListItems (Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_tp_slc_list.serializedProperty.GetArrayElementAtIndex (index);
            EditorGUI.PropertyField (new Rect (rect.x, rect.y + 2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        void DrawSliceTmHeader (Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField (rect, TextSht.gui_tp_cus_tms);
        }

        void AddSliceTm (ReorderableList list)
        {
            if (shatter.slice.sliceList == null)
                shatter.slice.sliceList = new List<Transform>();
            shatter.slice.sliceList.Add (null);
            list.index = list.count;
            InteractiveChange();
        }

        void RemoveSliceTm (ReorderableList list)
        {
            if (shatter.slice.sliceList != null)
            {
                shatter.slice.sliceList.RemoveAt (list.index);
                list.index = list.index - 1;
                InteractiveChange();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Property change
        void InteractiveChange()
        {
            if (shatter != null && shatter.interactive == true)
            {
                if (shatter.engine == RayfireShatter.RFEngineType.V2Beta)
                    return;
                
                // Apply changes
                serializedObject.ApplyModifiedProperties();
                
                // Refragment
                shatter.InteractiveChange();
            }
        }
    }
    
    public class UvRegionEditor : EditorWindow
    {
        static Texture2D      texture;
        static RayfireShatter shatter;
        Vector2               min;
        Vector2               max;
        Rect                  rect;
        Vector2[]             resizeHandles;
        
        // Show window
        public static EditorWindow ShowWindow(Texture2D tex, RayfireShatter sh)
        {
            texture = tex;
            shatter = sh;
            EditorWindow win = GetWindow<UvRegionEditor>(sh.name + ": UV Region");
            if (texture != null)
            {
                win.maxSize = new Vector2 (texture.width, texture.height);
                win.minSize = new Vector2 (texture.width, texture.height) / 10;
            }
            else
            {
                win.maxSize = new Vector2 (400, 400);
                win.minSize = new Vector2 (50, 50);
            }
            return win;
        }

        private void OnEnable()
        {
            if (shatter == null)
                return;
            
            // Get coords
            min   = shatter.material.UvRegionMin;
            max   = shatter.material.UvRegionMax;
            min.y = 1f - min.y;
            max.y = 1f - max.y;
            
            rect = new Rect(min.x * maxSize.x, min.y * maxSize.y, max.x*maxSize.x, max.x*maxSize.y);
            UpdateResizeHandles();
        }

        private void OnGUI()
        {
            if (shatter == null)
                return;
            
            // Draw texture
            if (texture != null)
                GUI.DrawTexture(new Rect(0, 0, position.width, position.height), texture, ScaleMode.StretchToFill);
            
            // Get coords
            min   = shatter.material.UvRegionMin;
            max   = shatter.material.UvRegionMax;
            min.y = 1f - min.y;
            max.y = 1f - max.y;
            
            float minX = Mathf.Lerp (0, position.width,  min.x);
            float minY = Mathf.Lerp (0, position.height, min.y);
            float maxX = Mathf.Lerp (0, position.width,  max.x);
            float maxY = Mathf.Lerp (0, position.height, max.y);
            rect = new Rect(minX, minY, maxX-minX- 0.01f, maxY-minY- 0.001f);
            
            // Draw rect
            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(rect, new Color(0.1f, 0.1f, 0.1f, 0.1f), Color.green);
            Handles.EndGUI();
            
            // TODO add dotted lines
            // Handles.DrawDottedLine ();
            
            // Draw labels
            Handles.Label (new Vector3 (minX + 2, minY - 10f, 0), shatter.material.UvRegionMin.ToString());
            Handles.Label (new Vector3 (maxX - 70, maxY + 10f, 0), shatter.material.UvRegionMax.ToString());
            
            // Resize
            // HandleResize();

            // Update
            // UpdateResizeHandles();
        }

        private void HandleResize()
        {
            for (int i = 0; i < resizeHandles.Length; i++)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 pos               = new Vector3 (resizeHandles[i].x, resizeHandles[i].y, 0);
                var fmh_1520_73_638806876166915126 = Quaternion.identity; Vector3 newHandlePosition = Handles.FreeMoveHandle(pos, 10f, Vector2.zero, Handles.RectangleHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateRectBasedOnHandle(i, new Vector2 (newHandlePosition.x, newHandlePosition.y));
                }
            }
        }

        private void UpdateRectBasedOnHandle(int handleIndex, Vector2 newPosition)
        {
            switch (handleIndex)
            {
                case 0: 
                    rect.xMin = newPosition.x;
                    rect.yMin = newPosition.y;
                    break;
                case 1: 
                    rect.xMax = newPosition.x;
                    rect.yMin = newPosition.y;
                    break;
                case 2: 
                    rect.xMin = newPosition.x;
                    rect.yMax = newPosition.y;
                    break;
                case 3: 
                    rect.xMax = newPosition.x;
                    rect.yMax = newPosition.y;
                    break;
            }
            
            rect.width = Mathf.Max(rect.width, 50);
            rect.height = Mathf.Max(rect.height, 50);
        }

        private void UpdateResizeHandles()
        {
            resizeHandles = new Vector2[]
            {
                new Vector2(rect.xMin, rect.yMin), 
                new Vector2(rect.xMax, rect.yMin), 
                new Vector2(rect.xMin, rect.yMax), 
                new Vector2(rect.xMax, rect.yMax)  
            };
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
    
    // Normalize quat in order to support Unity 2018.1
    public static class RFQuaternionExtension
    {
        public static Quaternion RFNormalize (this Quaternion q)
        {
            float f = 1f / Mathf.Sqrt (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return new Quaternion (q.x * f, q.y * f, q.z * f, q.w * f);
        }
    }
}
