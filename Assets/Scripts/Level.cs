using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private const int MAX_GRID_WIDTH = 6;
    private const int MAX_GRID_HEIGHT = 10;
    private Vector3 START_OFFSET = new Vector3(0, 0, 0);
    private GameObject _ballPrefab;
    private Ball[,] _grid;

    public static float radius;

    public void Initialize(GameObject ballPrefab)
    {
        _ballPrefab = ballPrefab; 
        _grid = new Ball[MAX_GRID_WIDTH, MAX_GRID_HEIGHT];

        // _grid[0, 0].gameObject;

        for (int y = 0; y < MAX_GRID_HEIGHT; ++y) {
            for (int x = 0; x < MAX_GRID_WIDTH; ++x)
            {
                Vector3 position = START_OFFSET + new Vector3(x * radius * 2, y * radius * 2); // take grid into account
                
                GameObject go = Instantiate(_ballPrefab, position, Quaternion.identity);
                go.name = String.Format("Ball[{0}][{1}]", x, y);
                _grid[x, y] = go.GetComponent<Ball>();
            }
        }
    }

    public void Update()
    {
    }
}