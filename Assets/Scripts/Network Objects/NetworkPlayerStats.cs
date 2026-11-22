using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class NetworkPlayerStats : NetworkBehaviour
{    
    public TankHealth health;
    public NetworkVariable<int> Lives = new NetworkVariable<int>(3);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            NetworkScoreboard scoreboard = FindFirstObjectByType<NetworkScoreboard>(FindObjectsInactive.Include);

            if (scoreboard != null)
            {
                scoreboard.RegisterPlayer(this);
            }
        }

        if (!IsOwner || !IsClient)
        {
            Debug.Log("Failed to bind local UI");
            return;
        }                

        Debug.Log("Binding UI");
        UI_Controller ui = FindFirstObjectByType<UI_Controller>(FindObjectsInactive.Include);

        if (ui != null)
        {
            Debug.Log("Successfully bound UI");
            ui.gameObject.SetActive(true);
            ui.Bind(this);
        }        
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            NetworkScoreboard scoreboard = FindFirstObjectByType<NetworkScoreboard>(FindObjectsInactive.Include);
            
            if (scoreboard != null)
            {
                scoreboard.UnregisterPlayer(OwnerClientId);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (!IsServer) return;

        health.CurrentHealth.Value = Mathf.Max(0, health.CurrentHealth.Value - amount);
    }

    public void AddScore(int amount)
    {
        if (!IsServer) return;

        Score.Value += amount;
    }

    public void LoseLife()
    {
        if (!IsServer) return;

        Lives.Value = Mathf.Max(0, Lives.Value - 1);
    }


}
