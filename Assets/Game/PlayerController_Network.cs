using UnityEngine;
using Mirror;
using TMPro;
using Utp;
using System.Collections.Generic;

public class PlayerController_Network : NetworkBehaviour
{
    public GameObject enemy_lives_hud;
    public TextMeshProUGUI lives_text_enemy;
    public GameObject projectile_prefab;
    public int max_lives;
    public float friction;
    public float acceleration;
    public float fire_time;

    [Header("INTERNAL")]
    [SyncVar(hook = nameof(on_life_changed))] public int lives;
    [SyncVar(hook = nameof(on_connection_id_changed))] public int connection_id = -1;
    public bool freeze_player;
    public float fire_t;
    public float powerup_t;
    public Powerup_Type current_powerup;
    public Vector3 dp;
    public Vector3 ddp;
    public Vector3 move_direction;
    public Vector3 last_frame_direction;
    public List<GameObject> free_projectiles;
    public List<GameObject> living_projectiles;

    private void Awake()
    {
        GI.players_connected.Add(this);
    }

    private void Start()
    {
        lives = max_lives;
        freeze_player = true;
        set_start_rotation();
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            enemy_lives_hud.SetActive(false);
            GI.local_player = this;

            if (isServer)
            {
                GI.local_player_hud.join_panel.SetActive(true);
                GI.local_player_hud.join_code_text.text = FindAnyObjectByType<CustomNetworkManager>().relayJoinCode;
            }
            else
            {
                GI.local_player_hud.connecting_panel.SetActive(true);
            }
        }
        else
        {
            enemy_lives_hud.SetActive(true);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (freeze_player) { return; }

        if (Input.GetKeyDown(KeyCode.Space) && isLocalPlayer)
        {
            if (isServer)
            {
                NetworkServer.Shutdown();
            }
            else
            {
                NetworkClient.Disconnect();
            }
        }
        
        { // Movement
            ddp = Vector3.zero;
            if (isLocalPlayer)
            {
                // Local player movement
                if (Input.GetKey(KeyCode.W)) { ddp.z += 1f; }
                if (Input.GetKey(KeyCode.S)) { ddp.z -= 1f; }
                if (Input.GetKey(KeyCode.A)) { ddp.x -= 1f; }
                if (Input.GetKey(KeyCode.D)) { ddp.x += 1f; }
            }
            else
            {
                // Client movement
                ddp = move_direction;
            }

            ddp.Normalize();
            Vector3 dir = ddp;

            ddp *= acceleration;
            ddp -= friction*dp;

            dp += ddp*dt;
            transform.position += (dp*dt) + ddp*(dt*dt*0.5f);

            // Clamp position
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -GI.game_mode.screen_bounds.x, GI.game_mode.screen_bounds.x);
            p.z = Mathf.Clamp(p.z, -GI.game_mode.screen_bounds.z, GI.game_mode.screen_bounds.z);
            transform.position = p;

            // Replication
            if (isLocalPlayer /*&& dir != last_frame_direction*/)
            {
                cmd_update_movement(transform.position, dir, dp, acceleration, friction);
            }

            last_frame_direction = dir;
        }

        { // Look at cursor
            if (isLocalPlayer)
            {
                Vector3 cursor_position = Input.mousePosition;
                cursor_position.z = 28.12f;
                cursor_position = Camera.main.ScreenToWorldPoint(cursor_position);

                transform.forward = cursor_position - transform.position;

                cmd_update_rotation(transform.rotation);
            }
        }

        { // Shoot
            if (fire_t > 0f)
            {
                fire_t -= dt;
            }

            if (isLocalPlayer)
            {
                if (Input.GetMouseButton(0) && fire_t <= 0f)
                {
                    cmd_maybe_shoot_projectile();
                }
            }
        }

        { // Powerup
            if (isServer)
            {
                if (powerup_t > 0f)
                {
                    powerup_t -= dt;
                    if (powerup_t <= 0f)
                    {
                        current_powerup = Powerup_Type.NONE;
                    }
                }
            }
        }

