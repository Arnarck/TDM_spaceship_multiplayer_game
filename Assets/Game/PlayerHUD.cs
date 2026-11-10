using UnityEngine;
using System;
using Utp;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class PlayerHUD : MonoBehaviour
{
    public GameObject hud_panel;
    public TextMeshProUGUI lives_text;
    public TextMeshProUGUI win_text;
    public TextMeshProUGUI countdown_text;

    [Header("Relay")]
    public GameObject relay_panel;
    public TMP_InputField input_field;
    public RelayNetworkManager relay_network_manager;

    [Header("INTERNAL")]
    public float go_message_t;
    public float start_game_t;


    private void Awake()
    {
        GI.local_player_hud = this;
    }

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Logged into Unity, player ID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
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

    public void host()
    {
        relay_network_manager.StartRelayHost(2);
    }

    public void join_game()
    {
        relay_network_manager.JoinRelayServer();
    }

    public void get_join_code()
    {
        relay_network_manager.relayJoinCode = input_field.text;
    }


    // ===================== GAME =====================

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
