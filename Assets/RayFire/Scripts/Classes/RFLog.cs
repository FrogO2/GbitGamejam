
namespace RayFire
{
    /// <summary>
    /// Rayfire plugin Debug.Log messages.
    /// </summary>
    public static class RFLog
    {
        // Names
        public const string act_name = "Rayfire Activator";
        public const string bld_name = "Rayfire Blade";
        public const string bmb_name = "Rayfire Bomb";
        public const string cmb_name = "Rayfire Combine";
        public const string cnt_name = "Rayfire Connectivity";
        public const string dbr_name = "Rayfire Debris";
        public const string dst_name = "Rayfire Dust";
        public const string gun_name = "Rayfire Gun";
        public const string man_name = "Rayfire Man";
        public const string rec_name = "Rayfire Recorder";
        public const string rst_name = "Rayfire Restriction";
        public const string rig_name = "Rayfire Rigid";
        public const string rot_name = "Rayfire RigidRoot";
        public const string sht_name = "Rayfire Shatter";
        public const string shl_name = "Rayfire Shell";
        public const string snp_name = "Rayfire Snapshot";
        public const string snd_name = "Rayfire Sound";
        public const string uny_name = "Rayfire Unyielding";
        public const string vrt_name = "Rayfire Vortex";
        public const string wnd_name = "Rayfire Wind";
        
        // Debug Names
        public const string act_dbgn = "Rayfire Activator: ";
        public const string bld_dbgn = "Rayfire Blade: ";
        public const string bmb_dbgn = "Rayfire Bomb: ";
        public const string cmb_dbgn = "Rayfire Combine: ";
        public const string cnt_dbgn = "Rayfire Connectivity: ";
        public const string dbr_dbgn = "Rayfire Debris: ";
        public const string dst_dbgn = "Rayfire Dust: ";
        public const string gun_dbgn = "Rayfire Gun: ";
        public const string man_dbgn = "Rayfire Man: ";
        public const string rec_dbgn = "Rayfire Recorder: ";
        public const string rst_dbgn = "Rayfire Restriction: ";
        public const string rig_dbgn = "Rayfire Rigid: ";
        public const string rot_dbgn = "Rayfire RigidRoot: ";
        public const string sht_dbgn = "Rayfire Shatter: ";
        public const string shl_dbgn = "Rayfire Shell: ";
        public const string snp_dbgn = "Rayfire Snapshot: ";
        public const string snd_dbgn = "Rayfire Sound: ";
        public const string uny_dbgn = "Rayfire Unyielding: ";
        public const string vrt_dbgn = "Rayfire Vortex: ";
        public const string wnd_dbgn = "Rayfire Wind: ";
        
        // Paths
        public const string act_path = "RayFire/Rayfire Activator";
        public const string bld_path = "RayFire/Rayfire Blade";
        public const string bmb_path = "RayFire/Rayfire Bomb";
        public const string cmb_path = "RayFire/Rayfire Combine";
        public const string cnt_path = "RayFire/Rayfire Connectivity";
        public const string dbr_path = "RayFire/Rayfire Debris";
        public const string dst_path = "RayFire/Rayfire Dust";
        public const string gun_path = "RayFire/Rayfire Gun";
        public const string man_path = "RayFire/Rayfire Man";
        public const string rec_path = "RayFire/Rayfire Recorder";
        public const string rst_path = "RayFire/Rayfire Restriction";
        public const string rig_path = "RayFire/Rayfire Rigid";
        public const string rot_path = "RayFire/Rayfire RigidRoot";
        public const string sht_path = "RayFire/Rayfire Shatter";
        public const string shl_path = "RayFire/Rayfire Shell";
        public const string snp_path = "RayFire/Rayfire Snapshot";
        public const string snd_path = "RayFire/Rayfire Sound";
        public const string uny_path = "RayFire/Rayfire Unyielding";
        public const string vrt_path = "RayFire/Rayfire Vortex";
        public const string wnd_path = "RayFire/Rayfire Wind";
        
