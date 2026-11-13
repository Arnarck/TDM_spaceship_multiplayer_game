using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    public Animator animator;
    public ParticleSystem particle_damage;
    

    public void take_damage()
    {
        animator.SetTrigger("Damage");
        particle_damage.Play();
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
