using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
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

        _boundsPlanes = new Plane[2];
        _boundsPlanes[0] = new Plane(Vector3.right, 0.0f); // left
        _boundsPlanes[1] = new Plane(Vector3.left, _ballSpawner.GeneratePosition(MAX_GRID_WIDTH, 0).magnitude
                                                   - _ballSpawner.GetBallRadius()); // right
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

    private bool HitBalls(Ray ray, out GameObject outGameObject, out RaycastHit hitInfo)
    {
        float closestDistance = Single.PositiveInfinity;
        bool found = false;
        outGameObject = null;
        hitInfo = new RaycastHit();

        for (int y = 0; y < MAX_GRID_HEIGHT; ++y)
        {
            for (int x = 0; x < MAX_GRID_WIDTH; ++x)
            {
                if (!_grid[x, y])
                {
                    continue;
                }

                GameObject go = _grid[x, y].gameObject;
                if (go.GetComponent<Collider>().Raycast(ray, out hitInfo, 1000.0f))
                {
                    if (hitInfo.distance < closestDistance)
                    {
                        closestDistance = hitInfo.distance;
                        outGameObject = go;
                        found = true;
                    }
                }
            }
        }

        return found;
    }

    void Update()
    {
        // check, if can shoot
        // else, tween 
        //  transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime);


        Vector3 mouseCoordsWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseCoordsWorldSpace.z = 0;
        Vector3 shootDirection = (mouseCoordsWorldSpace - _ballShooter.GetPosition()).normalized;
        Ray ray = new Ray(_ballShooter.GetPosition(), shootDirection);

        Color shootDirColor = new Color32(255, 0, 22, 255);
        Debug.DrawLine(_ballShooter.GetPosition(), _ballShooter.GetPosition() + shootDirection * 100, shootDirColor);

        GameObject hitBall;
        RaycastHit hitInfo;
        if (!HitBalls(ray, out hitBall, out hitInfo))
        {
            // Hit planes
            Color32[] planeColor = new Color32[2];
            planeColor[0] = new Color32(255, 0, 255, 255);
            planeColor[1] = new Color32(0, 255, 0, 255);

            for (int i = 0; i < _boundsPlanes.Length; ++i)
            {
                Plane plane = _boundsPlanes[i];

                Vector3 planeOrigin = -plane.normal * plane.distance;
                Debug.DrawLine(planeOrigin, planeOrigin + Vector3.down * 200, planeColor[i]);
                Debug.DrawLine(planeOrigin, planeOrigin + Vector3.up * 200, planeColor[i]);

                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);

                    Vector3 reflect = Vector3.Reflect(ray.direction, plane.normal);
                    Debug.DrawLine(hitPoint, hitPoint + reflect * 200, shootDirColor);

                    {
                        Vector3 tangent = Vector3.Cross(plane.normal, Vector3.forward);

                        if (tangent.magnitude == 0)
                        {
                            tangent = Vector3.Cross(plane.normal, Vector3.up);
                        }

                        {
                            Color color = new Color32(255, 255, 22, 255);
                            Debug.DrawLine(hitPoint, hitPoint + tangent * 200, color);
                        }

                        {
                            Color color = new Color32(0, 0, 225, 255);
                            Debug.DrawLine(hitPoint, hitPoint + plane.normal * 200, color);
                        }
                    }


                    Ray nextRay = new Ray(hitPoint, reflect);
                    HitBalls(nextRay, out hitBall, out hitInfo);
                }
            }
        }

        if (hitBall)
        {
            {
                Vector3 tangent = Vector3.Cross(hitInfo.normal, Vector3.forward);

                if (tangent.magnitude == 0)
                {
                    tangent = Vector3.Cross(hitInfo.normal, Vector3.up);
                }

                {
                    Color color = new Color32(255, 255, 22, 255);
                    var position = hitInfo.point;
                    Debug.DrawLine(position, position + tangent * 200, color);
                }

                {
                    Color color = new Color32(0, 0, 225, 255);
                    var position = hitInfo.point;
                    Debug.DrawLine(position, position + hitInfo.normal * 200, color);
                }
            }

            var hitBallComponent = hitBall.GetComponent<Ball>();
            if (PlaceOnGrid(hitBallComponent.GetGridXCoord(), hitBallComponent.GetGridYCoord(),
                hitInfo.normal, out var gridX, out var gridY))
            {
                _ballShooter.GetCurrentBall().transform.position = _ballSpawner.GeneratePosition(gridX, gridY);

                // todo: create list of points we need to visit
                // todo: visit path; end of path => try merge


                if (Input.GetButtonDown("Fire1"))
                {
                    _ballShooter.ShootBall(shootDirection); // kick off tween animation
                    _ballShooter.ReloadBall();

                    // todo: don't forget to do bounds checks for grid.
                    SpawnBallOnGrid(gridX, gridY, Ball.GenerateRandomValue());
                }
            }
        }
    }

    private bool IsInRange(float min, float max, float value)
    {
        return min <= value && max >= value;
    }

    private bool CoordinatesInRange(int x, int y)
    {
        return x >= 0 && x < MAX_GRID_WIDTH && y >= 0 && y < MAX_GRID_HEIGHT;
    }

    private bool CoordinatesOccupied(int x, int y)
    {
        if (_grid[x, y])
        {
            return false;
        }

        return true;
    }

    private bool PlaceOnGrid(int x, int y, Vector3 normal, out int outX, out int outY)
    {
        outX = x;
        outY = y;

        double angleInRadians = Math.Atan2(normal.y, normal.x);
        float angleInDegrees = (float) (angleInRadians / Math.PI * 180.0f);

        bool isEvenRow = y % 2 == 1;
        bool topHalfIntersects = angleInDegrees > 0;
        if (topHalfIntersects)
        {
            // if (IsInRange(90.0f, 180.0f, angleInDegrees)) // top-left
            // {
            //     Debug.Log("top left");
            //     neighbourYOffset = -1;
            //     neighbourXOffset = isEvenRow ? 0 : -1;
            //
            //     // todo: check if occupied, and take top-right
            // }
            // else if (IsInRange(0f, 90.0f, angleInDegrees)) // top - right
            // {
            //     Debug.Log("top right");
            //     neighbourYOffset = -1;
            //     neighbourXOffset = isEvenRow ? 1 : 0;
            //
            //     // todo: check if occupied, and take top-left
            // }
            //
            // Debug.Log(angleInDegrees);
        }
        else
        {
            if (IsInRange(-180.0f, -165.0f, angleInDegrees)) // left
            {
                Debug.Log("left");
                outX = x - 1;
                outY = y + 0;
            }
            else if (IsInRange(-165.0f, -90.0f, angleInDegrees)) // bottom-left
            {
                Debug.Log("Bottom left");
                outY = y + 1;
                outX = x + (isEvenRow ? 0 : -1);
            }
            else if (IsInRange(-90.0f, -15.0f, angleInDegrees)) // bottom - right
            {
                Debug.Log("Bottom right");
                outY = y + 1;
                outX = x + (isEvenRow ? 1 : 0);
            }
            else if (IsInRange(-15.0f, 0.0f, angleInDegrees)) // right
            {
                Debug.Log("right");
                outX = x + 1;
            }

            Debug.Log(angleInDegrees);
        }

        return CoordinatesInRange(outX, outY) && CoordinatesOccupied(outX, outY);
    }
}