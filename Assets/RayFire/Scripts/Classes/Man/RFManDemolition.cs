using System;
using UnityEngine;

namespace RayFire
{
    /// <summary>
    /// Rayfire Man advanced demolition properties class.
    /// </summary>
    [Serializable]
    public class RFManDemolition
    {
        // UI
        public FragmentParentType parent;
        public Transform          globalParent;
        public int                currentAmount;
        public int                maximumAmount;
        public int                badMeshTry;
        public float              sizeThreshold;

        // Non Serialized
        [NonSerialized] bool amountWaring;

        public RFManDemolition()
        {
            parent        = FragmentParentType.Manager;
            maximumAmount = 1000;
            badMeshTry    = 3;
            sizeThreshold = 0.05f;
            currentAmount = 0;
        }
        
        // Change current amount value
        public void ChangeCurrentAmount (int am)
        {
            // Add/subtract
            currentAmount += am;

            // One time Warning to avoid Debug spam in game build
            if (currentAmount >= maximumAmount)
                AmountWarning();
        }

        public void AmountWarning()
        {
            if (amountWaring == false)
                RayfireMan.Log ($"{RFLog.man_dbgn}{RayfireMan.inst.gameObject.name}{RFLog.man_amount}", RayfireMan.inst.gameObject);
            amountWaring = true;
            
        }

        public void ResetCurrentAmount()
        {
            currentAmount = 0;
        }
    }
}