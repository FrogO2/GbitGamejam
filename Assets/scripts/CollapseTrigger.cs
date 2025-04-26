using System;
using System.Collections.Generic;
using System.Linq;
using RayFire;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class CollapseTrigger : MonoBehaviour
{
    public Collider2D[] triggers;
    public GameObject[] objectsToDisable;
    public GameObject[] objectsToEnable;

    private List<Collider2D> trgs = new List<Collider2D>();
    bool[] flags;
    
    private void Start()
    {
        flags = new bool[triggers.Length];
    }

    private void Update()
    {
        bool flag = true;
        for (int i = 0; i < triggers.Length; i++)
        {
            triggers[i].GetContacts(trgs);
            if (trgs.Count > 0)
                foreach (var trg in trgs)
                {
                    if (trg.gameObject.CompareTag("Player"))
                        flags[i] = true;
                }

            flag = flag && flags[i];
        }

        if (flag)
            activate();
    }

    private void activate()
    {
        if (enabled)
        {
            foreach (var gameObject in objectsToDisable)
            {
                gameObject.SetActive(false);
            }
            
            foreach (var gameObject in objectsToEnable)
            {
                gameObject.SetActive(true);
            }
            
            enabled = false;
        }
    }
    
}
