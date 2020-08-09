using UnityEngine;

public class BallShooter : MonoBehaviour
{
    private static Vector3 BIG_NUMBER = new Vector3(10000.0f, 10000.0f, 10000.0f);
    
    private BallSpawner _ballSpawner;
    private Vector3 _currentBallSpawnPosition;
    private Vector3 _nextBallSpawnPosition;

    private GameObject _previewBall;
    private GameObject _currentBall;
    private GameObject _nextBall;

    public void Initialize(BallSpawner ballSpawner, Vector3 currentBallSpawnPosition, Vector3 nextBallSpawnPosition)
    {
        _ballSpawner = ballSpawner;
        _currentBallSpawnPosition = currentBallSpawnPosition;
        _nextBallSpawnPosition = nextBallSpawnPosition;

        _currentBall = SpawnRandomBall(_currentBallSpawnPosition);
        _nextBall = SpawnRandomBall(_nextBallSpawnPosition);
        _previewBall = _ballSpawner.SpawnBall(BIG_NUMBER, 1);
    }

    public void NextBall()
    {
        _currentBall = _nextBall;
        _currentBall.transform.position = _currentBallSpawnPosition;
        _nextBall = SpawnRandomBall(_nextBallSpawnPosition);
    }

    public Vector3 GetPosition()
    {
        return _currentBallSpawnPosition;
    }

    public void ShowPreviewBall(Vector3 position)
    {
        _previewBall.transform.position = position;
    }

    public void HidePreviewBall()
    {
        ShowPreviewBall(BIG_NUMBER);
    }

    private GameObject SpawnRandomBall(Vector3 position)
    {
        return _ballSpawner.SpawnBall(position, Ball.GenerateRandomValue());
    }

    public GameObject GetCurrentBall()
    {
        return _currentBall;
    }
}

