using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections.Generic;

public class GameMode : NetworkBehaviour
{
    public GameObject powerup_prefab;
    public Transform[] projectiles_parent;
    public Vector3 screen_bounds;

    [Header("INTERNAL")]
    public float start_match_t;
    public float init_projectile_pool_t;
    public float game_over_t;
    public float add_connection_id_t;

    void Awake()
    {
        GI.game_mode = this;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (!isServer) { return; }

        if (add_connection_id_t > 0f)
        {
            add_connection_id_t -= dt;
            if (add_connection_id_t <= 0f)
            {
                for (int i = 0; i < NetworkServer.connections.Count; i++)
                {
                    PlayerController_Network player = NetworkServer.connections[i].identity.GetComponent<PlayerController_Network>();
                    player.connection_id = player.connectionToClient.connectionId;
                }
            }
        }

        if (init_projectile_pool_t > 0f)
        {
            init_projectile_pool_t -= dt;
            if (init_projectile_pool_t <= 0f)
            {
                for (int i = 0; i < NetworkServer.connections.Count; i++)
                {
                    PlayerController_Network player = NetworkServer.connections[i].identity.GetComponent<PlayerController_Network>();

                    // Spawn projectiles for each player
                    for (int j = 0; j < 30; j++)
                    {
                        GameObject go = Instantiate(player.projectile_prefab);
                        go.GetComponent<Projectile_Network>().connection_id = player.connectionToClient.connectionId;

                        NetworkServer.Spawn(go, player.connectionToClient);
                    }
                }

                start_game();
            }
        }

        if (start_match_t > 0f)
        {
            start_match_t -= dt;
            if (start_match_t <= 0f)
            {
                start_match();
            }
        }

        if (game_over_t > 0f)
        {
            game_over_t -= dt;
            if (game_over_t <= 0f)
            {
                start_match_t = 4f;
                for (int i = 0; i < NetworkServer.connections.Count; i++)
                {
                    PlayerController_Network player = NetworkServer.connections[i].identity.GetComponent<PlayerController_Network>();
                    player.rpc_prepare_for_next_match(NetworkManager.startPositions[i].position);
                }
            }
        }
    }

    public void load_main_menu()
    {
        SceneManager.LoadScene(1);
    }

    [Server]
    public void start_game()
    {
        for (int i = 0; i < NetworkServer.connections.Count; i++)
        {
            PlayerController_Network player = NetworkServer.connections[i].identity.GetComponent<PlayerController_Network>();
            player.target_start_game();
        }

        // Spawn powerup
        GameObject powerup_object = Instantiate(powerup_prefab);
        NetworkServer.Spawn(powerup_object, GI.local_player.gameObject);

        start_match_t = 4f;
    }

    [Server]
    public void start_match()
    {
        for (int i = 0; i < NetworkServer.connections.Count; i++)
        {
            PlayerController_Network player = NetworkServer.connections[i].identity.GetComponent<PlayerController_Network>();
            player.rpc_start_match();
        }

        // Activates the powerup after some time
        GI.powerup.activate_powerup_t = 4f;
    }

    [Server]
    public void game_over(int player_defeated_id)
    {
        int player_winner = 0;
        if (player_defeated_id == 0) { player_winner = 1; }

        game_over_t = 2f;

        // Block players' movement
        for (int i = 0; i < NetworkServer.connections.Count; i++)
        {
            PlayerController_Network player = NetworkServer.connections[i].identity.GetComponent<PlayerController_Network>();
            player.rpc_game_over(player_winner);
        }

        // Forces powerup to disable
        GI.powerup.activate_powerup_t = 0f;
        GI.local_player.rpc_hide_powerup();
    }
}
