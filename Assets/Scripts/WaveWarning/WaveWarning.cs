using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EventSystem = Testerep.Events.EventSystem;

public class WaveWarning : MonoBehaviour
{
    public enum WarningState
    {
        None, Medium, Full
    }

    public WarningState CurrentState => _currentState;
    public UnityEvent<WarningState> WarningStateChanged;
    public bool AutoChangeState = true;

    private EventSystem _eventSystem;
    private WarningState _currentState;

    private void Awake()
    {
        _eventSystem = FindObjectOfType<EventSystem>();
        _eventSystem.EventAlarm1.AddListener(OnEventQueueAlarm1);
        _eventSystem.EventAlarm2.AddListener(OnEventQueueAlarm2);
        _eventSystem.EventQueued.AddListener(OnEventQueued);

        WaterSystem waterSystem = FindObjectOfType<WaterSystem>();
        waterSystem.WaveDone.AddListener(OnFloodDone);

        ActivateState(WarningState.None);
    }

    private void OnEventQueued(EventType eventType, int hoursUntil)
    {
        if (!AutoChangeState || !EventSystem.IsStormEvent(eventType))
        {
            return;
        }
        WarningState desiredState = GetWarningStateFor(hoursUntil);
        TryActivateState(desiredState);
    }

    private void OnEventQueueAlarm1(EventType eventType)
    {
        if (!AutoChangeState || !EventSystem.IsStormEvent(eventType))
        {
            return;
        }
        TryActivateState(WarningState.Medium);
    }

    private void OnEventQueueAlarm2(EventType eventType)
    {
        if (!AutoChangeState || !EventSystem.IsStormEvent(eventType))
        {
            return;
        }
        TryActivateState(WarningState.Full);
    }

    private void OnFloodDone()
    {
        if (!AutoChangeState)
        {
            return;
        }
        ActivateState(GetUpcomingWaveWarningState());
    }

    private void TryActivateState(WarningState state)
    {
        if (state > _currentState)
        {
            ActivateState(state);
        }
    }

    public void ActivateState(WarningState state)
    {
        _currentState = state;
        WarningStateChanged.Invoke(_currentState);
    }

    private WarningState GetUpcomingWaveWarningState()
    {
        int hoursUntilWave = _eventSystem.GetHoursUntilUpcomingStorm();
        return GetWarningStateFor(hoursUntilWave);
    }

    private WarningState GetWarningStateFor(int hoursUntil)
    {
        if (hoursUntil <= 0 || hoursUntil > _eventSystem.Alarm1Hours)
        {
            return WarningState.None;
        }
        if (hoursUntil <= _eventSystem.Alarm2Hours)
        {
            return WarningState.Full;
        }
        return WarningState.Medium;
    }
}
