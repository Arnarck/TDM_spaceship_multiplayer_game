using System.Collections.Generic;

public static class GI
{
    public static PlayerController_Network local_player;
    public static GameMode game_mode;
    public static PlayerHUD local_player_hud;
    public static Powerup_Network powerup;
    public static List<PlayerController_Network> players_connected = new List<PlayerController_Network>();
}
