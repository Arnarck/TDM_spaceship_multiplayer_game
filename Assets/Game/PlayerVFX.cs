using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    public Animator animator;

    

    public void take_damage()
    {
        animator.SetTrigger("Damage");
    }

    public void take_powerUp()
    {
        animator.SetTrigger("PowerUp");
    }

    public void shoot()
    {
        animator.SetTrigger("Shoot");
    }
}
