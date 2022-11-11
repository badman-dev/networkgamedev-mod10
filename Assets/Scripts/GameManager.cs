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
            NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnect;
            NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
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
            SpawnPlayer(pi);
        }
    }

    private void SpawnPlayer(PlayerInfo info)
    {
        Player playerSpawn = Instantiate(playerPrefab, GetNextSpawnLocation(), Quaternion.identity);
        playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(info.clientId);
        playerSpawn.PlayerColor.Value = info.color;
        players.Add(playerSpawn);
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

    private void HostOnClientDisconnect(ulong clientId)
    {
        NetworkObject nObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        Player pObject = nObject.GetComponent<Player>();
        players.Remove(pObject);
        Destroy(pObject);
    }

    private void HostOnClientConnected(ulong clientId)
    {
        int playerIndex = GameData.Instance.FindPlayerIndex(clientId);
        if (playerIndex != -1)
        {
            PlayerInfo newPlayerInfo = GameData.Instance.allPlayers[playerIndex];
            SpawnPlayer(newPlayerInfo);
        }
    }
}