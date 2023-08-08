using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayTimerUI : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI _dayTimer;
    [SerializeField] TextMeshProUGUI _hourTimer;
    [SerializeField] private int _minuteIntervalUpdate = 15;

    private void Awake()
    {
        _dayTimer.text = "1";
        _hourTimer.text = "00:00";
    }

    // private void Update()
    // {
    //    int day = _gameTime.Day;
    //    _dayTimer.text = day.ToString();

    //    int hour = _gameTime.Hour;
    //    int minute = _gameTime.Minute;
    //    string timeText = hour.ToString() + ":" + minute.ToString();
    //    _hourTimer.text = timeText; 
    // }


    public void UpdateDayCounter(int day)
    {
        _dayTimer.text = day.ToString();
        _hourTimer.text = "00:00";
    }

    public void UpdateHourCounter(int hour, int minute)
    {
        string hourText = hour.ToString();
        if (hourText.Length == 1)
        {
            hourText = "0" + hourText;
        }

        string minuteText = (minute / _minuteIntervalUpdate * _minuteIntervalUpdate).ToString();
        if (minuteText.Length == 1)
        {
            minuteText = "0" + minuteText;
        }

        string timeText = hourText + ":" + minuteText;
        _hourTimer.text = timeText;
    }

}
