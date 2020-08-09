﻿using System;
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
        InGame inGameComponent = _levelGo.AddComponent<InGame>();
        inGameComponent.Initialize(ballPrefab, mainCamera, 6);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}