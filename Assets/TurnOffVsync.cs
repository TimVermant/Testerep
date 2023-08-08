using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffVsync : MonoBehaviour
{
    [SerializeField] private bool vSync = false;
    private void Awake() 
    {
        if (!vSync)
        {
            Application.targetFrameRate = -1;
        }
    }
}
