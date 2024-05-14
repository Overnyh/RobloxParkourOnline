using FishNet.Object;
using UnityEngine;


public class NetTest : NetworkBehaviour
{
    [SerializeField] private GameObject objToSpawn;
    
    [HideInInspector] public GameObject spawnedObject;
 
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
            enabled = false;
    }
 
    private void Update()
    {
        if(spawnedObject == null && Input.GetKeyDown(KeyCode.Q))
        {
            SpawnObject(objToSpawn, transform, this);
        }
 
        if (spawnedObject != null && Input.GetKeyDown(KeyCode.Q))
        {
            DespawnObject(spawnedObject);
        }
    }
 
    [ServerRpc]
    public void SpawnObject(GameObject obj, Transform player, NetTest script)
    {
        GameObject spawned = Instantiate(obj, player.position + player.forward, Quaternion.identity);
        ServerManager.Spawn(spawned);
        SetSpawnedObject(spawned, script);
    }
 
    [ObserversRpc]
    public void SetSpawnedObject(GameObject spawned, NetTest script)
    {
        script.spawnedObject = spawned;
    }
 
    [ServerRpc(RequireOwnership = false)]
    public void DespawnObject(GameObject obj)
    {
        ServerManager.Despawn(obj);
    }
}
