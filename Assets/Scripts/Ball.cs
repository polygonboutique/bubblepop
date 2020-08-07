﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private static int MAX_VALUE = 1024;
    private static Color[] COLORS = new Color[(int) Mathf.Log(MAX_VALUE, 2)];

    private int value = 2;

    static Ball()
    {
        // todo: init colors
        for (int i = 0; i < COLORS.Length; ++i)
        {
            float lerp = (float)(i / COLORS.Length);
            COLORS[i].r = lerp;
            COLORS[i].g = Math.Max(lerp * 2 -1.0f, 0.0f);
            COLORS[i].b = 1.0f - lerp;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void IncreaseValue()
    {
        value = Math.Min(MAX_VALUE, value << 1); // go to next pow, if not bigger than max 
        AssignColor();
    }

    private void AssignColor()
    {
        gameObject.GetComponent<SpriteRenderer>().color = COLORS[value];
    }
}