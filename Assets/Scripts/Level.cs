using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Level : MonoBehaviour
{
    private const int MAX_GRID_WIDTH = 6;
    private const int MAX_GRID_HEIGHT = 10;
    
    private Ball[,] _grid;

    private GameObject _ballSpawnerGo;
    private BallSpawner _ballSpawner;
    
    private GameObject _ballShooterGo;
    private BallShooter _ballShooter;

    public void Initialize(GameObject ballPrefab, float ballSize)
    {
        InitializeBallSpawner(ballPrefab, ballSize);
        InitializeBallShooter();
        
        _grid = new Ball[MAX_GRID_WIDTH, MAX_GRID_HEIGHT];
        
        int gridHeight = 6;
        for (int y = 0; y < gridHeight; ++y)
        {
            for (int x = 0; x < MAX_GRID_WIDTH; ++x)
            {
                SpawnBallOnGrid(x, y, 2);
            }
        }
    }

    private void InitializeBallSpawner(GameObject ballPrefab, float ballScale)
    {
        _ballSpawnerGo = new GameObject();
        _ballSpawner = _ballSpawnerGo.AddComponent<BallSpawner>();
        _ballSpawner.Initialize(ballPrefab, ballScale);
    }

    private void InitializeBallShooter()
    {
        _ballShooterGo = new GameObject();
        _ballShooter = _ballShooterGo.AddComponent<BallShooter>();
        _ballShooter.Initialize();


        // calculate ball shooter position
    }
    
    private void SpawnBallOnGrid(int x, int y, int value)
    {
        _grid[x, y] = _ballSpawner.SpawnBallOnGrid(x, y).GetComponent<Ball>();
        _grid[x, y].SetValue(value);
    }

    private void DestroyBallOnGrid(int x, int y)
    {
        Destroy(_grid[x, y].gameObject);
    }
    
  

    public void Update()
    {
    }
}