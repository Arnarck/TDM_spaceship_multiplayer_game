using UnityEngine;
using TMPro;
using Mirror;

public class PlayerHUD : MonoBehaviour
{
    public GameObject pause_ui;
    public GameObject hud_panel;
    public TextMeshProUGUI lives_text;
    public TextMeshProUGUI win_text;
    public TextMeshProUGUI countdown_text;
    public TextMeshProUGUI player_wins_text;

    [Header("Relay")]
    public GameObject relay_panel;
    public GameObject join_panel;
    public GameObject connecting_panel;
    public TextMeshProUGUI join_code_text;
    public TextMeshProUGUI copy_join_code_text;

    [Header("INTERNAL")]
    public int player_1_wins;
    public int player_2_wins;
    public float go_message_t;
    public float start_game_t;


    private void Awake()
    {
        GI.local_player_hud = this;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (go_message_t > 0f)
        {
            go_message_t -= dt;
            if (go_message_t <= 0f)
            {
                countdown_text.enabled = false;
            }
        }

        if (start_game_t > 0f)
        {
            int previous_start_game_t = (int)start_game_t;
            start_game_t -= dt;

            if (previous_start_game_t > (int)start_game_t)
            {
                countdown_text.text = previous_start_game_t.ToString();
            }

            if (start_game_t <= 0f)
            {
                go_message_t = 1f;
                countdown_text.text = "GO!";
            }
        }

    }


    // ===================== RELAY =====================

    public void copy_join_code_to_clipboard()
    {
        GUIUtility.systemCopyBuffer = join_code_text.text;
        copy_join_code_text.text = "Copied!";
    }


    // ===================== GAME =====================

    public void show_pause()
    {
        pause_ui.SetActive(true);
    }

    public void hide_pause()
    {
        pause_ui.SetActive(false);
    }

    public void resume()
    {
        GI.local_player.resume_local_game();
    }

    public void quit()
    {
        if (GI.local_player.isServer)
        {
            CustomNetworkManager.singleton.StopHost();
            NetworkServer.Shutdown();
        }
        else
        {
            CustomNetworkManager.singleton.StopClient();
            NetworkClient.Disconnect();
        }
    }

    public void update_wins(int player)
    {
        if (player == 0) { player_1_wins++; }
        else             { player_2_wins++; }

        player_wins_text.text = "Player 1 wins: " + player_1_wins + "\n\nPlayer 2 wins: " + player_2_wins;
    }

    public void update_lives()
    {
        lives_text.text = "Lives: " + GI.local_player.lives;
    }

    public void show_next_match_countdown()
    {
        countdown_text.text = "";
        countdown_text.enabled = true;
        start_game_t = 3.5f;
    }

    public void show_win_message(int player_winner)
    {
        win_text.enabled = true;
        win_text.text = "Player " + (player_winner + 1) + " Wins";
    }

    public void hide_win_message()
    {
        win_text.enabled = false;
    }

    public void hide_relay_panel()
    {
        relay_panel.SetActive(false);
        hud_panel.SetActive(true);
    }
}
