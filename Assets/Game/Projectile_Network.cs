using UnityEngine;
using Mirror;

public class Projectile_Network : NetworkBehaviour
{
    public float lifetime;
    public float move_speed;
    public TrailRenderer trail;

    [Header("INTERNAL")]
    public PlayerController_Network player_owner;
    public float lifetime_t;
    [SyncVar(hook = nameof(on_connection_id_changed))]public int connection_id = -1;

    public void init(Vector3 position, Quaternion rotation)
    {
        gameObject.SetActive(true);

        transform.position = position;
        transform.rotation = rotation;

        lifetime_t = lifetime;
    }

    private void Start()
    {
        for (int i = 0; i < GI.players_connected.Count; i++)
        {
            PlayerController_Network player = GI.players_connected[i];
            if (player.connection_id == connection_id)
            {
                player_owner = player;
                break;
            }
        }

        transform.SetParent(GI.game_mode.projectiles_parent[connection_id]);
        destroy(false);
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        if (!isServer || !player_owner) { return; }

        // Movement
        transform.position += transform.forward*move_speed*dt;
        if (player_owner.current_powerup == Powerup_Type.RICOCHET)
        {
            if (transform.position.x >= GI.game_mode.screen_bounds.x || transform.position.x <= -GI.game_mode.screen_bounds.x)
            {
                // Prevents from crossing the screen bounds
                Vector3 position = transform.position;
                if (transform.position.x > 0f) { position.x =  GI.game_mode.screen_bounds.x; }
                else                           { position.x = -GI.game_mode.screen_bounds.x; }
                transform.position = position;

                // Flip direction
                Vector3 v = transform.forward;
                v.x *= -1f;
                transform.forward = v;
            }
            if (transform.position.z >= GI.game_mode.screen_bounds.z || transform.position.z <= -GI.game_mode.screen_bounds.z)
            {
                Vector3 position = transform.position;
                if (transform.position.z > 0f) { position.z =  GI.game_mode.screen_bounds.z; }
                else                           { position.z = -GI.game_mode.screen_bounds.z; }
                transform.position = position;

                Vector3 v = transform.forward;
                v.z *= -1f;
                transform.forward = v;
            }
        }

        // Lifetime
        if (lifetime_t > 0f)
        {
            lifetime_t -= dt;
            if (lifetime_t <= 0f && isServer)
            {
                server_destroy(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) { return; }

        if (other.CompareTag("Player"))
        {
            PlayerController_Network player = other.GetComponent<PlayerController_Network>();
            if (player_owner != player)
            {
                player.server_lose_life();
                server_destroy(true);
            }
        }
    }
    
    public void destroy(bool collided)
    {
        gameObject.SetActive(false);
        player_owner.destroy_projectile(gameObject);
        trail.Clear();

        if (collided)
        {
            GI.game_mode.hit_vfx.spawn(transform.position);
        }
    }

    public void on_connection_id_changed(int old_value, int new_value)
    {
        
    }

    [Server]
    public void server_destroy(bool collided)
    {
        player_owner.rpc_destroy_projectile(gameObject, collided);
    }
}
