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
            GI.game_mode.add_connection_id_t = 1f;
            GI.game_mode.init_projectile_pool_t = 2f;
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        GI.game_mode.load_main_menu();
    }
}
