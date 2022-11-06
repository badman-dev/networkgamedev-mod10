using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class GameManager : NetworkBehaviour {
    public Player playerPrefab;
    public GameObject spawnPoints;

    private int spawnIndex = 0;
    private List<Vector3> availableSpawnPositions = new List<Vector3>();

    private List<Player> players = new List<Player>();

    public void Awake()
    {
        RefreshSpawnPoints();
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            SpawnPlayers();
        }
    }

    private void RefreshSpawnPoints()
    {
        Transform[] allPoints = spawnPoints.GetComponentsInChildren<Transform>();
        availableSpawnPositions.Clear();
        foreach (Transform point in allPoints)
        {
            if (point != spawnPoints.transform)
            {
                availableSpawnPositions.Add(point.localPosition);
            }
        }
    }

    public Vector3 GetNextSpawnLocation()
    {
        var newPosition = availableSpawnPositions[spawnIndex];
        spawnIndex += 1;

        if (spawnIndex > availableSpawnPositions.Count - 1)
        {
            spawnIndex = 0;
        }

        return newPosition;

    }

    private void SpawnPlayers()
    {
        foreach(PlayerInfo pi in GameData.Instance.allPlayers)
        {
            Player playerSpawn = Instantiate(playerPrefab, GetNextSpawnLocation(), Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(pi.clientId);
            playerSpawn.PlayerColor.Value = pi.color;
            players.Add(playerSpawn);
        }
    }

    public void EndGame()
    {
        Player[] ps = FindObjectsOfType<Player>();
        foreach (Player p in ps)
        {
            Debug.Log(p.gameObject.name);
            p.gameObject.GetComponentInChildren<BulletSpawner>().enabled = false;
            p.SetEndText();
        }
    }
}