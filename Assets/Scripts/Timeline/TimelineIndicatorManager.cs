using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Testerep.Events;

public class TimelineIndicatorManager : MonoBehaviour
{
    [System.Serializable]
    private class EventSprite
    {
        public EventType EventType;
        public Sprite Sprite;
    }

    [SerializeField] private RectTransform _timelineRect;
    [SerializeField] private RectTransform _iconParent;
    [SerializeField] private GameObject _indicatorPrefab;

    [SerializeField] private Sprite _defaultIconSprite;
    [SerializeField] private EventSprite[] _eventSprites;
    [SerializeField] private GameObject _waveTooltip;

    private float _timelineWidthHalved;
    private float _hoursOnTimelineHalved;
    private float _indicatorWidthHalved;

    public void ShowWaveTooltip()
    {
        if(_waveTooltip && !_waveTooltip.activeSelf)
        {
            _waveTooltip.SetActive(true);
        }
    }

    private void Awake()
    {
        _timelineWidthHalved = _timelineRect.sizeDelta.x / 2f;
        _indicatorWidthHalved = _indicatorPrefab.GetComponent<RectTransform>().sizeDelta.x / 2;

        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        eventSystem.EventQueued.AddListener(OnEventQueued);

        ScrollScript scrollScript = FindObjectOfType<ScrollScript>();
        _hoursOnTimelineHalved = scrollScript.DaysOnTimeLine * GameTime.HoursInDay / 2f;
    }

    private void Update()
    {
        foreach (Transform child in _iconParent)
        {
            if (child.localPosition.x < -_timelineWidthHalved - _indicatorWidthHalved)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void OnEventQueued(EventType eventType, int hoursUntil)
    {
        Sprite sprite = FindIconSprite(eventType);
        float pos = FindIconPosition(hoursUntil);
        PlaceIcon($"{eventType}-icon", sprite, pos);
    }

    private void PlaceIcon(string name, Sprite sprite, float xPos)
    {
        GameObject indicator = Instantiate(_indicatorPrefab, _iconParent);
        indicator.name = name;
        indicator.GetComponent<Image>().sprite = sprite;

        indicator.transform.localPosition = new(xPos, 0, 0);
    }

    private float FindIconPosition(int hoursUntil)
    {
        return hoursUntil / _hoursOnTimelineHalved * _timelineWidthHalved;
    }

    private Sprite FindIconSprite(EventType eventType)
    {
        return _eventSprites.FirstOrDefault(eventSprite => eventSprite.EventType == eventType)?.Sprite ?? _defaultIconSprite;
    }


}
