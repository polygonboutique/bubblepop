﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Level : MonoBehaviour
{
    private const int MAX_GRID_WIDTH = 6;
    private const int MAX_GRID_HEIGHT = 10;
    private Vector3 START_OFFSET = new Vector3(0, 0, 0);
    private GameObject _ballPrefab;
    private Ball[,] _grid;

    private GameObject _ballShooterGo;
    private float _ballScale;
    private float _ballRadius;
    private float _ballDiameter;

    private BallSpawner _ballSpawner;

    public void Initialize(GameObject ballPrefab, float ballSize)
    {
        _ballScale = ballSize;
        _ballPrefab = ballPrefab;
        _grid = new Ball[MAX_GRID_WIDTH, MAX_GRID_HEIGHT];
        
        var extents = _ballPrefab.GetComponent<Renderer>().bounds.extents;
        _ballPrefab.transform.localScale = new Vector3(_ballScale, _ballScale, 1);

       _ballRadius = extents.x;
       _ballDiameter = _ballRadius * 2;

        // todo: proper load level function here.
        int gridHeight = 6;
        for (int y = 0; y < gridHeight; ++y)
        {
            for (int x = 0; x < MAX_GRID_WIDTH; ++x)
            {
                SpawnBallOnGrid(x, y, 2);
            }
        }

        InitializeBallShooter();
    }

    private void InitializeBallShooter()
    {
        _ballShooterGo = new GameObject();
        _ballShooterGo.AddComponent<BallShooter>();
        
        
        // calculate ball shooter position

    }
    
    private void SpawnBallOnGrid(int x, int y, int value)
    {
        GameObject go = Instantiate(_ballPrefab, Vector3.zero, Quaternion.identity);
        go.name = String.Format("Ball[{0}][{1}]", x, y);
        go.transform.position = GeneratePosition(x, y);
        _grid[x, y] = go.GetComponent<Ball>();
        _grid[x, y].SetValue(value);
    }

    private void DestroyBallOnGrid(int x, int y)
    {
        Destroy(_grid[x, y].gameObject);
    }
    
    private Vector3 GeneratePosition(int x, int y)
    {
        bool isOddRow = y % 2 == 1;
        var xOffset = isOddRow ? _ballRadius : 0;
        var yOffset = (_ballRadius / 4) * y;
        return START_OFFSET + new Vector3(x * _ballDiameter + xOffset, -y * _ballDiameter + yOffset, 0);
    }

    public void Update()
    {
    }
}