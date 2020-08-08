using UnityEngine;

public class BallShooter : MonoBehaviour
{
    private BallSpawner _ballSpawner;
    private Vector3 _currentBallSpawnPosition;
    private Vector3 _nextBallSpawnPosition;

    private GameObject _currentBall;
    private GameObject _nextBall;
    
    public void Initialize(BallSpawner ballSpawner, Vector3 currentBallSpawnPosition, Vector3 nextBallSpawnPosition)
    {
        _ballSpawner = ballSpawner;
        _currentBallSpawnPosition = currentBallSpawnPosition;
        _nextBallSpawnPosition = nextBallSpawnPosition;

        _currentBall = SpawnRandomBall(_currentBallSpawnPosition);
        _nextBall = SpawnRandomBall(_nextBallSpawnPosition);
    }

    public void ShootBall(Vector2 direction)
    {
        // trigger ball to move
    }

    public void ReloadBall()
    {
        _currentBall = _nextBall;
        _currentBall.transform.position = _currentBallSpawnPosition;
        _nextBall = SpawnRandomBall(_nextBallSpawnPosition);
    }

    public Vector3 GetPosition()
    {
        return _currentBallSpawnPosition;
    }

    public GameObject GetCurrentBall()
    {
        return _currentBall.gameObject;
    }

    private GameObject SpawnRandomBall(Vector3 position)
    {
        return _ballSpawner.SpawnBall(position, Ball.GenerateRandomValue());
    }
}

