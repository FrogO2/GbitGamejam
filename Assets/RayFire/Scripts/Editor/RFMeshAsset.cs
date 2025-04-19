using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using RayFire;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RayFireEditor
{
	// Class to save mesh to asset file
	public static class RFMeshAsset
	{
		// 
		public static string shatterPath = "Assets/";
		
		/// //////////////////////////////////////////////////
		/// Combine 
		/// //////////////////////////////////////////////////
		
        // Save mesh as asset
        public static void SaveMesh (MeshFilter mf, string name) 
        {
	        if (mf == null)
	        {
		        Debug.Log ("MeshFilter is null");
		        return;
	        }
	        
	        if (mf.sharedMesh == null)
            {
                Debug.Log ("Mesh is null");
                return;
            } 
    
			// Save path
            string savePath = EditorUtility.SaveFilePanel ("Save Mesh Asset", "Assets/", name, "asset");
        	
            // No path
            if (string.IsNullOrEmpty(savePath) == true) 
                return;
            
            // Convert path
        	savePath = FileUtil.GetProjectRelativePath(savePath);
    
            // No path
            if (string.IsNullOrEmpty(savePath) == true) 
                return;
            
            // Delete existing
            bool exist = AssetDatabase.GetMainAssetTypeAtPath (savePath) != null;
            if (exist == true)
	            AssetDatabase.DeleteAsset (savePath);
            
            // Create asset
        	AssetDatabase.CreateAsset(mf.sharedMesh, savePath);
            AssetDatabase.SaveAssets();
        }

		/// //////////////////////////////////////////////////
		/// Shatter
		/// //////////////////////////////////////////////////

		// Get asset save path
		static string GetSavePath(string saveName)
		{
			// Save path
			string savePath = EditorUtility.SaveFilePanel ("Save Fragments To Asset", shatterPath, saveName, "asset");
            
			// Convert path
			savePath = FileUtil.GetProjectRelativePath(savePath);

			// No path
			if (string.IsNullOrEmpty(savePath) == true) 
				return "";

			// Save path for next save
			shatterPath = Path.GetDirectoryName (savePath);

			return savePath;
		}
		
		// Export meshes into asset
        static void ExportMeshes(List<MeshFilter> meshFilters, List<Mesh> meshes, string savePath, string saveName)
        {
	        // Empty mesh
	        Mesh emptyMesh = new Mesh ();
	        emptyMesh.name = saveName;

	        // Create asset
	        AssetDatabase.CreateAsset(emptyMesh, savePath);
            
	        // Save each fragment mesh
	        for (int i = 0; i < meshFilters.Count; i++)
	        {
		        // Skip if no mesh
		        if (meshFilters[i] == null)
			        continue;
	            
		        // Apply to meshfilter to avoid save of already referenced mesh
		        meshFilters[i].sharedMesh = meshes[i];

		        // Add all meshes
		        AssetDatabase.AddObjectToAsset (meshFilters[i].sharedMesh, savePath);
	        }

	        // Save
	        AssetDatabase.SaveAssets();
        }
        
        // Save mesh as asset
        public static bool ExportBatch (RayfireShatter shatter, RFShatterBatch batch, string path)
        {
	        // Get save name
	        string saveName = shatter.gameObject.name + "_mesh";
	        
	        // Get save path
	        string savePath = path;
	        if (path == null)
		        savePath = GetSavePath (saveName);
            
	        // No path
	        if (savePath.Length == 0)
		        return false;
            
	        // Collect all meshes to save
	        bool hasMesh = false;
            
	        // Collect fragments meshes
	        if (batch.HasFragments == false)
		        return false;
			
	        // Collect meshes
	        List<Mesh>       meshes      = new List<Mesh>();
	        List<MeshFilter> meshFilters = new List<MeshFilter>();
	        foreach (var fragTm in batch.fragments)
	        {
		        // Skip
		        if (fragTm == null)
					continue;

		        // Get mf
		        MeshFilter mf = fragTm.GetComponent<MeshFilter>();
		        meshFilters.Add (mf);
	            
		        // No mf
		        if (mf == null)
			        meshes.Add (null);

		        // No mesh
		        if (mf != null && mf.sharedMesh == null)
			        meshes.Add (null);
	            
		        // New mesh
		        Mesh tempMesh = Object.Instantiate(mf.sharedMesh);
		        tempMesh.name = mf.sharedMesh.name;
	            
		        // Collect
		        meshes.Add (tempMesh);

		        // List has mesh
		        hasMesh = true;
	        }

	        // List has no meshes to save
	        if (hasMesh == false)
		        return false;

	        // Export meshes into asset
	        ExportMeshes (meshFilters, meshes, savePath, saveName);

	        return true;
        }

        /// //////////////////////////////////////////////////
        /// Recorder
        /// //////////////////////////////////////////////////

        // Export demolished rigid runtime fragments
        public static void SaveFragments(RayfireRigid rigid, string path)
        {
	        
			// Export meshes into asset
	        // ExportMeshFilters (meshFilters, savePath, saveName);
        }

        // Export meshes into asset
        static void ExportMeshFilters(List<MeshFilter> meshFilters, string savePath, string saveName)
        {
	        // Empty mesh
	        Mesh emptyMesh = new Mesh {name = saveName};

	        // Create asset
	        AssetDatabase.CreateAsset(emptyMesh, savePath);
            
	        // Save each fragment mesh
	        for (int i = 0; i < meshFilters.Count; i++)
	        {
		        // Skip if no meshfilter
		        if (meshFilters[i] == null)
			        continue;
		        
		        // Skip if no mesh
		        if (meshFilters[i].sharedMesh == null)
			        continue;

		        // Add all meshes
		        AssetDatabase.AddObjectToAsset (meshFilters[i].sharedMesh, savePath);
	        }
            
	        // Save
	        AssetDatabase.SaveAssets();
        }
	}
}