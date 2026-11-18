using UnityEngine;
using Mirror;


public enum Powerup_Type
{
    NONE,
    MULTISHOT,
    RICOCHET,
    HEAL,

    COUNT
}


public class Powerup_Network : NetworkBehaviour
{
    public Collider powerup_collider;
    public PowerUpVFX[] powerups;

    [Header("INTERNAL")]
    public float activate_powerup_t;
    public Powerup_Type current_powerup;

    void Awake()
    {
        GI.powerup = this;
    }

    public override void OnStartClient()
    {
        hide_powerup();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) { return; }

        float dt = Time.deltaTime;

        if (activate_powerup_t > 0f)
        {
            activate_powerup_t -= dt;
            if (activate_powerup_t <= 0f)
            {
                server_activate_powerup();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) { return; }

        if (other.CompareTag("Player") && activate_powerup_t <= 0f)
        {
            other.GetComponent<PlayerController_Network>().server_use_powerup(current_powerup);

            hide_powerup();
            activate_powerup_t += 4f;

            GI.local_player.rpc_hide_powerup();
        }
    }

    [Server]
    public void server_activate_powerup()
    {
        Powerup_Type random_powerup = (Powerup_Type)Random.Range(1, (int)Powerup_Type.COUNT); // 0 is 'NONE'

        Vector3 spawn_position;
        spawn_position.x = Random.Range(-GI.game_mode.screen_bounds.x, GI.game_mode.screen_bounds.x);
        spawn_position.y = 0f;
        spawn_position.z = Random.Range(-GI.game_mode.screen_bounds.z, GI.game_mode.screen_bounds.z);

        show_powerup(random_powerup, spawn_position);

        GI.local_player.rpc_show_powerup(current_powerup, spawn_position);
    }

    public void show_powerup(Powerup_Type type, Vector3 position)
    {
        current_powerup = type;
        transform.position = position;
        for (int i = 0; i < powerups.Length; i++)
        {
            if (i == ((int)type - 1))
            {
                powerups[i].gameObject.SetActive(true);
                powerups[i].particle.Play();
            }
            else
            {
                powerups[i].gameObject.SetActive(false);
            }
        }

        powerup_collider.enabled = true;
    }

    public void hide_powerup()
    {
        powerup_collider.enabled = false;
        if (current_powerup != Powerup_Type.NONE)
        {
            powerups[((int)current_powerup - 1)].gameObject.SetActive(false);
        }
    }
}
