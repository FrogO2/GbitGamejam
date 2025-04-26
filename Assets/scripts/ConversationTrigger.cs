using UnityEngine;
using DialogueEditor;
using UnityEditor.UI;

public class ConversationTrigger : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enabled && other.CompareTag("Player"))
        {
            ConversationManager.Instance.StartConversation(GetComponent<NPCConversation>());
            enabled = false;
        }
    }
}
