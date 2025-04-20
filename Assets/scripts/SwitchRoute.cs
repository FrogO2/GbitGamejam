using System.Collections.Generic;
using UnityEngine;

public class SwitchRoute : MonoBehaviour
{
    public Collider2D[] triggers;
    public Material[] materials;
    private List<Collider2D> trgs = new List<Collider2D>();
    
    private void Update()
    {
        for (int i = 0; i < triggers.Length; i++)
        {
            triggers[i].GetContacts(trgs);
            if (trgs.Count > 0)
                foreach (var trg in trgs)
                {
                    if (trg.gameObject.CompareTag("Player"))
                        GetComponent<Renderer>().material = materials[i];
                }
        }
    }
}
