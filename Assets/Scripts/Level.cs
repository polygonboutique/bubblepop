using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using Plane = UnityEngine.Plane;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Level : MonoBehaviour
{
    private const int MAX_GRID_WIDTH = 6;
    private const int MAX_GRID_HEIGHT = 10;

    private Ball[,] _grid;

    private GameObject _ballSpawnerGo;
    private BallSpawner _ballSpawner;

    private GameObject _ballShooterGo;
    private BallShooter _ballShooter;

    private Plane[] _boundsPlanes;

    public void Initialize(GameObject ballPrefab, GameObject mainCamera, float ballSize)
    {
        InitializeBallSpawner(ballPrefab, ballSize);
        InitializeBallShooter(_ballSpawner);
        SetupCamera(mainCamera);

        _grid = new Ball[MAX_GRID_WIDTH, MAX_GRID_HEIGHT];

        int gridHeight = 6;
        for (int y = 0; y < gridHeight; ++y)
        {
            for (int x = 0; x < MAX_GRID_WIDTH; ++x)
            {
                SpawnBallOnGrid(x, y, Ball.GenerateRandomValue());
            }
        }

        ;
        
        _boundsPlanes = new Plane[2];
        _boundsPlanes[0] = new Plane(Vector3.right, 0.0f); // left
        _boundsPlanes[1] = new Plane(Vector3.left, _ballSpawner.GeneratePosition(MAX_GRID_WIDTH, 0).magnitude); // right
    }

    private void SetupCamera(GameObject mainCamera)
    {
        var position = _ballSpawner.GeneratePosition(MAX_GRID_WIDTH / 2, MAX_GRID_HEIGHT / 2 + 1);
        mainCamera.transform.position = position + new Vector3(0, 0, -10);
    }

    private void InitializeBallSpawner(GameObject ballPrefab, float ballScale)
    {
        _ballSpawnerGo = new GameObject();
        _ballSpawner = _ballSpawnerGo.AddComponent<BallSpawner>();
        _ballSpawner.Initialize(ballPrefab, ballScale);
    }

    private void InitializeBallShooter(BallSpawner ballSpawner)
    {
        int widthIndex = MAX_GRID_WIDTH / 2;

        Vector3 currentBallPosition = _ballSpawner.GeneratePosition(widthIndex, MAX_GRID_HEIGHT);
        Vector3 nextBallPosition = _ballSpawner.GeneratePosition(widthIndex - 2, MAX_GRID_HEIGHT + 1);

        _ballShooterGo = new GameObject();
        _ballShooter = _ballShooterGo.AddComponent<BallShooter>();
        _ballShooter.Initialize(ballSpawner, currentBallPosition, nextBallPosition);
    }

    private void SpawnBallOnGrid(int x, int y, int value)
    {
        _grid[x, y] = _ballSpawner.SpawnBallOnGrid(x, y, value).GetComponent<Ball>();
    }

    private void DestroyBallOnGrid(int x, int y)
    {
        Destroy(_grid[x, y].gameObject);
    }


    private bool HitBalls(Ray ray, out GameObject gameObject)
    {
        // RaycastHit hitInfo;
        for (int y = 0; y < MAX_GRID_HEIGHT; ++y) {
            for (int x = 0; x < MAX_GRID_WIDTH; ++x)
            {
                if (!_grid[x, y])
                {
                    continue;
                }

                GameObject go = _grid[x, y].gameObject;

                if (go.GetComponent<Collider>().Raycast(ray, out RaycastHit hitInfo, 100.0f))
                {
                    Debug.Log(hitInfo);
                    Debug.Log(go);
                    Debug.Log("----");
                }

            }
        }
        
        gameObject = null;
        return false;
    }

    void Update()
    {
        Vector3 mouseCoordsWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseCoordsWorldSpace.z = 0;
        Vector3 shootDirection = (mouseCoordsWorldSpace - _ballShooter.GetPosition()).normalized;
        Ray ray = new Ray(_ballShooter.GetPosition(), shootDirection);
        
        GameObject hitBall;
        if(!HitBalls(ray, out hitBall))
        {
            // Hit planes
            Color32[] planeColor = new Color32[2];
            planeColor[0] = new Color32(255, 0, 255, 255);
            planeColor[1] = new Color32(0, 255, 0, 255);
        
            for (int i = 0; i < _boundsPlanes.Length; ++i)
            {
                Plane plane = _boundsPlanes[i];
            
                Color col = new Color32(255, 0, 22, 255);
                Debug.DrawLine(_ballShooter.GetPosition(), _ballShooter.GetPosition() + shootDirection * 100, col);

                Vector3 planeOrigin = -plane.normal * plane.distance;
                Debug.DrawLine(planeOrigin, planeOrigin + Vector3.down * 200, planeColor[i]);
                Debug.DrawLine(planeOrigin, planeOrigin + Vector3.up * 200, planeColor[i]);

                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                
                    _ballShooter.GetCurrentBall().transform.position = hitPoint;
                
                    Vector3 reflect = Vector3.Reflect(ray.direction, plane.normal);
                    Debug.DrawLine(hitPoint, hitPoint + reflect * 100, col);
                
                    HitBalls(ray, out hitBall);
                }
            }
        }
        
        if (hitBall)
        {
            Debug.Log("Hit a ball, trace ray with tweens, until we reach end!\n");
                
            if (Input.GetButtonDown("Fire1"))
            {
                _ballShooter.ShootBall(shootDirection);
                _ballShooter.ReloadBall();
            }
        }
    }
}