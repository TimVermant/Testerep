using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightToStair : MonoBehaviour
{
    [SerializeField] GameObject _straight;
    [SerializeField] GameObject _stairs;


    private void Awake()
    {
    }

    public void ShouldBecomeStair(bool stairActive)
    {
        _straight.SetActive(!stairActive);
        _stairs.SetActive(stairActive);
    }

    public GameObject GetRoad()
    {
        return _straight.activeSelf ? _straight : _stairs;
    }
}
