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
        _currentBall.name = "CurrentBall";
        
        _nextBall = SpawnRandomBall(_nextBallSpawnPosition);
        _nextBall.name = "NextBall";
        
        _previewBall = _ballSpawner.SpawnBall(BIG_NUMBER, _currentBall.GetComponent<Ball>().GetValue());
        _previewBall.name = "PreviewBall";
    }

    public void NextBall()
    {
        _currentBall = _nextBall;
        _currentBall.transform.position = _currentBallSpawnPosition;
        _currentBall.name = "NEXT BALL";
        
        _nextBall = SpawnRandomBall(_nextBallSpawnPosition);
        _nextBall.name = "NEXT BALL";

        var prevBall = _previewBall.GetComponent<Ball>();
        prevBall.SetValue(_currentBall.GetComponent<Ball>().GetValue());
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
        return _ballSpawner.SpawnBall(position, Ball.GenerateCappedRandomValue());
    }

    public GameObject GetCurrentBall()
    {
        return _currentBall;
    }
}

