using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Multiplayer.Tools;
using Unity.Netcode.Transports.UTP;
using System.Net;
using AddressFamily = System.Net.Sockets.AddressFamily;
using System.IO;
using System.Text;

public class Main : NetworkBehaviour {

    public Button btnHost;
    public Button btnClient;
    public TMPro.TMP_InputField inputIp;
    public TMPro.TMP_InputField inputPort;
    public TMPro.TMP_Text txtStatus;

    public void Start() {
        btnHost.onClick.AddListener(OnHostClicked);
        btnClient.onClick.AddListener(OnClientClicked);
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;
        Application.targetFrameRate = 30;

        ShowConnectionData();
    }

    private void StartHost(string sceneName = "Lobby", string startMessage = "Starting Host") {
        bool validSettings = ValidateSettings();
        if (!validSettings)
        {
            return;
        }

        txtStatus.text = startMessage;

        NetworkManager.Singleton.StartHost();
        NetworkManager.SceneManager.LoadScene(
            sceneName,
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void StartClient(string startMessage = "Starting Client")
    {
        bool validSettings = ValidateSettings();
        if (!validSettings)
        {
            return;
        }

        txtStatus.text = startMessage;

        NetworkManager.Singleton.StartClient();
        txtStatus.text = "Waiting on Host";
    }

    private void ShowConnectionData()
    {
        var curSettings = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData;
        inputIp.text = curSettings.Address.ToString();
        inputPort.text = curSettings.Port.ToString();
    }

    private bool ValidateSettings()
    {
        IPAddress ip;
        bool isValidIp = IPAddress.TryParse(inputIp.text, out ip);
        if (!isValidIp)
        {
            txtStatus.text = "Invalid IP";
            return false;
        }

        bool isValidPort = ushort.TryParse(inputPort.text, out ushort port);
        if (!isValidPort)
        {
            txtStatus.text = "Invalid Port";
            return false;
        }

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip.ToString(), port);

        btnHost.gameObject.SetActive(false);
        btnClient.gameObject.SetActive(false);
        inputIp.enabled = false;
        inputPort.enabled = false;

        return true;
    }

    private void OnHostClicked() {
        StartHost();
    }

    private void OnClientClicked() {
        StartClient();
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnect;
        }
    }

    private void OnDisconnect(ulong clientId)
    {
        txtStatus.text = "Failed to connect to server";

        //btnHost.gameObject.SetActive(true);
        //btnClient.gameObject.SetActive(true);
        //inputIp.enabled = true;
        //inputPort.enabled = true;
    }
}
