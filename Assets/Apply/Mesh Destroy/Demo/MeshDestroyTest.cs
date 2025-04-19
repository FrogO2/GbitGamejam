using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshDestroy;

public class MeshDestroyTest : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Destroy();
    }

    private void Destroy()
    {
        //Êó±ê×ó¼üµã»÷´Ý»Ù
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                MeshDestroyAble destroyAble = hit.collider.GetComponent<MeshDestroyAble>();
                if (destroyAble != null)
                {
                    MeshDestroyMachine.Instance.DestroyMesh(destroyAble);
                }
            }
        }
    }
}