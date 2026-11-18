using System.Collections.Generic;
using UnityEngine;

public static class GI
{
    public static PlayerController_Network local_player;
    public static GameMode game_mode;
    public static PlayerHUD local_player_hud;
    public static Powerup_Network powerup;
    public static AudioPlayer audio_player;
    public static List<PlayerController_Network> players_connected;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void game_init()
    {
        players_connected = new List<PlayerController_Network>();
    }
}
