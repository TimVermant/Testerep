using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Rain : MonoBehaviour
{
    [SerializeField] private VisualEffect _visualEffect;

    private void Awake()
    {
        WaveWarning waveWarning = FindObjectOfType<WaveWarning>();
        waveWarning.WarningStateChanged.AddListener(OnWarningStateChanged);
    }

    private void OnWarningStateChanged(WaveWarning.WarningState warningState)
    {
        switch (warningState)
        {
            case WaveWarning.WarningState.None: StopRain(); break;
            case WaveWarning.WarningState.Medium: StartRain(); break;
            case WaveWarning.WarningState.Full: StartRain(); break;
        }
    }

    public bool IsRaining()
    {
        return false;
    }

    public void StartRain()
    {
        if (IsRaining())
        {
            return;
        }
        _visualEffect.Play();

        //Play Ambient Music
        SoundManager.Instance.GetAmbientMusic(SoundManager.AmbientMusic.storm);
    }

    public void StopRain()
    {
       
        _visualEffect.Stop();
        SoundManager.Instance.GetAmbientMusic(SoundManager.AmbientMusic.maingame);
    }
}
