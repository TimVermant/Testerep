using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollScript : MonoBehaviour
{
    public float DaysOnTimeLine => _daysOnTimeline;

    [SerializeField] private Renderer _renderer;
    [SerializeField] private RectTransform _indicatorManager;
    [SerializeField] private float _initOffset = .012f;
    private float _secondsPerDay;
    private const float _daysOnTimeline = 4f;
    private float _pixelLengthPerDay;
    private float _timelineLength;

    private void Awake()
    {
        RectTransform rectTransform = transform.parent.GetComponent<RectTransform>();
        _pixelLengthPerDay = rectTransform.sizeDelta.x / _daysOnTimeline;
        _timelineLength = rectTransform.sizeDelta.x;

        GameTime time = FindObjectOfType<GameTime>();
        _secondsPerDay = time.RealtimeMinPerDay * 60; // to x seconds/Day

        Vector2 offset = new(_initOffset, 0);
        _renderer.material.mainTextureOffset += offset;
    }

    void Update()
    {
        if (GameLoop.CurrentGamestate != GameLoop.GameState.Game)
        {
            return;
        }
        float speed = _pixelLengthPerDay / _secondsPerDay;
        Vector2 offset = new(-speed * Time.deltaTime, 0);

        _renderer.material.mainTextureOffset += offset / _timelineLength;
        foreach (RectTransform indicator in _indicatorManager)
        {
            Vector3 pos = indicator.localPosition;
            pos.x += offset.x;
            indicator.localPosition = pos;
        }
    }
}
