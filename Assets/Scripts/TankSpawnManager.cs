using UnityEngine;

using Unity.Netcode;
using UnityEngine.SceneManagement;

public class TankSpawnManager : MonoBehaviour
{
    public static TankSpawnManager Instance { get; private set; }

    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    public void GetSpawn(out Vector3 position, out Quaternion rotation)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }

        Transform selected = spawnPoints[Random.Range(0, spawnPoints.Length)];

        position = selected.position;
        rotation = selected.rotation;
    }

    // ================ Network Spawning =====================
    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
        }
    }

    private void HandleSceneEvent(SceneEvent sceneEvent)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (sceneEvent.SceneEventType != SceneEventType.LoadComplete)
            return;

        //if (sceneEvent.SceneName != "TankArena")
        //    return;

        SpawnAllPlayers();
    }

    private void SpawnAllPlayers()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        int index = 0;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // - Prevent duplicate spawning.
            var client = NetworkManager.Singleton.ConnectedClients[clientId];

            // - If the player already has a tank for some reason skip them
            if (client.PlayerObject != null)
                continue;

            // - Manually grab a spawn point unique for each player, so you don't spawn ontop of each other
            Transform spawnPoint = spawnPoints[index % spawnPoints.Length];

            // - Spawn the tank prefab at the spawn point, giving ownership to the target client
            GameObject tank = Instantiate(tankPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkObject networkObject = tank.GetComponent<NetworkObject>();            
            networkObject.SpawnAsPlayerObject(clientId);

            index++;
        }
    }
}
