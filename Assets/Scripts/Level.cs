using System;
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
    [FormerlySerializedAs("_ballRadius")] public float _ballScale;
    private Ball[,] _grid;

    public void Initialize(GameObject ballPrefab, float ballScale)
    {
        _ballScale = ballScale;
        _ballPrefab = ballPrefab;
        _grid = new Ball[MAX_GRID_WIDTH, MAX_GRID_HEIGHT];

        for (int y = 0; y < MAX_GRID_HEIGHT; ++y)
        {
            for (int x = 0; x < MAX_GRID_WIDTH; ++x)
            {
                GameObject go = Instantiate(_ballPrefab, Vector3.zero, Quaternion.identity);
                _grid[x, y] = go.GetComponent<Ball>();
                
                go.name = String.Format("Ball[{0}][{1}]", x, y);
                go.transform.localScale = new Vector3(_ballScale, _ballScale, 1);
                
                var extents = go.GetComponent<Renderer>().bounds.extents;
                var ballRadius = extents.x;
                var ballDiameter = ballRadius * 2;

                bool isOddRow = y % 2 == 1;
                var xOffset = isOddRow ? ballRadius : 0;
                var yOffset = (ballRadius / 4) * y;
                go.transform.position = START_OFFSET + new Vector3(x * ballDiameter + xOffset, -y * ballDiameter + yOffset, 0);
            }
        }
    }

    public void Update()
    {
    }
}