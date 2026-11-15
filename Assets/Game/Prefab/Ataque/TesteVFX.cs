using UnityEngine;

public class TesteVFX : MonoBehaviour
{
    public GameObject disparo;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Instantiate(disparo, transform.position, transform.rotation);
        }
    }
}
