using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    public Animator animator;
    public ParticleSystem particle_damage, particle_die;
    

    public void take_damage()
    {
        animator.SetTrigger("Damage");
    }
    private void start_particle_damage()
    {
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
    public void Die()
    {
        particle_die.Play();
    }
}
