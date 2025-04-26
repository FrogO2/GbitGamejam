using UnityEngine;
using MoreMountains.CorgiEngine;
using UnityEditor.UI;

public class CartTrigger : MonoBehaviour
{
    public MovingPlatform cart;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            print("moving");
            cart.MoveTowardsEnd();
        }
    }
}
