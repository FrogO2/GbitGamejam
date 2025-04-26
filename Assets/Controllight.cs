using UnityEngine;

public class Controllight : MonoBehaviour
{
    private bool flag = false;
    private void SetAlarmingBool()
    {
        GetComponent<Animator>().SetBool("IsAlarming", flag);
        flag = !flag;
    }
    
    
}
