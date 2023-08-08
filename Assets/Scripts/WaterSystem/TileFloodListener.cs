using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TileFloodListener : MonoBehaviour
{
    public UnityEvent<int> FloodedHourPassed;
    public UnityEvent<int> FloodedDayPassed;

    public bool IsFlooded => _hoursPassed > -1;
    public int HoursFlooded => IsFlooded ? _hoursPassed : 0;
    public int DaysFlooded => HoursFlooded / NrHoursInDay;

    private static readonly int NrHoursInDay = 24;
    private int _hoursPassed = -1;

    private void OnDestroy()
    {
        GameTime timer = FindObjectOfType<GameTime>();
        if (timer != null)
        {
            timer.OnHour.RemoveListener(OnTimerHour);
        }
    }

    public void OnTileFlooded()
    {
        GameTime timer = FindObjectOfType<GameTime>();
        timer.OnHour.AddListener(OnTimerHour);
    }

    private void OnTimerHour(int hour, int day)
    {
        _hoursPassed++;

        //invoke hour passed
        FloodedHourPassed.Invoke(_hoursPassed);

        //invoke day passed
        if (_hoursPassed % NrHoursInDay == 0)
        {
            FloodedDayPassed.Invoke(_hoursPassed / NrHoursInDay);
        }
    }
}
