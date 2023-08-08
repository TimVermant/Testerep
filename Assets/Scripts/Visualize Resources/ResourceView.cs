using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResourceView : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private GameObject _imagesOfResource;

    public float _lifeTime { private get; set;} = 0;
    [SerializeField] float _maxTime = 3;
    public void SetText(string txt)
    {
        _text.text = txt;
    }
    public void SetColor(Color color)
    {
        _text.color = color;
    }
    public void SetImage(Sprite resource)
    {
        _imagesOfResource.GetComponent<Image>().sprite = resource;
    }

    private void Update()
    {
        if(_maxTime >= _lifeTime)
        {
            _lifeTime += Time.deltaTime;
        }
        if(_maxTime <= _lifeTime)
        {
           gameObject.SetActive(false);
        }
    }
}
