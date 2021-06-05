using System;
using System.Collections;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Yyt : MonoBehaviour
{
    [SerializeField]
    Text debugText;

    [SerializeField]
    string applicationId;

    [SerializeField]
    string authUrl;

    [SerializeField]
    string serverUrl;
    
    [SerializeField]
    string testUserName;
    
    [SerializeField]
    string testUserEmail;

    string authorizationToken;
    ClientWebSocket webSocket;
    bool readyToReceive;

    [Serializable]
    class AuthSimplePostData
    {
        public string name;
        public string email;
        public string application;
    }

    [Serializable]
    class MatchRequestData
    {
        public string type;
        public string application;
    }

    IEnumerator Start()
    {
        yield return StartAuthCoro();

        EnterLobbyAsync();
        
        yield return new WaitUntil(() => readyToReceive);
        
        SendMatchAsync();
    }

    IEnumerator StartAuthCoro()
    {
        var data = new AuthSimplePostData
        {
            name = testUserName,
            email = testUserName,
            application = applicationId,
        };

        var jsonBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));

        using var www = new UnityWebRequest(authUrl)
        {
            method = UnityWebRequest.kHttpVerbPOST,
            timeout = 10,
            uploadHandler = new UploadHandlerRaw(jsonBytes) {contentType = "application/json"},
            downloadHandler = new DownloadHandlerBuffer()
        };
        yield return www.SendWebRequest();
        authorizationToken = www.downloadHandler.text;
        debugText.text = authorizationToken + "\n";
    }

    async void EnterLobbyAsync()
    {
        try
        {
            webSocket = new ClientWebSocket();
            var cts = new CancellationTokenSource();
            debugText.text += "Entering lobby...";
            await webSocket.ConnectAsync(new Uri($"{serverUrl}/lobby?authorization={authorizationToken}"),
                cts.Token);
            debugText.text += "Done!\n";

            var buffer = new byte[4096];
            while (webSocket.State == WebSocketState.Open)
            {
                debugText.text += "Waiting messages...";
                var resOp = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                readyToReceive = true;
                var res = await resOp;
                debugText.text += "Arrived.\n";
                if (res.MessageType == WebSocketMessageType.Close)
                {
                    debugText.text += "Close request received. Closing...\n";
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cts.Token);
                }
            
                debugText.text += Encoding.UTF8.GetString(buffer, 0, res.Count) + "\n";
            }
        
            debugText.text += "Disposing web socket...\n";
            webSocket.Dispose();
            webSocket = null;
        }
        catch (Exception e)
        {
            if (debugText != null)
            {
                debugText.text += e.ToString();
            }
        }
    }

    async void SendMatchAsync()
    {
        var cts = new CancellationTokenSource();
        
        var bytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(
            JsonUtility.ToJson(new MatchRequestData {type = "match", application = applicationId})));
        debugText.text += "Sending match...";
        await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, cts.Token);
        debugText.text += "Done!\n";
    }
}