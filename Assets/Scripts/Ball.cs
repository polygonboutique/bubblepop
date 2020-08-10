using System;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

public class Ball : MonoBehaviour
{
    private static int MAX_VALUE = 2048;
    private static Random rng = new Random();

    private int _value = 2;
    private int _gridX, _gridY;

    public Sprite[] _sprites;
    
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

    bool IsPowerOfTwo(ulong x)
    {
        return (x != 0) && ((x & (x - 1)) == 0);
    }

    public void SetValue(int val)
    {
        Assert.IsTrue(IsPowerOfTwo((ulong) val));
        _value = val;

        AssignSprite();
    }

    public void IncreaseValue()
    {
        _value = Math.Min(MAX_VALUE, _value << 1); // go to next pow, if not bigger than max 
        AssignSprite();
    }

    private void AssignSprite()
    {
        // int maxIndex = (int) Mathf.Log(MAX_VALUE, 2) + 1;
        int index = (int) Mathf.Log(_value, 2);
        gameObject.GetComponent<SpriteRenderer>().sprite = _sprites[index];
    }

    public bool CanMerge(Ball ball)
    {
        return _value == ball._value;
    }

    public int GetValue()
    {
        return _value;
    }

    public bool ReachedMaxValue()
    {
        return _value == MAX_VALUE;
    }
}