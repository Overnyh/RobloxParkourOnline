using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;


public class MapGenerator : NetworkBehaviour
{
    [SerializeField] private List<GameObject> tiles;
    [SerializeField] private float stemp;
    [SerializeField] private float angle;
    [SerializeField] private bool canDublicate;

    private List<GameObject> _spawnedTiles;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _spawnedTiles = new List<GameObject>();
        GenerateMap();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        enabled = false;
    }

    public void GenerateMap()
    {
        foreach (var tile in _spawnedTiles?.ToList()!)
        {
            ServerManager.Despawn(tile);
            _spawnedTiles.Remove(tile);
        }
        
        var tempTiles = new List<GameObject>(tiles);
        for (int i = 0; i < tiles.Count; i++)
        {
            print(i);
            int randomIndex = Random.Range(0, tempTiles.Count);
            GameObject objectToSpawn = tempTiles[randomIndex];
            if (!canDublicate) tempTiles.RemoveAt(randomIndex);
            _spawnedTiles.Add(Instantiate(objectToSpawn, transform.position + new Vector3(0, stemp * i), new Quaternion(0, 0, 0, 0), transform));
            _spawnedTiles[i].transform.Rotate(0, angle * i, 0);
            
            ServerManager.Spawn(_spawnedTiles[i]);
            
        }
        
    }
}
