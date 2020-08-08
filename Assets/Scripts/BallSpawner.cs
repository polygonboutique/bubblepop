using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    private Vector3 START_OFFSET = new Vector3(0, 0, 0);
    private GameObject _ballPrefab;
    private float _ballScale;
    private float _ballRadius;
    private float _ballDiameter;
    
    public void Initialize(GameObject ballPrefab, float ballScale)
    {
        _ballScale = ballScale;
        _ballPrefab = ballPrefab;
        
        var extents = _ballPrefab.GetComponent<Renderer>().bounds.extents;
        _ballPrefab.transform.localScale = new Vector3(_ballScale, _ballScale, 1);

        _ballRadius = extents.x;
        _ballDiameter = _ballRadius * 2;
    }

    public GameObject SpawnBall(Vector3 position, int value)
    {
        GameObject go = Instantiate(_ballPrefab, position, Quaternion.identity);
        go.name = "Ball";
        go.GetComponent<Ball>().SetValue(value);
        return go;
    }

    public GameObject SpawnBallOnGrid(int x, int y, int value)
    {
        GameObject go = Instantiate(_ballPrefab, GeneratePosition(x, y), Quaternion.identity);
        go.name = String.Format("Ball[{0}][{1}]", x, y);
        go.GetComponent<Ball>().SetValue(value);
        return go;
    }
    
    public Vector3 GeneratePosition(int x, int y)
    {
        bool isOddRow = y % 2 == 1;
        var xOffset = isOddRow ? _ballRadius : 0;
        var yOffset = (_ballRadius / 4) * y;
        return START_OFFSET + new Vector3(x * _ballDiameter + xOffset, -y * _ballDiameter + yOffset, 0);
    }

    public float GetBallRadius()
    {
        return _ballRadius;
    }
}
