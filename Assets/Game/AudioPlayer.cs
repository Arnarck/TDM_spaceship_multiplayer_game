using UnityEngine;


public enum Audio_Type
{
    PLAYER_HIT,
    PLAYER_DEATH,
    PLAYER_SHOOTING,
    POWERUP_COLLECTED,
    POWERUP_SPAWNED,

    COUNT
}



public class AudioPlayer : MonoBehaviour
{
    public AudioSource[] audios;

    private void Awake()
    {
        GI.audio_player = this;
    }

    public void play(Audio_Type audio_type)
    {
        audios[(int)audio_type].Play();
    }
}
