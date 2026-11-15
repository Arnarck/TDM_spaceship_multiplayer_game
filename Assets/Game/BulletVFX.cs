using UnityEngine;

public class BulletVFX : MonoBehaviour
{
    public float velocidade = 10;

    public GameObject Muzzle, Impacto;
    
    public void Update()
    {
        if(velocidade != 0)
        {
            transform.position += transform.forward * (velocidade * Time.deltaTime);
        }
    }
    void Start()
    {
        var muzzle = Instantiate(Muzzle, transform.position, Quaternion.identity);
        muzzle.transform.forward = gameObject.transform.forward;
        Destroy(muzzle.gameObject, 2f);
    }

    private void OnCollisionEnter(Collision obj)
    {
        ContactPoint contato = obj.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contato.normal);
        Vector3 pos = contato.point;

        var impacto = Instantiate(Impacto, pos, rot); 
        Destroy(impacto.gameObject, 2f);


        velocidade = 0;
        Destroy(gameObject);


        Debug.Log("To mandando o Projetil se auto destroir quando colide com alguma coisa nn sei se da problema com o mirror");
    }
}
