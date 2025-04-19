using UnityEngine;

namespace RayFire
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu(RFLog.snp_path)]
    [HelpURL(RFLog.snp_link)]
    public class RayfireSnapshot: MonoBehaviour
    {
        public string assetName;
        public bool   compress;
        public Object snapshotAsset;
        public float  sizeFilter;

        // Reset
        void Reset()
        {
            assetName = gameObject.name;
        }
        
#if UNITY_EDITOR
        
        // Save asset
        public void Snapshot()
        {
            RFSnapshotAsset.Snapshot (gameObject, compress, assetName);
        }

        // Load asset
        public void Load()
        {
            RFSnapshotAsset.Load (snapshotAsset, gameObject, sizeFilter);
        }
#endif     
        
    }
}
