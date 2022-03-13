/////////////////////////////////////////////////////////////////
/*
  ESP32 + UWB | Indoor Positioning + Unity Visualization
  For More Information: https://youtu.be/c8Pn7lS5Ppg
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////

//UDP Client-Server implementation in Unity
//https://github.com/manlaig/basic_multiplayer_unity

using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;

public class UDPServer : MonoBehaviour
{
    
    [SerializeField] int port = 8080;
    [SerializeField] string ipAddress = "";
    [SerializeField] TextMeshProUGUI mLogText;
    
    Socket udp;
    int idAssignIndex = 0;
    int frameWait = 2;
    
    void Start()
    {
        LogAdd("*UDP Server*");
        IPAddress ip = IPAddress.Parse(ipAddress);
        IPEndPoint endPoint = new IPEndPoint(ip, port);
        LogAdd("Server IP Address: " + ip);
        LogAdd("Port: " + port);
        udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        udp.Bind(endPoint);
        udp.Blocking = false;
    }

    void Update()
    {
        if (Time.frameCount % frameWait == 0 && udp.Available != 0)
        {
            byte[] packet = new byte[64];
            EndPoint sender = new IPEndPoint(IPAddress.Any, port);

            int rec = udp.ReceiveFrom(packet, ref sender);
            string info = Encoding.Default.GetString(packet);

            LogAdd("Received: " + info, true);
            FindObjectOfType<DataHandler>().setData(info);
        }
    }

    void LogAdd(string t, bool isData = false)
    {
        if (isData)
        {
            mLogText.text = t;
        }
        else
        {
            mLogText.text += "\n" + t;
        }
    }
}