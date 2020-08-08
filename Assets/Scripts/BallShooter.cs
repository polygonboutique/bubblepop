using UnityEngine;

public class BallShooter : MonoBehaviour
{
    private BallSpawner _ballSpawner;
    private Vector3 _currentBallSpawnPosition;
    private Vector3 _nextBallSpawnPosition;
    private int _uiLayer;
    private int _defaultLayer;

    private GameObject _currentBall;
    private GameObject _nextBall;
    
    public void Initialize(BallSpawner ballSpawner, Vector3 currentBallSpawnPosition, Vector3 nextBallSpawnPosition)
    {
        _ballSpawner = ballSpawner;
        _currentBallSpawnPosition = currentBallSpawnPosition;
        _nextBallSpawnPosition = nextBallSpawnPosition;
        _defaultLayer = LayerMask.NameToLayer("Default");
        _uiLayer = LayerMask.NameToLayer("UI");

        _currentBall = SpawnRandomBall(_currentBallSpawnPosition, _uiLayer);
        _nextBall = SpawnRandomBall(_nextBallSpawnPosition, _uiLayer);
    }

    public void ShootBall(Vector2 direction)
    {
        var body = _currentBall.GetComponent<Rigidbody2D>();
        body.gravityScale = 1.0f;
        body.AddForce(direction * 180, ForceMode2D.Impulse);
        _currentBall.layer = _defaultLayer;
    }

    public void ReloadBall()
    {
        _currentBall = _nextBall;
        _currentBall.transform.position = _currentBallSpawnPosition;
        _nextBall = SpawnRandomBall(_nextBallSpawnPosition, _uiLayer);
    }

    public Vector3 GetPosition()
    {
        return _currentBallSpawnPosition;
    }

    private GameObject SpawnRandomBall(Vector3 position, int layer)
    {
        GameObject go = _ballSpawner.SpawnBall(position, Ball.GenerateRandomValue());
        go.layer = layer;

        return go;
    }
}

