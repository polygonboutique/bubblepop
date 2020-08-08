using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

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
        
        _currentBall = _ballSpawner.SpawnBall(_currentBallSpawnPosition, Ball.GenerateRandomValue());
        _nextBall = _ballSpawner.SpawnBall(_nextBallSpawnPosition, Ball.GenerateRandomValue());
    }

    private void ShootBall(Vector2 direction)
    {
        var body = _currentBall.GetComponent<Rigidbody2D>();
        body.gravityScale = 1.0f;
        body.AddForce(direction * 180, ForceMode2D.Impulse);
    }

    private void ReloadBall()
    {
        _currentBall = _nextBall;
        _currentBall.transform.position = _currentBallSpawnPosition;
        _nextBall = _ballSpawner.SpawnBall(_nextBallSpawnPosition, Ball.GenerateRandomValue());
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
            Vector3 mouseCoordsWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseCoordsWorldSpace.z = 0;
            
            Vector3 shootDirection = (mouseCoordsWorldSpace - _currentBallSpawnPosition).normalized;
            
            ShootBall(shootDirection);
            ReloadBall();
        }
    }
}

