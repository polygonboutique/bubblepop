using System;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

/*
 * todos:
 * - rename this file to "InGame" and remove "Level"
 * - move game over check to "Main"
 * - remove debug drawings
 */

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

    // *************************************************
    // Main logic
    // ************************************************* 

    void Update()
    {
        if (ReachedGameOverState())
        {
            Debug.Log("Game Over!");
        }

        Vector3 mouseCoordsWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseCoordsWorldSpace.z = 0;
        Vector3 shootDirection = (mouseCoordsWorldSpace - _ballShooter.GetPosition()).normalized;
        Ray ray = new Ray(_ballShooter.GetPosition(), shootDirection);

        Color shootDirColor = new Color32(255, 0, 22, 255);
        Debug.DrawLine(_ballShooter.GetPosition(), _ballShooter.GetPosition() + shootDirection * 100, shootDirColor);

        GameObject hitBall;
        RaycastHit hitInfo;
        if (!IntersectsBalls(ray, out hitBall, out hitInfo))
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
                    if (IntersectsBalls(nextRay, out hitBall, out hitInfo))
                    {
                        Debug.Log(hitBall);
                    }
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
                _ballShooter.ShowPreviewBall(_ballSpawner.GeneratePosition(gridX, gridY));

                // todo: create list of points we need to visit
                // todo: visit path; end of path => try merge

                if (Input.GetButtonDown("Fire1"))
                {
                    _ballShooter.ShootBall(shootDirection); // kick off tween animation
                    _ballShooter.ReloadBall();

                    // todo: execture this, when tween is done. 
                    SpawnBallOnGrid(gridX, gridY, Ball.GenerateRandomValue());
                }
            }
            else
            {
                _ballShooter.HidePreviewBall();
            }
        }
    }

    // *************************************************
    // Helper functions down here
    // ************************************************* 

    private void SpawnBallOnGrid(int x, int y, int value)
    {
        _grid[x, y] = _ballSpawner.SpawnBallOnGrid(x, y, value).GetComponent<Ball>();
    }

    private void DestroyBallOnGrid(int x, int y)
    {
        Destroy(_grid[x, y].gameObject);
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

        bool isEvenRow = y % 2 == 1;
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
                outX = x + (isEvenRow ? 0 : -1);
                outY = y - 1;
            }
            else if (IsInRange(15.0f, 90.0f, angleInDegrees)) // top - right
            {
                outX = x + (isEvenRow ? 1 : 0);
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
                outX = x + (isEvenRow ? 0 : -1);
                outY = y + 1;
            }
            else if (IsInRange(-90.0f, -15.0f, angleInDegrees)) // bottom - right
            {
                outX = x + (isEvenRow ? 1 : 0);
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
}