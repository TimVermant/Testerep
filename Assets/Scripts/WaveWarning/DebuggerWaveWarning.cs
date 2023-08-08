using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSystem = Testerep.Events.EventSystem;

public class DebuggerWaveWarning : MonoBehaviour
{
    private WaveWarning _waveWarning;
    private EventSystem _eventSystem;

    [Header("Input")]
    [SerializeField] private bool _autoChangeState = true;
    [SerializeField] private bool _setState = false;
    [SerializeField] private WaveWarning.WarningState _warningState;

    [Header("ReadOnly")]
    [SerializeField] private WaveWarning.WarningState _currentState;
    [SerializeField] private int _hoursUntilStorm;

    private void Awake()
    {
        _waveWarning = FindObjectOfType<WaveWarning>();
        _eventSystem = FindObjectOfType<EventSystem>();
    }

    private void Update()
    {
        _waveWarning.AutoChangeState = _autoChangeState;

        if(_setState)
        {
            _setState = false;
            _waveWarning.ActivateState(_warningState);
        }

        _currentState = _waveWarning.CurrentState;
        _hoursUntilStorm = _eventSystem.GetHoursUntilUpcomingStorm();
    }
}
