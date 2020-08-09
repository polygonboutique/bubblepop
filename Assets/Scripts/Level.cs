﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

/*
 * todos:
 * - remove debug drawings
 */

public class InGame : MonoBehaviour
{
    private const int MAX_GRID_WIDTH = 6;
    private const int MAX_GRID_HEIGHT = 10;

    private Ball[,] _grid;

    private GameObject _ballSpawnerGo;
    private BallSpawner _ballSpawner;

    private GameObject _ballShooterGo;
    private BallShooter _ballShooter;

    private Plane[] _boundsPlanes;
    private bool _canShoot;
    private bool _canMoveRow;

    private Ball _mergeTarget = null;
    private bool _mergeAnimationsRunning = false;
    private int _numLerpAnimationsRunning = 0;

    // *************************************************
    // Init and set-up
    // ************************************************* 
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
                SpawnBallOnGrid(x, y, Ball.GenerateCappedRandomValue());
            }
        }

        _boundsPlanes = new Plane[3];
        _boundsPlanes[0] = new Plane(Vector3.right, 0.0f); // left
        _boundsPlanes[1] = new Plane(Vector3.left, _ballSpawner.GeneratePosition(MAX_GRID_WIDTH, 0).magnitude
                                                   - _ballSpawner.GetBallRadius()); // right
        _boundsPlanes[2] = new Plane(Vector3.down, _ballSpawner.GetBallRadius() * 2); // up

        _canShoot = true;
        _canMoveRow = false;
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

    // *************************************************
    // Main logic
    // ************************************************* 

    void Update()
    {
        if (ReachedGameOverState())
        {
            Debug.Log("Game Over!");
            return;
        }

        if (_mergeAnimationsRunning && LerpAnimationsCompleted())
        {
            _mergeAnimationsRunning = false;

            if (_mergeTarget.ReachedMaxValue())
            {
                RemoveBallFromGrid(_mergeTarget.GetGridXCoord(), _mergeTarget.GetGridYCoord());
                TriggerExplosion(_mergeTarget.GetGridXCoord(), _mergeTarget.GetGridYCoord());

                // Trigger animation
                Destroy(_mergeTarget.gameObject);
            }

            // Check balls are supposed to fall down =>
            // loop through balls and find clusters, which can be removed, because they are not connected anymore

            _mergeTarget = null;
            NextTurn();
        }

        if (!CanShoot())
        {
            return;
        }

        Vector3 mouseCoordsWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseCoordsWorldSpace.z = 0;
        Vector3 shootDirection = (mouseCoordsWorldSpace - _ballShooter.GetPosition()).normalized;
        Ray ray = new Ray(_ballShooter.GetPosition(), shootDirection);

        Color shootDirColor = new Color32(255, 0, 22, 255);
        Debug.DrawLine(_ballShooter.GetPosition(), _ballShooter.GetPosition() + shootDirection * 100, shootDirColor);

        GameObject hitBall;
        RaycastHit hitInfo;
        List<Vector3> animationPath = new List<Vector3>();
        if (!IntersectsBalls(ray, out hitBall, out hitInfo))
        {
            // Hit planes
            Color32[] planeColor = new Color32[3];
            planeColor[0] = new Color32(255, 0, 255, 255);
            planeColor[1] = new Color32(0, 255, 0, 255);
            planeColor[1] = new Color32(255, 0, 223, 255);

            // => max 1 reflection
            // track nearest intersection point and store "next ray" here, then after the loop we do 1 more intersection test vs balls
            for (int i = 0; i < _boundsPlanes.Length; ++i)
            {
                Plane plane = _boundsPlanes[i];

                Vector3 planeOrigin = -plane.normal * plane.distance;
                Debug.DrawLine(planeOrigin, planeOrigin + Vector3.down * 200, planeColor[i]);
                Debug.DrawLine(planeOrigin, planeOrigin + Vector3.up * 200, planeColor[i]);

                // todo: get closest intersection and use that for "next ray"
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

                    // add plane intersection point to path
                    animationPath.Add(hitPoint);

                    Ray nextRay = new Ray(hitPoint, reflect);
                    if (IntersectsBalls(nextRay, out hitBall, out hitInfo))
                    {
                        break;
                    }

                    _ballShooter.HidePreviewBall();
                }
            }
        }

        if (hitBall)
        {
            var hitBallComp = hitBall.GetComponent<Ball>();

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

                {
                    Color color = new Color32(255, 0, 225, 255);
                    var position = _ballSpawner.GeneratePosition(hitBallComp.GetGridXCoord(), hitBallComp.GetGridYCoord());
                    Debug.DrawLine(position, position + (hitInfo.point - position).normalized * 200, color);
                }
            }

            var gridPosition = _ballSpawner.GeneratePosition(hitBallComp.GetGridXCoord(), hitBallComp.GetGridYCoord());
            var centerToHitDir = (hitInfo.point - gridPosition).normalized;
            if (PlaceOnGrid(hitBallComp.GetGridXCoord(), hitBallComp.GetGridYCoord(), centerToHitDir, out var gridX, out var gridY))
            {
                Vector3 nextPosition = _ballSpawner.GeneratePosition(gridX, gridY);
                _ballShooter.ShowPreviewBall(nextPosition);

                // add next ball position to path
                animationPath.Add(nextPosition);

                if (Input.GetButtonDown("Fire1"))
                {
                    _canShoot = false;
                    _ballShooter.HidePreviewBall();
                    StartCoroutine(MoveBallAnimation(_ballShooter.GetCurrentBall().GetComponent<Ball>(), gridX, gridY, animationPath));
                }
            }
            else
            {
                _ballShooter.HidePreviewBall();
            }
        }
    }

    private void NextTurn()
    {
        _ballShooter.NextBall();
        _canShoot = true;
        _canMoveRow = true;
    }

    // *************************************************
    // Helper functions down here
    // ************************************************* 

    private void SpawnBallOnGrid(int x, int y, int value)
    {
        _grid[x, y] = _ballSpawner.SpawnBallOnGrid(x, y, value).GetComponent<Ball>();
    }

    private void RemoveBallFromGrid(int x, int y)
    {
        _grid[x, y] = null;
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
        return !(_grid[x, y] == null);
    }

    private bool PlaceOnGrid(int x, int y, Vector3 normal, out int outX, out int outY)
    {
        outX = x;
        outY = y;

        double angleInRadians = Math.Atan2(normal.y, normal.x);
        float angleInDegrees = (float) (angleInRadians / Math.PI * 180.0f);

        bool isOddRow = y % 2 != 0;
        bool topHalfIntersects = angleInDegrees > 0;
        if (topHalfIntersects)
        {
            if (IsInRange(180.0f, 165.0f, angleInDegrees)) // left
            {
                outX = x - 1;
                outY = y + 0;
            }
            else if (IsInRange(90.0f, 165.0f, angleInDegrees)) // top-left
            {
                outX = x + (isOddRow ? 0 : -1);
                outY = y - 1;
            }
            else if (IsInRange(15.0f, 90.0f, angleInDegrees)) // top - right
            {
                outX = x + (isOddRow ? 1 : 0);
                outY = y - 1;
            }
            else if (IsInRange(0.0f, 15.0f, angleInDegrees)) // right
            {
                outX = x + 1;
                outY = y + 0;
            }
        }
        else
        {
            if (IsInRange(-180.0f, -165.0f, angleInDegrees)) // left
            {
                outX = x - 1;
                outY = y + 0;
            }
            else if (IsInRange(-165.0f, -90.0f, angleInDegrees)) // bottom-left
            {
                outX = x + (isOddRow ? 0 : -1);
                outY = y + 1;
            }
            else if (IsInRange(-90.0f, -15.0f, angleInDegrees)) // bottom - right
            {
                outX = x + (isOddRow ? 1 : 0);
                outY = y + 1;
            }
            else if (IsInRange(-15.0f, 0.0f, angleInDegrees)) // right
            {
                outX = x + 1;
                outY = y + 0;
            }
        }

        return CoordinatesInRange(outX, outY) && !CoordinatesOccupied(outX, outY);
    }

    private bool IntersectsBalls(Ray ray, out GameObject outGameObject, out RaycastHit outHitInfo)
    {
        float closestDistance = Single.PositiveInfinity;
        bool found = false;
        outGameObject = null;
        outHitInfo = new RaycastHit();

        for (int y = 0; y < MAX_GRID_HEIGHT; ++y)
        {
            for (int x = 0; x < MAX_GRID_WIDTH; ++x)
            {
                if (!_grid[x, y])
                {
                    continue;
                }

                GameObject go = _grid[x, y].gameObject;
                if (go.GetComponent<Collider>().Raycast(ray, out var tmpHitInfo, 1000.0f))
                {
                    if (tmpHitInfo.distance < closestDistance)
                    {
                        outHitInfo = tmpHitInfo;
                        closestDistance = outHitInfo.distance;
                        outGameObject = go;
                        found = true;
                    }
                }
            }
        }

        return found;
    }

    private bool ReachedGameOverState()
    {
        int centerXIndex = MAX_GRID_WIDTH / 2;
        int lastYIndex = MAX_GRID_HEIGHT - 1;
        return (CoordinatesInRange(centerXIndex, lastYIndex) && _grid[centerXIndex, lastYIndex] != null)
               || (CoordinatesInRange(centerXIndex - 1, lastYIndex) && _grid[centerXIndex - 1, lastYIndex] != null)
               || (CoordinatesInRange(centerXIndex + 1, lastYIndex) && _grid[centerXIndex + 1, lastYIndex] != null);
    }

    private bool CanShoot()
    {
        return _canShoot;
    }

    private bool LerpAnimationsCompleted()
    {
        Assert.IsTrue(_numLerpAnimationsRunning > -1);
        return _numLerpAnimationsRunning == 0;
    }

    private List<Ball> GatherNeighboursBalls(int gridX, int gridY)
    {
        List<Ball> neighbours = new List<Ball>();

        bool isEvenRow = gridY % 2 == 0;

        int topY = gridY - 1;
        int bottomY = gridY + 1;
        int diagonalLeftX = gridX + (isEvenRow ? -1 : 0);
        int diagonalRightX = gridX + (isEvenRow ? 0 : 1);
        int leftX = gridX - 1;
        int rightX = gridX + 1;

        // check top-left, top-right, right, bottom-right, bottom-left, left
        if (CoordinatesInRange(diagonalLeftX, topY) && CoordinatesOccupied(diagonalLeftX, topY))
        {
            neighbours.Add(_grid[diagonalLeftX, topY]);
        }

        if (CoordinatesInRange(diagonalRightX, topY) && CoordinatesOccupied(diagonalRightX, topY))
        {
            neighbours.Add(_grid[diagonalRightX, topY]);
        }

        if (CoordinatesInRange(rightX, gridY) && CoordinatesOccupied(rightX, gridY))
        {
            neighbours.Add(_grid[rightX, gridY]);
        }

        if (CoordinatesInRange(diagonalRightX, bottomY) && CoordinatesOccupied(diagonalRightX, bottomY))
        {
            neighbours.Add(_grid[diagonalRightX, bottomY]);
        }

        if (CoordinatesInRange(diagonalLeftX, bottomY) && CoordinatesOccupied(diagonalLeftX, bottomY))
        {
            neighbours.Add(_grid[diagonalLeftX, bottomY]);
        }

        if (CoordinatesInRange(leftX, gridY) && CoordinatesOccupied(leftX, gridY))
        {
            neighbours.Add(_grid[leftX, gridY]);
        }

        return neighbours;
    }

    private void GatherClusters(int gridX, int gridY, ref List<Ball> ballCluster)
    {
        Ball activeBall = _grid[gridX, gridY];
        List<Ball> neighbours = GatherNeighboursBalls(gridX, gridY);

        foreach (Ball neighbour in neighbours) 
        {
            if (activeBall.CanMerge(neighbour) && !ballCluster.Contains(neighbour))
            {
                ballCluster.Add(neighbour);
                GatherClusters(neighbour.GetGridXCoord(), neighbour.GetGridYCoord(), ref ballCluster);
            }
        }
    }

    private Ball DetermineMergeTarget(List<Ball> ballCluster)
    {
        Ball mergeTarget = null;
        int maxDist = Int32.MaxValue;

        foreach (Ball ball in ballCluster)
        {
            // do manhatten distance
            // |x1 - x2| + |y1 - y2|
            int distance = ball.GetGridXCoord() + ball.GetGridYCoord();
            if (distance < maxDist)
            {
                maxDist = distance;
                mergeTarget = ball;
            }
        }

        return mergeTarget;
    }

    private void MergeBalls(int activeGridX, int activeGridY)
    {
        List<Ball> ballCluster = new List<Ball>();
        GatherClusters(activeGridX, activeGridY, ref ballCluster);

        if (ballCluster.Count >= 2)
        {
            Ball mergeTarget = DetermineMergeTarget(ballCluster);
            List<Vector3> path = new List<Vector3>();
            path.Add(mergeTarget.transform.position);

            _numLerpAnimationsRunning = ballCluster.Count - 1;
            _mergeAnimationsRunning = true;
            _mergeTarget = mergeTarget;

            foreach (Ball merger in ballCluster)
            {
                if (mergeTarget == merger)
                {
                    continue;
                }

                RemoveBallFromGrid(merger.GetGridXCoord(), merger.GetGridYCoord());
                StartCoroutine(LerpBallAnimation(merger, mergeTarget.GetGridXCoord(), mergeTarget.GetGridYCoord(), path));
            }
        }
        else
        {
            NextTurn();
        }
    }

    private void TriggerExplosion(int gridX, int gridY)
    {
        // remove surrounding neighbours
    }

    // *************************************************
    // Animation functions
    // *************************************************
    IEnumerator MoveBallAnimation(Ball ball, int targetGridX, int targetGridY, List<Vector3> path)
    {
        const float epsilon = 0.05f;
        const float speed = 155.0f;
        while (path.Count > 0)
        {
            while (Vector3.Distance(ball.transform.position, path[0]) > epsilon)
            {
                ball.transform.position = Vector3.MoveTowards(ball.transform.position, path[0], Time.deltaTime * speed);
                yield return null;
            }

            path.RemoveAt(0);
            yield return null;
        }

        var value = ball.GetValue();
        Destroy(ball.gameObject);

        SpawnBallOnGrid(targetGridX, targetGridY, value);
        MergeBalls(targetGridX, targetGridY);

        yield return null;
    }

    IEnumerator LerpBallAnimation(Ball ball, int targetGridX, int targetGridY, List<Vector3> path)
    {
        float elapsed = 0;
        float targetDuration = 0.45f; // seconds
        Vector3 initialLerpPosition = ball.transform.position;

        while (elapsed < targetDuration)
        {
            elapsed += Time.deltaTime;
            float lerpAmount = elapsed / targetDuration;
            float t = Mathf.Sin(lerpAmount * Mathf.PI * 0.5f);
            ball.transform.position = Vector3.Lerp(initialLerpPosition, path[0], t);
            yield return null;
        }

        Destroy(ball.gameObject);
        _grid[targetGridX, targetGridY].IncreaseValue();
        _numLerpAnimationsRunning--;

        yield return null;
    }
}