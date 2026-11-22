
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class TankHealth : NetworkBehaviour
{
    public int maxHealth = 100;
    [SerializeField] private float respawnDelay = 3f;

    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsAlive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private ParticleSystem smoke_VFX;

    [SerializeField] NetworkPlayerStats stats;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        CurrentHealth.Value = maxHealth;
        IsAlive.Value = true;
    }

    public void TakeDamage(int damage, ulong shooterId)
    {
        if (!IsServer || !IsAlive.Value) 
            return;

        CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - damage);

        if (CurrentHealth.Value == 0)
        {
            IsAlive.Value = false;
            ToggleSmokeRpc(true);

            if (stats.Lives.Value > 0)
            {
                StartCoroutine(RespawnRoutine());
            }
            
            // - Award owner of bullet a point
            AwardKill(shooterId);

            if (stats.Lives.Value <= 0)
            {
                // Trigger game over somehow
            }
        }
    }

    private void AwardKill(ulong shooterId)
    {
        if (!IsServer) return;

        // - Find the client that fired the projectile
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(shooterId, out NetworkClient shooter))
        {
            Debug.Log("Shooter Disconnected");
            return;
        }

        // - Get the shooter's player object
        NetworkObject playerObject = shooter.PlayerObject;
    
        if (playerObject == null) return;

        // - Get THAT player objects stat component
        NetworkPlayerStats stats = playerObject.GetComponent<NetworkPlayerStats>();

        if (stats == null) return;

        // - Server updates the score directly.
        stats.AddScore(100);

        Debug.Log($"Awarded {100} points to Client {shooterId}");
    }

    private IEnumerator RespawnRoutine()
    {           
        yield return new WaitForSeconds(respawnDelay);

        if (!IsSpawned) yield break;

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (TankSpawnManager.Instance != null)
        {
            TankSpawnManager.Instance.GetSpawn(out spawnPosition, out spawnRotation);
        }

        // - MUST be executed on the server
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        GetComponent<NetworkPlayerController>().TeleportOwnerRpc(spawnPosition, spawnRotation);

        stats.LoseLife();
        CurrentHealth.Value = maxHealth;
        IsAlive.Value = true;
        ToggleSmokeRpc(false);
    }

    [Rpc(SendTo.Everyone)]
    private void ToggleSmokeRpc(bool state)
    {
        if (state)
        {
            smoke_VFX.gameObject.SetActive(true);
            smoke_VFX.Play();
        }

        else
        {
            smoke_VFX.Stop();
            smoke_VFX.gameObject.SetActive(false);
        }
    }
}