        enemy_lives_hud.transform.position = transform.position + Vector3.forward*2f;
        enemy_lives_hud.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }


    // ================================= Helper Functions =================================

    public void set_start_rotation()
    {
        Quaternion rotation = Quaternion.Euler(0f, 90f, 0f); // Rotation for player 1
        if (transform.position.x > 0f)
        {
            rotation = Quaternion.Euler(0f, -90f, 0f); // Rotation for player 2
        }

        transform.rotation = rotation;
    }

    public void update_enemy_lives()
    {
        lives_text_enemy.text = "Lives: " + lives;
    }

    public GameObject spawn_projectile()
    {
        if (free_projectiles.Count < 1)
        {
            //@BUG: Sometimes the free_projectiles pool isn't filled when the projectile calls 'destroy_projectile'. This is a workaround
            Transform projectiles_parent = GI.game_mode.projectiles_parent[connection_id];
            for (int i = 0; i < projectiles_parent.childCount; i++)
            {
                GameObject projectile = projectiles_parent.GetChild(0).gameObject;
                if (!projectile.activeInHierarchy)
                {
                    destroy_projectile(projectile);
                }
            }

            if (free_projectiles.Count < 1)
            {
                Debug.Assert(false, "No more projectiles available in the pool!");
                return null;
            }
        }

        GameObject go = free_projectiles[free_projectiles.Count - 1];
        go.GetComponent<Projectile_Network>().init(Vector3.zero, Quaternion.identity);

        free_projectiles.RemoveAt(free_projectiles.Count - 1);
        living_projectiles.Add(go);

        return go;
    }

    public void destroy_projectile(GameObject go)
    {
        living_projectiles.Remove(go);
        free_projectiles.Add(go);
    }


    // ================================= Server Calls =================================

    [Server]
    public void server_use_powerup(Powerup_Type powerup)
    {
        if (powerup == Powerup_Type.HEAL)
        {
            server_add_lives(max_lives);
        }
        else
        {
            current_powerup = powerup;
            powerup_t = 2f;
        }
    }

    // Player losing life directly from the server
    [Server]
    public void server_lose_life()
    {
        if (lives < 1) { return; }

        lives--;
        if (lives <= 0)
        {
            // Game Over
            GI.game_mode.game_over(connectionToClient.connectionId);
        }
    }

    [Server]
    public void server_add_lives(int amount)
    {
        lives += amount;
        lives = Mathf.Min(lives, max_lives);
    }


    // ================================= Sync Vars =================================

    public void on_life_changed(int old_value, int new_value)
    {
        // update HUD
        GI.local_player_hud.update_lives();
        update_enemy_lives();
    }

    public void on_connection_id_changed(int old_value, int new_value)
    {
        
    }


    // ================================= RPCs =================================

    [ClientRpc]
    public void rpc_spawn_projectile(Vector3 position, Quaternion rotation)
    {
        Projectile_Network projectile = spawn_projectile().GetComponent<Projectile_Network>();
        projectile.init(position, rotation);
    }

    [ClientRpc]
    public void rpc_destroy_projectile(GameObject go)
    {
        go.GetComponent<Projectile_Network>().destroy();
    }

    [ClientRpc]
    public void rpc_prepare_for_next_match(Vector3 position)
    {
        lives = max_lives;
        transform.position = position;
        set_start_rotation();
        update_enemy_lives();

        if (isLocalPlayer)
        {
            GI.local_player_hud.hide_win_message();
            GI.local_player_hud.show_next_match_countdown();
        }
    }

    [ClientRpc]
    public void rpc_hide_powerup()
    {
        GI.powerup.hide_powerup();
    }

    [ClientRpc]
    public void rpc_show_powerup(Powerup_Type type, Vector3 position)
    {
        GI.powerup.show_powerup(type, position);
    }

    [TargetRpc]
    public void target_start_game()
    {
        GI.local_player_hud.hide_relay_panel();
        GI.local_player_hud.show_next_match_countdown();
    }

    [ClientRpc]
    public void rpc_start_game()
    {
        freeze_player = false;
    }

    [ClientRpc]
    public void rpc_start_match()
    {
        freeze_player = false;
    }

    [ClientRpc]
    public void rpc_game_over(int player_winner)
    {
        freeze_player = true;
        dp = Vector3.zero;
        move_direction = Vector3.zero;

        if (isLocalPlayer) GI.local_player_hud.show_win_message(player_winner);
    }

    [Command]
    public void cmd_maybe_shoot_projectile()
    {
        if (fire_t <= 0f)
        {
            if (current_powerup == Powerup_Type.MULTISHOT)
            {
                rpc_spawn_projectile(transform.position, transform.rotation*Quaternion.Euler(0f, 30f, 0f));
                rpc_spawn_projectile(transform.position, transform.rotation);
                rpc_spawn_projectile(transform.position, transform.rotation*Quaternion.Euler(0f, -30f, 0f));
            }
            else
            {
                rpc_spawn_projectile(transform.position, transform.rotation);
            }

            
            fire_t += fire_time;

            rpc_shoot_projectile();
        }
    }

    [ClientRpc]
    public void rpc_shoot_projectile()
    {
        fire_t += fire_time;
    }

    [Command]
    public void cmd_update_movement(Vector3 position, Vector3 direction, Vector3 _dp, float _acceleration, float _friction)
    {
        if (freeze_player) { return; }

        //transform.position = position;
        move_direction = direction;
        acceleration = _acceleration;
        friction = _friction;
        dp = _dp;

        rpc_update_movement(position, direction, _dp, _acceleration, _friction);
    }

    // Makes the client move by its own so we don't need to update its position every frame
    [ClientRpc]
    public void rpc_update_movement(Vector3 position, Vector3 direction, Vector3 _dp, float _acceleration, float _friction)
    {
        if (isLocalPlayer) { return; }
        if (freeze_player) { return; }

        //transform.position = position;
        move_direction = direction;
        acceleration = _acceleration;
        friction = _friction;
        dp = _dp;
    }

    [Command]
    public void cmd_update_rotation(Quaternion rotation)
    {
        if (freeze_player) { return; }

        transform.rotation = rotation;

        rpc_update_rotation(rotation);
    }

    [ClientRpc]
    public void rpc_update_rotation(Quaternion rotation)
    {
        if (isLocalPlayer) { return; }
        if (freeze_player) { return; }

        transform.rotation = rotation;
    }
}
