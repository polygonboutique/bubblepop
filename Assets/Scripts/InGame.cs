﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

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

    private GameObject _ballPrefab;
    private GameObject _gameOverGroup;
    private GameObject _winScreenGroup;
    private LineRenderer _lineRenderer;

    private bool _mouse_down;

    // *************************************************
    // Init and set-up
    // *************************************************

    public void Test()
    {
        Debug.Log("Button pressed");
    }

    public void Initialize(GameObject ballPrefab, GameObject gameOverGroup,
        GameObject winScreenGroup, LineRenderer lineRenderer, float ballSize)
    {
        name = "Ingame";

        _ballPrefab = ballPrefab;
        _gameOverGroup = gameOverGroup;
        _winScreenGroup = winScreenGroup;
        _lineRenderer = lineRenderer;

        gameOverGroup.SetActive(false);
        winScreenGroup.SetActive(false);

        InitializeBallSpawner(ballPrefab, ballSize);
        InitializeBallShooter(_ballSpawner);
        SetupCamera();

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

    private void SetupCamera()
    {
        Camera.main.transform.position = _ballShooter.GetPosition() + new Vector3(0, 25, -10);
    }

    private void InitializeBallSpawner(GameObject ballPrefab, float ballScale)
    {
        _ballSpawnerGo = new GameObject();
        _ballSpawnerGo.name = "BallSpawner";
        _ballSpawnerGo.transform.parent = transform;

        _ballSpawner = _ballSpawnerGo.AddComponent<BallSpawner>();
        _ballSpawner.Initialize(ballPrefab, ballScale);
    }

    private void InitializeBallShooter(BallSpawner ballSpawner)
    {
        int widthIndex = MAX_GRID_WIDTH / 2;

        Vector3 currentBallPosition = _ballSpawner.GeneratePosition(widthIndex, MAX_GRID_HEIGHT)
                                      + new Vector3(-_ballSpawner.GetBallRadius() / 2, 0, 0);
        Vector3 nextBallPosition = _ballSpawner.GeneratePosition(widthIndex - 2, MAX_GRID_HEIGHT + 1);

        _ballShooterGo = new GameObject();
        _ballShooterGo.name = "BallShooter";
        _ballShooterGo.transform.parent = transform;
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
            _gameOverGroup.SetActive(true);
            return;
        }

        if (ReachedWinState())
        {
            _winScreenGroup.SetActive(true);
            return;
        }

        if (!_mouse_down && Input.GetMouseButtonDown(0))
        {
            _mouse_down = true;
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

                NextTurn();
            }
            else
            {
                MergeBalls(_mergeTarget.GetGridXCoord(), _mergeTarget.GetGridYCoord());
                _mergeAnimationsRunning = !LerpAnimationsCompleted();
            }

            if (!_mergeAnimationsRunning)
            {
                RemoveFreeFloatingBall();
                _mergeTarget = null;
            }
        }

        if (!CanShoot())
        {
            return;
        }
        
        if (_mouse_down)
        {
            Vector3 mouseCoordsWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseCoordsWorldSpace.z = 0;
            Vector3 shootDirection = (mouseCoordsWorldSpace - _ballShooter.GetPosition()).normalized;
            Ray ray = new Ray(_ballShooter.GetPosition(), shootDirection);

            List<Vector3> linesToDraw = new List<Vector3>();
            linesToDraw.Add(_ballShooter.GetPosition());

            GameObject hitBall;
            RaycastHit hitInfo;
            List<Vector3> animationPath = new List<Vector3>();
            if (!IntersectsBalls(ray, out hitBall, out hitInfo))
            {
                float closestDistance = Single.PositiveInfinity;
                Plane closestPlane = _boundsPlanes[0];
                Vector3 closestHitPoint = new Vector3();
                bool didHitPlane = false;

                // => max 1 reflection
                // track nearest intersection point and store "next ray" here, then after the loop we do 1 more intersection test vs balls
                for (int i = 0; i < _boundsPlanes.Length; ++i)
                {
                    Plane plane = _boundsPlanes[i];
                    if (plane.Raycast(ray, out float enter))
                    {
                        didHitPlane = true;
                        Vector3 hitPoint = ray.GetPoint(enter);

                        float distance = Vector3.Distance(_ballShooter.GetPosition(), hitPoint);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestPlane = plane;
                            closestHitPoint = hitPoint;
                        }
                    }
                }

                if (didHitPlane)
                {
                    Vector3 reflect = Vector3.Reflect(ray.direction, closestPlane.normal);

                    // add plane intersection point to path
                    animationPath.Add(closestHitPoint);
                    linesToDraw.Add(closestHitPoint);

                    Ray nextRay = new Ray(closestHitPoint, reflect);
                    IntersectsBalls(nextRay, out hitBall, out hitInfo);
                    _ballShooter.HidePreviewBall();
                }
            }

            if (hitBall)
            {
                linesToDraw.Add(hitInfo.point);
                _lineRenderer.positionCount = linesToDraw.Count;
                for (int i = 0; i < linesToDraw.Count; ++i)
                {
                    _lineRenderer.SetPosition(i, linesToDraw[i]);
                }

                var hitBallComp = hitBall.GetComponent<Ball>();
                var gridPosition = _ballSpawner.GeneratePosition(hitBallComp.GetGridXCoord(), hitBallComp.GetGridYCoord());
                var centerToHitDir = (hitInfo.point - gridPosition).normalized;

                if (PlaceOnGrid(hitBallComp.GetGridXCoord(), hitBallComp.GetGridYCoord(), centerToHitDir, out var gridX, out var gridY))
                {
                    Vector3 nextPosition = _ballSpawner.GeneratePosition(gridX, gridY);
                    _ballShooter.ShowPreviewBall(nextPosition);

                    // add next ball position to path
                    animationPath.Add(nextPosition);

                    if (Input.GetMouseButtonUp(0))
                    {
                        _canShoot = false;
                        _ballShooter.HidePreviewBall();
                        StartCoroutine(ShootBallAnimation(_ballShooter.GetCurrentBall().GetComponent<Ball>(), gridX, gridY, animationPath));
                        _lineRenderer.positionCount = 0;
                    }
                }
                else
                {
                    _ballShooter.HidePreviewBall();
                }
            }
            else
            {
                _lineRenderer.positionCount = 0;
            }
        }


        if (_mouse_down && Input.GetMouseButtonUp(0))
        {
            _mouse_down = false;
            _lineRenderer.positionCount = 0;
        }
    }

    private void NextTurn()
    {
        _ballShooter.NextBall();
        _canShoot = true;
        _canMoveRow = true;

        // todo: advance rows
    }

    // *************************************************
    // Helper functions
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

    private bool ReachedWinState()
    {
        for (int y = 0; y < MAX_GRID_HEIGHT; ++y)
        {
            for (int x = 0; x < MAX_GRID_WIDTH; x++)
            {
                if (CoordinatesOccupied(x, y))
                {
                    return false;
                }
            }
        }

        return true;
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

    private void GatherClusters(int gridX, int gridY, ref List<Ball> ballCluster, bool sameValueRequired)
    {
        Ball activeBall = _grid[gridX, gridY];
        List<Ball> neighbours = GatherNeighboursBalls(gridX, gridY);

        foreach (Ball neighbour in neighbours)
        {
            bool canMerge = !sameValueRequired || activeBall.CanMerge(neighbour);
            if (canMerge && !ballCluster.Contains(neighbour))
            {
                ballCluster.Add(neighbour);
                GatherClusters(neighbour.GetGridXCoord(), neighbour.GetGridYCoord(), ref ballCluster, sameValueRequired);
            }
        }
    }

    private Ball DetermineMergeTarget(List<Ball> ballCluster)
    {
        Ball mergeTarget = null;
        int maxDist = Int32.MaxValue;

        foreach (Ball ball in ballCluster)
        {
            // manhattan dist: |x1 - x2| + |y1 - y2|
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
        GatherClusters(activeGridX, activeGridY, ref ballCluster, true);

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
                StartCoroutine(MergeBallAnimation(merger, mergeTarget.GetGridXCoord(), mergeTarget.GetGridYCoord(), path));
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
        List<Ball> neighbours = GatherNeighboursBalls(gridX, gridY);
        foreach (Ball ball in neighbours)
        {
            RemoveBallFromGrid(ball.GetGridXCoord(), ball.GetGridYCoord());
            Destroy(ball.gameObject);
        }
    }

    private void RemoveFreeFloatingBall()
    {
        List<Ball> ballsFloatingOnGrid = new List<Ball>();
        List<int> tilesToVisit = new List<int>();
        for (int x = 0; x < MAX_GRID_WIDTH; x++)
        {
            tilesToVisit.Add(x);
            for (int y = 0; y < MAX_GRID_HEIGHT; ++y)
            {
                if (CoordinatesOccupied(x, y))
                {
                    ballsFloatingOnGrid.Add(_grid[x, y]);
                }
            }
        }

        while (tilesToVisit.Count > 0)
        {
            int x = tilesToVisit[0];

            if (CoordinatesOccupied(x, 0))
            {
                List<Ball> cluster = new List<Ball>();
                GatherClusters(x, 0, ref cluster, false);

                for (int i = tilesToVisit.Count - 1; i >= 1; --i)
                {
                    Ball ball = _grid[tilesToVisit[i], 0];
                    if (cluster.Contains(ball))
                    {
                        tilesToVisit.RemoveAt(i);
                    }
                }

                // remove balls from deletion list, which are contained in cluster
                foreach (Ball ballInCluster in cluster)
                {
                    if (ballsFloatingOnGrid.Contains(ballInCluster))
                    {
                        ballsFloatingOnGrid.Remove(ballInCluster);
                    }
                }
            }

            tilesToVisit.RemoveAt(0);
        }

        foreach (Ball ball in ballsFloatingOnGrid)
        {
            RemoveBallFromGrid(ball.GetGridXCoord(), ball.GetGridYCoord());
            Destroy(ball.gameObject);
        }
    }

    // *************************************************
    // Animation functions
    // *************************************************
    IEnumerator ShootBallAnimation(Ball ball, int targetGridX, int targetGridY, List<Vector3> path)
    {
        const float epsilon = 0.05f;
        const float speed = 165.0f;
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

    IEnumerator MergeBallAnimation(Ball ball, int targetGridX, int targetGridY, List<Vector3> path)
    {
        float elapsed = 0;
        float targetDuration = 0.32f; // seconds
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