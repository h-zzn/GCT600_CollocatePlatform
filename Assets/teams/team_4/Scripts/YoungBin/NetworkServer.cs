using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
//using UnityEditor.Analytics;

public class NetworkServer : MonoBehaviour
{
    [Header("Controller Settings")]
    public DisplayManager displayManager;

    [Header("Network Settings")]
    public int port = 5000;

    private UdpClient networkClient;
    private Thread receiveThread;
    private bool isRunning = false;

    private string receivedData = null;  // 초기값 null로 변경

    void Start()
    {
        StartServer();
    }

    private void OnApplicationQuit()
    {
        StopServer();
    }

    private void StartServer()
    {
        try
        {
            networkClient = new UdpClient(port);
            isRunning = true;
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            Debug.Log($"[Network Server] UDP Server started on port: {port}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Network Server] Error starting UDP server: {e.Message}");
        }
    }

    void StopServer()
    {
        isRunning = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        if (networkClient != null)
        {
            networkClient.Close();
        }

        Debug.Log("[Network Server] UDP Server stopped.");
    }

    private void ReceiveData()
    {
        while (isRunning)
        {
            try
            {
                
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

                byte[] data = networkClient.Receive(ref anyIP);

                string message = Encoding.UTF8.GetString(data);

                // 빈 메시지 무시
                if (string.IsNullOrWhiteSpace(message))
                {
                    Debug.LogWarning($"[Network Server] Received empty message from {anyIP.Address}:{anyIP.Port}");
                    continue;
                }

                lock (this)
                {
                    receivedData = message;
                }

                Debug.Log($"[Network Server] Received message from Client({anyIP.Address}:{anyIP.Port}): {message}");

            }
            catch (SocketException e)
            {
                if (isRunning)
                {
                    Debug.LogError($"[Network Server] Socket Exception: {e.Message}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Network Server] Exception in ReceiveData thread: {e.Message}");
            }
        }
    }

    private void Update()
    {
        string currentData = null;
        lock (this)
        {
            if (receivedData != null)
            {
                currentData = receivedData;
                receivedData = null; 
            }
        }

        // 빈 문자열 체크 추가
        if (!string.IsNullOrWhiteSpace(currentData))
        {
            if (displayManager != null)
            {
                currentData = currentData.Trim();
                Debug.Log($"[Network Server] Processing message: {currentData}");
                displayManager.ChangeImage(currentData);
            }
        }
    }
}