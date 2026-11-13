using UnityEngine;

public class PowerUpVFX : MonoBehaviour
{
    public Animator animator;

    public ParticleSystem particle;

    public void Collected()
    {
        animator.SetTrigger("Collected");
    }

    private void end_animation()
    {
        Debug.Log("@Andre to so dando Destroy no obj, nn sei se precisa mexer em alguma coisa por causa do mirror"); 
        Destroy(this.gameObject);
    }

    private void active_particle()
    {
        particle.Play();
    }
}
