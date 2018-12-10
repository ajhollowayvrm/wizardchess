using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class s_TCP : MonoBehaviour
{
    #region private members     
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    //private Thread mainThread;
    //private string mes;
    #endregion
    // Use this for initialization  
    void Start()
    {
        //mainThread = System.Threading.Thread.CurrentThread;
        ConnectToTcpServer();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessage();
        }
    }

    /// <summary>   
    /// Setup socket connection.    
    /// </summary>  
    private void ConnectToTcpServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }
    /// <summary>   
    /// Runs in background clientReceiveThread; Listens for incomming data.     
    /// </summary>     
    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient("localhost", 5001);
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                // Get a stream object for reading              
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    
                    // Read incomming stream into byte arrary.
                    while ((length = stream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 

                        string serverMessage = Encoding.ASCII.GetString(incommingData);
                        Debug.Log("server message received as: " + serverMessage);
                        //mes = serverMessage;
                        //JObject json = JObject.Parse(str);
                        Dictionary<string, string> dict = new Dictionary<string, string>();
                        ExampleMainThreadCall(serverMessage);
                    }
                    
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public void ExampleMainThreadCall(string serverMessage)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(ThisWillBeExecutedOnTheMainThread(serverMessage));
    }

    public IEnumerator ThisWillBeExecutedOnTheMainThread(string serverMessage)
    {
        Worker(serverMessage);
        yield return null;
    }

    public void Worker(string serverMessage)
    {
        List<String> serverMessageArr;
        serverMessageArr = serverMessage.Split(' ').ToList();

        switch (serverMessageArr[0]) 
        {
            case "play":
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                break;
            case "move":
                GameManager.instance.alexaMove(serverMessageArr[1], serverMessageArr[2]);
                break;
            default:
                return;
        }

        //GameObject.Find("Play").GetComponentInChildren<Text>().text = mes;

        
    }

    /// <summary>   
    /// Send message to server using socket connection.     
    /// </summary>  
    private void SendMessage()
    {
        if (socketConnection == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing.             
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                string clientMessage = "This is a message from one of your clients.";
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Client sent his message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
}