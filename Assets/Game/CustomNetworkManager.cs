using UnityEngine;
using Mirror;
using Utp;

public class CustomNetworkManager : RelayNetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // All players connected
        if (NetworkServer.connections.Count == 2)
        {
            for (int i = 0; i < NetworkServer.connections.Count; i++)
            {
                PlayerController_Network player = NetworkServer.connections[i].identity.GetComponent<PlayerController_Network>();

                // Connection ID
                player.connection_id = player.connectionToClient.connectionId;

                // Spawn projectiles
                for (int j = 0; j < 30; j++)
                {
                    GameObject go = Instantiate(player.projectile_prefab);
                    go.GetComponent<Projectile_Network>().connection_id = player.connectionToClient.connectionId;

                    NetworkServer.Spawn(go, player.connectionToClient);
                }
            }

            GI.game_mode.start_game();
        }
    }
}
