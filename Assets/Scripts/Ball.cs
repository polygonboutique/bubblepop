using System;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

public class Ball : MonoBehaviour
{
    private static int MAX_VALUE = 2048;
    private static Color[] COLORS = new Color[(int) Mathf.Log(MAX_VALUE, 2) + 1];
    private static Random rng = new Random(0);

    private int _value = 2;
    private bool _active = false;
    private int _gridX, _gridY;

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
        COLORS[10] = new Color32(64, 255, 0, 255);
        COLORS[11] = new Color32(0, 0, 0, 255);
    }

    public static int GenerateCappedRandomValue()
    {
        int maxExp = (int) Mathf.Log(Ball.MAX_VALUE, 2) / 2;
        int randomExp = rng.Next(1, maxExp);
        return (int) Math.Pow(2, randomExp);
    }

    public void SetGridCoords(int x, int y)
    {
        _gridX = x;
        _gridY = y;
    }

    public int GetGridXCoord()
    {
        return _gridX;
    }

    public int GetGridYCoord()
    {
        return _gridY;
    }

    public void SetActive()
    {
        _active = true;
    }

    public bool IsActive()
    {
        return _active;
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
        int nextValue = _value << 1;
        _value = Math.Min(MAX_VALUE, nextValue); // go to next pow, if not bigger than max 
        AssignColor();
    }

    private void AssignColor()
    {
        int index = (int) Mathf.Log(_value, 2);
        
        Debug.Log(index);
        gameObject.GetComponent<SpriteRenderer>().color = COLORS[index];
    }

    public bool CanMerge(Ball ball)
    {
        return _value == ball._value;
    }

    public int GetValue()
    {
        return _value;
    }
}