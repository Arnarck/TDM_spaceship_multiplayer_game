using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public CustomNetworkManager relay_network_manager;
    public TMP_InputField input_field;
    public GameObject main_menu;
    public GameObject join_game_menu;
    public Button join_game_button;

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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void host_game()
    {
        relay_network_manager.StartRelayHost(2);
    }

    public void go_to_join_menu()
    {
        join_game_menu.SetActive(true);

        join_game_button.interactable = false;
        if (input_field.text != "" && input_field.text != null)
        {
            join_game_button.interactable = true;
        }
    }

    public void go_to_main_menu()
    {
        join_game_menu.SetActive(false);
        main_menu.SetActive(true);
    }

    public void join_game()
    {
        relay_network_manager.JoinRelayServer();
    }

    public void get_join_code()
    {
        relay_network_manager.relayJoinCode = input_field.text;
        join_game_button.interactable = false;
        if (input_field.text != "" && input_field.text != null)
        {
            join_game_button.interactable = true;
        }
    }

    public void quit_game()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
