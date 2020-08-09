using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject ballPrefab;
    public GameObject gameOverGroup;
    public GameObject winScreenGroup;
    public LineRenderer lineRenderer;

    private GameObject _ingame;
    
    // Start is called before the first frame update
    void Start()
    {
        _ingame = new GameObject();
        var ingame = _ingame.AddComponent<InGame>();
        ingame.Initialize(ballPrefab, gameOverGroup, winScreenGroup, lineRenderer, 6);
    }

    public void Restart()
    {
        Debug.Log("Restart game!");
    }
}
