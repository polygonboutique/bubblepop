using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

public class Ball : MonoBehaviour
{
    private static int MAX_VALUE = 1024;
    private static Color[] COLORS = new Color[(int) Mathf.Log(MAX_VALUE, 2)];
    private static Random rng = new Random(0);
    
    private int _value = 2;

    static Ball()
    {
        COLORS[0] = new Color32(0, 0, 0, 255);
        COLORS[1] = new Color32(255, 97, 199, 255);
        COLORS[2] = new Color32(58, 248, 255, 255);
        COLORS[3] = new Color32(255, 255, 119, 255);
        COLORS[4] = new Color32(143, 61, 255, 255);
        COLORS[5] = new Color32(125, 255, 167, 255);
        COLORS[6] = new Color32(255, 154, 43, 255);
        COLORS[7] = new Color32(255, 26, 74, 255);
        COLORS[8] = new Color32(126, 104, 255, 255);
        COLORS[9] = new Color32(229, 58, 255, 255);
    }
    
    public static int GenerateRandomValue()
    {
        int maxExp = (int) Mathf.Log(Ball.MAX_VALUE, 2);
        int randomExp = rng.Next(1, maxExp);
        return (int) Math.Pow(2, randomExp);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    bool IsPowerOfTwo(ulong x)
    {
        return (x != 0) && ((x & (x - 1)) == 0);
    }

    public void SetValue(int val)
    {
        Assert.IsTrue(IsPowerOfTwo((ulong) val));
        _value = val;
        
        AssignColor();
    }

    public void IncreaseValue()
    {
        _value = Math.Min(MAX_VALUE, _value << 1); // go to next pow, if not bigger than max 
        AssignColor();
    }

    private void AssignColor()
    {
        int index = (int) Mathf.Log(_value, 2);
        gameObject.GetComponent<SpriteRenderer>().color = COLORS[index];
    }
}