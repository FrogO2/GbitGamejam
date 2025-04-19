using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Environment/Key Locked Teleporter")]
    public class KeyLockedTeleporter : Teleporter 
    {
        [Header("Key Requirement")]
        
        [Tooltip("需要检测的钥匙ID")]
        public string RequiredKeyID = "GateKey";
        
        [Tooltip("没有钥匙时的提示反馈")]
        public MMFeedbacks MissingKeyFeedback;

        [Header("Key Consumption")]
        [Tooltip("是否消耗钥匙")]
        public bool ConsumeKey = true;

        [Tooltip("消耗钥匙后永久解锁")]
        public bool PermanentUnlock = true;

        protected bool _keyConsumed = false;
        protected bool _permanentlyUnlocked = false;

        #if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (PermanentUnlock && !ConsumeKey)
            {
                ConsumeKey = true;
                EditorUtility.SetDirty(this);
            }
        }
        #endif

        public override void TriggerButtonAction(GameObject instigator)
        {
            // 如果已永久解锁，直接调用基类传送
            if (_permanentlyUnlocked)
            {
                base.TriggerButtonAction(instigator);
                return;
            }

            Inventory inventory = Inventory.FindInventory("MainInventory", "Player1");
            
            if (inventory == null)
            {
                Debug.LogError("Inventory system not found");
                return;
            }

            List<int> keySlots = inventory.InventoryContains(RequiredKeyID);
            
            if (keySlots.Count == 0)
            {
                PlayMissingKeyFeedback();
                return;
            }

            // 标记需要消耗钥匙
            if (ConsumeKey)
            {
                _keyConsumed = true;
            }

            // 如果是永久解锁模式，标记状态
            if (PermanentUnlock && ConsumeKey)
            {
                _permanentlyUnlocked = true;
            }

            base.TriggerButtonAction(instigator);
        }

        protected override void SequenceEnd(Collider2D collider)
        {
            base.SequenceEnd(collider);

            if (_keyConsumed)
            {
                Inventory inventory = Inventory.FindInventory("MainInventory", "Player1");
                inventory.RemoveItemByID(RequiredKeyID, 1);
                int quantityBefore = inventory.GetQuantity(RequiredKeyID);
                bool removalResult = inventory.RemoveItemByID(RequiredKeyID, 1);
                int quantityAfter = inventory.GetQuantity(RequiredKeyID);

                Debug.Log($"钥匙消耗结果: {removalResult} | 数量变化: {quantityBefore} -> {quantityAfter}");

                _keyConsumed = false; // 重置消耗标记
            }
        }

        protected virtual void PlayMissingKeyFeedback()
        {
            if (MissingKeyFeedback != null)
            {
                MissingKeyFeedback.PlayFeedbacks();
            }
            else
            {
                Debug.Log("Required key: " + RequiredKeyID);
            }
        }
    }
}