        // Links
        public const string act_link = "https://rayfirestudios.com/unity-online-help/components/unity-activator-component/";
        public const string bld_link = "https://rayfirestudios.com/unity-online-help/components/unity-blade-component/";
        public const string bmb_link = "https://rayfirestudios.com/unity-online-help/components/unity-bomb-component/";
        public const string cmb_link = "https://rayfirestudios.com/unity-online-help/components/unity-combine-component/";
        public const string cnt_link = "https://rayfirestudios.com/unity-online-help/components/unity-connectivity-component/";
        public const string dbr_link = "https://rayfirestudios.com/unity-online-help/components/unity-debris-component/";
        public const string dst_link = "https://rayfirestudios.com/unity-online-help/components/unity-dust-component/";
        public const string gun_link = "https://rayfirestudios.com/unity-online-help/components/unity-gun-component/";
        public const string man_link = "https://rayfirestudios.com/unity-online-help/components/unity-gun-component/";
        public const string rec_link = "https://rayfirestudios.com/unity-online-help/components/unity-recorder-component/";
        public const string rst_link = "https://rayfirestudios.com/unity-online-help/components/unity-restriction-component/";
        public const string rig_link = "https://rayfirestudios.com/unity-online-help/components/unity-rigid-component/";
        public const string rot_link = "https://rayfirestudios.com/unity-online-help/components/unity-rigid-root-component/";
        public const string sht_link = "https://rayfirestudios.com/unity-online-help/components/unity-shatter-component/";
        public const string snp_link = "https://rayfirestudios.com/unity-online-help/components/unity-snapshot-component/";
        public const string snd_link = "https://rayfirestudios.com/unity-online-help/components/unity-sound-component/";
        public const string uny_link = "https://rayfirestudios.com/unity-online-help/components/unity-unyielding-component/";
        public const string vrt_link = "https://rayfirestudios.com/unity-online-help/components/unity-vortex-component/";
        public const string wnd_link = "https://rayfirestudios.com/unity-online-help/components/unity-wind-component/";
        
        
        // Activator
        public const string act_noCol   = ". Has no activation collider.";
        public const string act_onCol   = ". Particle System Gizmo Type supports only On Collision Activation type. Set Activation Type to On Collision.";
        public const string act_noPos   = ". Position list is empty and scale is not animated.";
        public const string act_noLine  = ". Path line is not defined.";
        public const string act_savePos = ". Position can be saved only for Global and Local Position animation type.";
        public const string act_samePos = ". Activator at the same position.";

        // Combine 
        public const string cmb_noMesh = ". No meshes to combine.";
        public const string cmb_index  = ". Combined mesh has more than 65535 vertices. UInt32 mesh Index Format will be used.";
        
        // Connectivity
        public const string cnt_noObj  = ". Has no objects to check for connectivity. Connectivity disabled.";
        public const string cnt_noRoot = ". Object has Rigid host but it's object type is not Mesh Root. Connectivity disabled.";
        public const string cnt_noCon  = ". Object has Rigid host with Mesh Root type but activation By Connectivity is disabled.";
        public const string cnt_noAct  = ". Object has Rigid but simulation type is not Inactive or Kinematic. Connectivity disabled.";
        public const string cnt_conDis = ". Object has RigidRoot host but activation By Connectivity is disabled.";
        public const string cnt_noKin  = ". Object has RigidRoot but simulation type is not Inactive or Kinematic. Connectivity disabled.";
        public const string cnt_noSh   = ". Object has missing shards. Reset and Editor Setup Connectivity host again.";
        public const string cnt_fract  = ". Fracture Collider is not supported. Use Box or Sphere trigger collider.";
        
        // Debris
        public const string dbr_noRef  = ". Debris reference not defined.";
        public const string dbr_noMesh = ". Debris reference mesh is not defined.";
        public const string dbr_noVert = ". No mesh or amount of vertices too low.";
        public const string dbr_noBurs = ". Deprecated Burst Type property.";
        
        // Dust
        public const string dst_noMat  = ". Dust material not defined.";
        public const string dst_noBurs = ". Deprecated Burst Type property.";
        
        // Man
        public const string man_amount = ". Maximum fragments amount reached. Increase Maximum Amount property in Advanced Properties.";
        
        // Recorder
        public const string rec_md1    = ". Mode set to ";
        public const string rec_noChld = " but object has no children. Mode set to None.";
        public const string rec_noCont = " but controller is not defined. Mode set to None.";
        public const string rec_noClip = " but animation clip is not defined. Mode set to None.";
        public const string rec_noAnim = " but animation clip is not defined in controller. Mode set to None.";

        // Restriction
        public const string rst_noTrg = ". Target is not defined.";
        public const string rst_noCol = ". Collider is not trigger.";
        
