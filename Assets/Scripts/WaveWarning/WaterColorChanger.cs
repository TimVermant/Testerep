using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterColorChanger : MonoBehaviour
{
    [SerializeField] private Material _calmOceanMaterial;
    [SerializeField] private Material _stormyOceanMaterial;
    [SerializeField] private MeshRenderer _waterRenderer;

    [SerializeField] private float _lerpDuration = 5f; //in seconds
    [SerializeField] private float _maxLerp = .99f;
    private float _currentAlpha = 0f;
    private float _desiredAlpha = -1f;

    //debug
    [SerializeField] private bool _useManual = false;
    [Range(0f, 1f)]
    [SerializeField] private float _manualAlpha = 0;

    private void Awake()
    {
        WaveWarning waveWarning = FindObjectOfType<WaveWarning>();
        waveWarning.WarningStateChanged.AddListener(OnWaveWarningChanged);
    }

    private void OnWaveWarningChanged(WaveWarning.WarningState state)
    {
        _desiredAlpha = state == WaveWarning.WarningState.Full
            ? _maxLerp : 0;
    }

    private void Update()
    {
        if (_useManual)
        {
            _waterRenderer.material.Lerp(_calmOceanMaterial, _stormyOceanMaterial, _manualAlpha);
            return;
        }
        if (_currentAlpha == _desiredAlpha)
        {
            return;
        }

        _currentAlpha = Mathf.MoveTowards(_currentAlpha, _desiredAlpha, (_maxLerp / _lerpDuration) * Time.deltaTime);
        _waterRenderer.material.Lerp(_calmOceanMaterial, _stormyOceanMaterial, _currentAlpha);
    }
}
