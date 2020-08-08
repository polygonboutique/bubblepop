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

    private void ShootBall(Vector2 direction)
    {
        var body = _currentBall.GetComponent<Rigidbody2D>();
        body.gravityScale = 1.0f;
        body.AddForce(direction * 180, ForceMode2D.Impulse);
        _currentBall.layer = _defaultLayer;
    }

    private void ReloadBall()
    {
        _currentBall = _nextBall;
        _currentBall.transform.position = _currentBallSpawnPosition;
        _nextBall = SpawnRandomBall(_nextBallSpawnPosition, _uiLayer);
    }

    private GameObject SpawnRandomBall(Vector3 position, int layer)
    {
        GameObject go = _ballSpawner.SpawnBall(position, Ball.GenerateRandomValue());
        go.layer = layer;

        return go;
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

