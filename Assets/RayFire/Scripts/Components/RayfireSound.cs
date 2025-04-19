using System;
using UnityEngine;

namespace RayFire
{
    [SelectionBase]
    [AddComponentMenu (RFLog.snd_path)]
    [HelpURL (RFLog.snd_link)]
    public class RayfireSound : MonoBehaviour
    {
        public float   baseVolume;
        public float   sizeVolume;
        public RFSound initialization;
        public RFSound activation;
        public RFSound demolition;
        public RFSound collision;
        public float   relativeVelocity;
        public float   lastCollision;
        public float   minimumSize;
        public float   cameraDistance;
        
        // Non Serialized
        [NonSerialized] public RayfireRigid     rigid;
        [NonSerialized]        RayfireRigidRoot rigidRoot;
        [NonSerialized]        MeshRenderer     meshRenderer;
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RayfireSound()
        {
            baseVolume       = 1f;
            sizeVolume       = 0.2f;
            minimumSize      = 0f;
            cameraDistance   = 0;
            relativeVelocity = 5f;
        }

        // Start
        void Start()
        {
            SetComponents();
        }
        
        // Initialize 
        void SetComponents()
        {
            // Set rigid
            rigid = GetComponent<RayfireRigid>();
            if (rigid != null)
            {
                rigid.sound  = this;
                meshRenderer = rigid.mRnd;
            }

            // Set rigidroot
            rigidRoot = GetComponent<RayfireRigidRoot>();
            if (rigidRoot != null)
                rigidRoot.sound = this;
            
            // Mesh Renderer
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
            
            // Checks
            WarningCheck();
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// ///////////////////////////////////////////////////////// 
        
        // Initialize
        void WarningCheck()
        {
            // Skip warnings
            if (RayfireMan.debugStateStatic == false)
                return;
            
            // Has no host
            if (initialization.enable == false &&
                activation.enable == false &&
                demolition.enable == false)
                if (rigid == null && rigidRoot == null)
                    RayfireMan.Log (RFLog.snd_dbgn + name + RFLog.sht_noRig, gameObject);

            // All disabled
            if (initialization.enable == false &&
                activation.enable == false &&
                demolition.enable == false &&
                collision.enable == false)
                RayfireMan.Log (RFLog.snd_dbgn + name + RFLog.sht_noEve, gameObject);

            // No clips
            if (initialization.enable == true)
                if (initialization.clip == null && initialization.HasClips == false)
                    RayfireMan.Log (RFLog.snd_dbgn + name + RFLog.sht_noIni, gameObject);
            if (activation.enable == true)
                if (activation.clip == null && activation.HasClips == false)
                    RayfireMan.Log (RFLog.snd_dbgn + name + RFLog.sht_noAct, gameObject);
            if (demolition.enable == true)
                if (demolition.clip == null && demolition.HasClips == false)
                    RayfireMan.Log (RFLog.snd_dbgn + name + RFLog.sht_noDml, gameObject);
            if (collision.enable == true)
                if (collision.clip == null && collision.HasClips == false)
                    RayfireMan.Log (RFLog.snd_dbgn + name + RFLog.sht_noCol, gameObject);
        }

        // Copy from
        public void CopyFrom (RayfireSound source)
        {
            baseVolume     = source.baseVolume;
            sizeVolume     = source.sizeVolume;
            initialization = new RFSound(source.initialization);
            activation     = new RFSound(source.activation);
            demolition     = new RFSound(source.demolition);
            collision      = new RFSound(source.collision);
            minimumSize    = source.minimumSize;
            cameraDistance = source.cameraDistance;
        }
        
        // Create audio source and play clip
        void CreateSource(RayfireRigid scr)
        {
            GameObject soundGo = new GameObject("SoundSource");
            soundGo.transform.position = scr.gameObject.transform.position;
            AudioSource audioSource = soundGo.AddComponent<AudioSource>();
            audioSource.clip                  = demolition.clip;
            audioSource.mute                  = false;
            audioSource.bypassEffects         = false;
            audioSource.bypassListenerEffects = false;
            audioSource.bypassReverbZones     = false;
            audioSource.playOnAwake           = false;
            audioSource.loop                  = false;
            audioSource.priority              = 127;
            audioSource.volume                = demolition.multiplier;
            audioSource.pitch                 = 1f;
            audioSource.panStereo             = 0f;
            audioSource.spatialBlend          = 0f;
            audioSource.reverbZoneMix         = 1f;
            audioSource.minDistance           = 0f;
            //audioSource.maxDistance           = demolitionSound.maxDistance;
            audioSource.PlayOneShot (demolition.clip, demolition.multiplier);
            Destroy (soundGo, demolition.clip.length);
        }

        /// /////////////////////////////////////////////////////////
        /// Collision
        /// ///////////////////////////////////////////////////////// 
        
        // Collision check
        public void OnCollisionEnter (Collision coll)
        {
            CollisionSound (coll);
        }

        // Collision sound
        void CollisionSound(Collision coll)
        { 
            if (collision.enable == true)
            {
                // Save last collision strength
                lastCollision = coll.relativeVelocity.magnitude;

                // Play sound if higher than threshold
                if (lastCollision > relativeVelocity)
                {
                    // Get size
                    float size = 0f;
                    if (rigid != null)
                        size = rigid.limitations.size;
                    else if (meshRenderer != null)
                        size = meshRenderer.bounds.size.magnitude;

                    // Play
                    RFSound.CollisionSound (this, size);
                }
            }
        }
    }
}