        // Rigid
        public const string rig_noCon  = ". Object has enabled Connectivity activation but has no Connectivity component.";
        public const string rig_noSlc  = ". Has no defined slicing planes.";
        public const string rig_conRes = ". Can not reset Connectivity. Enable Connectivity Reset in Reset properties.";
        public const string rig_child1 = " has no children with mesh. Object Excluded from simulation.";
        public const string rig_act1   = " Convert property set to ";
        public const string rig_act2   = " but object has no Connectivity component. Convertation disabled.";
        public const string rig_misFrg = " object has missing fragments. Reset Setup and used Editor Setup again.";
        public const string rig_sim    = " Simulation Type set to ";
        public const string rig_noDml  = " but Demolition Type is not None. Static object can not be demolished. Demolition Type set to None.";
        public const string rig_stObj  = " but object is Static. Turn off Static checkbox in Inspector.";
        public const string rig_obj    = " Object Type set to ";
        public const string rig_noMsh  = " but object has no mesh. Object Excluded from simulation.";
        public const string rig_cnv    = " Convert property set to ";
        public const string rig_noIna  = " but Fragments Sim Type is not Inactive or Kinematik. Convertation disabled.";
        public const string rig_noRead = " Mesh is not readable. Demolition type set to None. Open Import Settings and turn On Read/Write Enabled property.";
        public const string rig_child2 = " has no children with mesh. Object Excluded from simulation.";
        public const string rig_noSkin = " but object has no SkinnedMeshRenderer. Object Excluded from simulation.";
        public const string rig_dml    = " Demolition Type is ";
        public const string rig_dml1   = " but Demolition Type is ";
        public const string rig_dml2   = " but Demolition Type is ";
        public const string rig_noShat = ". Has no Shatter component, but Use Shatter property is On. Use Shatter property was turned Off.";
        public const string rig_noCch1 = ". Demolition Type set to None. Had precached meshes which were destroyed.";
        public const string rig_noCch2 = ". Demolition Type set to Runtime. Had precached meshes which were destroyed.";
        public const string rig_frg1   = ". Demolition Type set to None. Had prefragmented objects which were destroyed.";
        public const string rig_frg2   = ". Demolition Type set to Runtime. Had prefragmented objects which were destroyed.";
        public const string rig_rnt    = ". Demolition Type is Runtime, Use Shatter is On. Unsupported fragments type. Runtime Caching supports only Voronoi, Splinters, Slabs and Radial fragmentation types. Runtime Caching was Disabled.";
        public const string rig_awk1   = ". Demolition Type set to Awake Precache. Had manually precached Unity meshes which were overwritten.";
        public const string rig_awk2   = ". Demolition Type set to Awake Precache. Had manually prefragmented objects which were destroyed.";
        public const string rig_awk3   = ". Demolition Type set to Awake Prefragment. Has manually prefragmented objects";
        public const string rig_awk4   = ". Demolition Type set to Awake Prefragment. Has manually precached Unity meshes.";
        public const string rig_plane  = " had planar low poly mesh. Object can't get Mesh Collider.";
        public const string rig_init   = ". Demolition Reference object has already initialized Rigid. Set By Method Initialization type or Deactivate reference.";
        public const string rig_ref    = ". Reference Demolition object is prefab asset. Reference Demolition Action property changed to Instantiate.";
        public const string rig_res1   = ". Mesh Root Fragment destroyed.";
        public const string rig_res2   = ". Reset not supported.";
        public const string rig_cls1   = ". GetShardsBoundByPosition warning.";
        public const string rig_cls2   = ". GetShardsBound warning.";
        public const string rig_cls3   = ". GetShardsBound warning.";
        
        // RigidRoot
        public const string rot_noChld = " has no children. RigidRoot should be used on object with children.";
        public const string rot_Edt    = " has Editor Setup but its connection data is not cached. Reset Setup and use Editor Setup again.";
        public const string rot_noCon  = " has enabled Connectivity activation but has no Connectivity component.";
        public const string rot_conDis = " has Connectivity component but activation by Connectivity is disabled.";
        public const string rot_noFlt  = " has no MeshFilter. Shard won't be simulated.";
        public const string rot_noMesh = " has no mesh. Shard won't be simulated.";
        public const string rot_noVert = " has 3 or less vertices. Shard can't get Mesh Collider and won't be simulated.";
        public const string rot_small  = " is very small and won't be simulated.";
        public const string rot_planar = " has planar low poly mesh. Shard can't get Mesh Collider and won't be simulated.";
        public const string rot_noShr  = " has missing shards. Reset Setup and use Editor Setup again.";
        public const string rot_noRig  = " has missing Rigid component with MeshRoot object type. Reset Setup and use Editor Setup again.";

        // Shatter
        public const string sht_pref   = ". Can't fragment prefab because prefab unable to store Unity mesh. Fragment prefab in scene.";
        public const string sht_noMesh = ". MeshFilter has no Mesh, object excluded.";
        public const string sht_noRead = ". Mesh is not Readable, object excluded. Enable Read/Write in Import Settings.";
        public const string sht_noFlt  = ". Object has no mesh to fragment.";
        public const string sht_noTet  = ". Tet fragmentation type is not supported by V2 engine for now.";
        public const string sht_time   = "Rayfire Shatter: Fragmentation time: ";
        public const string sht_small  = " is too small.";
        public const string sht_noPnt  = " Point Cloud is empty.";
        public const string sht_bad1   = " Bad Shatter output mesh.";
        public const string sht_bad2   = " Bad Slice output mesh.";
        public const string sht_null   = " Null mesh.";
        public const string sht_empt   = " Empty Mesh.";
        public const string sht_low   = " Mesh amount: ";
        
        // Shell
        public const string shl_noEdg = " has no open edges. Shell disabled.";
        
        // Sound Debug
        public const string sht_noRig = ". Sound component has no attached Rigid or RigidRoot component.";
        public const string sht_noEve = ". All events disabled. Enable at least one event.";
        public const string sht_noIni = ". Initialization sound has no clips to play.";
        public const string sht_noAct = ". Activation sound has no clips to play.";
        public const string sht_noDml = ". Demolition sound has no clips to play.";
        public const string sht_noCol = ". Collision sound has no clips to play.";
        
        // RayfireMan.Log ($"{RFLog.rig_dbgn}{scr.name}{RFLog.rig_child}", scr.gameObject);
        // static float fval = 5f;
        // Debug.Log ($"String {fval} name: {scr_bmb}, more");
        // [Flags] https://giannisakritidis.com/blog/Enum-Flags-In-Unity/#to-remove-a-value-the-code-is-equally-simple
    }
}
