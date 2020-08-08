using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject ballPrefab;
    public GameObject mainCamera;
    
    private GameObject _levelGo;
    
    void Start()
    {
        _levelGo = new GameObject();
        Level levelComponent = _levelGo.AddComponent<Level>();
        levelComponent.Initialize(ballPrefab, mainCamera, 6);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}