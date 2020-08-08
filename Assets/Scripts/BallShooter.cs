using System.Collections;
using System.Collections.Generic;
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
        
        _currentBall = _ballSpawner.SpawnBall(_currentBallSpawnPosition);
        _nextBall = _ballSpawner.SpawnBall(_nextBallSpawnPosition);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log(Input.mousePosition);
        }
    }
}
