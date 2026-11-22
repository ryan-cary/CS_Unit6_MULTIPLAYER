using Unity.Netcode;
using UnityEngine;

public class MineSpawner : NetworkBehaviour
{
    [SerializeField] private int numMines = 25;
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private float maxRadius = 50;
    [SerializeField] private float minRadius = 5;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!IsServer) return;
        
        for (int i = 0; i < numMines; i++)
        {
            Vector2 pos = Random.onUnitCircle * Random.Range(minRadius, maxRadius);
            Vector3 spawn = new Vector3(pos.x, 0, pos.y);
            
            GameObject instance = Instantiate(minePrefab, spawn, Quaternion.identity);
            NetworkObject instanceNetworkObject = instance.GetComponent<NetworkObject>();
            instanceNetworkObject.Spawn();
        }
    }
}
