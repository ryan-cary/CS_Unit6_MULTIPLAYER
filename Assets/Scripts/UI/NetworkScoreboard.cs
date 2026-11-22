using System.Collections.Generic;
using UnityEngine;

public class NetworkScoreboard : MonoBehaviour
{
    [SerializeField] private Transform rowContainer;
    [SerializeField] private ScoreboardRow rowPrefab;

    private readonly Dictionary<ulong, ScoreboardRow> rows = new();
    private readonly Dictionary<ulong, NetworkPlayerStats> players = new();

    public void RegisterPlayer(NetworkPlayerStats player)
    {
        ulong id = player.OwnerClientId;

        // - Prevent duplicate registrations
        if (players.ContainsKey(id))
            return;

        players.Add(id, player);

        // - Create local UI row
        ScoreboardRow row = Instantiate(rowPrefab, rowContainer);
        rows.Add(id, row);

        // - Subscribe to network changes
        player.health.CurrentHealth.OnValueChanged += (oldValue, newValue) => RefreshPlayer(id);
        player.Lives.OnValueChanged += (oldValue, newValue) => RefreshPlayer(id);
        player.Score.OnValueChanged += (oldValue, newValue) => RefreshPlayer(id);

        RefreshPlayer(id);
    }

    public void UnregisterPlayer(ulong id)
    {
        if (!players.ContainsKey(id)) 
            return;

        players.Remove(id);

        if (rows.TryGetValue(id, out ScoreboardRow row))
        {
            Destroy(row.gameObject);
            rows.Remove(id);
        }
    }

    private void RefreshPlayer(ulong id)
    {
        if (!players.TryGetValue(id, out NetworkPlayerStats player))
            return;

        if (!rows.TryGetValue(id, out ScoreboardRow row))
            return;

        row.UpdateRow($"Player {id}", player.health.CurrentHealth.Value, player.Lives.Value, player.Score.Value);
    }
}
