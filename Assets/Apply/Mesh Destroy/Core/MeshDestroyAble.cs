using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshDestroy
{
    public class MeshDestroyAble : MonoBehaviour
    {
        public int cutCascades = 1;
        public float explodeForce = 0;
        public bool inherit = true;

        [HideInInspector] public MeshFilter meshFilter;
        [HideInInspector] public MeshRenderer meshRenderer;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void Inherit(MeshDestroyAble destroyAble)
        {
            this.cutCascades = destroyAble.cutCascades;
            this.explodeForce = destroyAble.explodeForce;
            this.inherit = destroyAble.inherit;
        }

        public Mesh OrigionMesh { get { return meshFilter.mesh; } }
    }
}