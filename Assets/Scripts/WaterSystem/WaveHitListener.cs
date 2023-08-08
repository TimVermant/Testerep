using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveHitListener : MonoBehaviour
{
    WaterSystem _waterSystem;

    private void Awake()
    {
        _waterSystem = FindObjectOfType<WaterSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Node"))
        {
            FindObjectOfType<WaterSystem>();
        }
    }
}
