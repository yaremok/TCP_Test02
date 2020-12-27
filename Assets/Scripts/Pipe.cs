using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public class Pipe : MonoBehaviour
{
    public int selfPort;
    public int remotePort;
    public string status;
    public bool ready;
    public bool isIncomingData;


    private Thread udpListenerThread;
    UdpClient udpClient;
    private TcpListener tcpListener; 	
	private Thread tcpListenerThread;
    private bool isRunning;
    private bool isServer;
    private TcpClient connectedTcpClient; 	
    private Thread clientReceiveThread;
    private IPEndPoint ep;
    private TcpClient socketConnection; 
    private string incomingData;


    private void Start() 
    {
        status = "Wait for command";
    }

    private void Update() 
    {
        //Debug.Log(status);
    }

    public void Connect()
    {
        status = "Connecting...";
        UDPSend();
    }


    public void Stop()
    {
        isRunning = false;
    }


    public void pipeSendData(string data) 
    {
        if (isServer)
        {
            SendServerMessage(data);
        }
        else
        {
            SendClientMessage(data);
        }
    }


    public string pipeReceiveData()
    {
        isIncomingData = false;
        return incomingData;
    }

    private void UDPSend()
    {
        udpClient = new UdpClient(selfPort);
        //udpClient.ExclusiveAddressUse = false;
        //udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        try
        {

            //udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, remotePort));
        
            udpListenerThread = new Thread (new ThreadStart(ListenForIncommingUDPRequests)); 		
            udpListenerThread.IsBackground = true; 		
            udpListenerThread.Start(); 	

            var data = Encoding.ASCII.GetBytes("ping");
            IPEndPoint epRemote = new IPEndPoint(IPAddress.Parse("255.255.255.255"), remotePort);
            udpClient.Send(data, data.Length, epRemote);
        }
        catch (SocketException socketException)
        {
            Debug.Log(socketException.ToString());
            udpClient.Close();
        }
    }


    private void ListenForIncommingUDPRequests()
    {
        int waitAnswerForMillis = 3000;
        var from = new IPEndPoint(0, 0);
        string selfIP = GetLocalIPAddress();

        byte[] recvBuffer = new byte[0];
        udpClient.Client.ReceiveTimeout = waitAnswerForMillis;

        bool answered = false;
        int callCount = 3;
        while (callCount > 0 && !answered)
        {
            Debug.Log("Trys left: " + callCount);
            status = "Trys left: " + callCount;
            try
            {
                recvBuffer = udpClient.Receive(ref from);
            }
            catch (SocketException socketException)
            {
                Debug.Log(socketException.ToString());
            }

            if (recvBuffer.Length > 0)
            {
                if (from.Address.ToString() != selfIP || from.Port != selfPort)
                {
                    answered = true;
                    Debug.Log("Answer: " + Encoding.ASCII.GetString(recvBuffer));
                    Debug.Log("From: " + from.Address + ":" + from.Port);

                    Debug.Log("Run client");
                    ep = from;
                    RunClient();
                }
            }

            callCount--;
        }

        if (!answered)
        {
            Debug.Log("Run server");
            RunServer(); 

            // Answer to anybody
            while (isRunning)
            {
                // infinite time-out 
                udpClient.Client.ReceiveTimeout = -1;
                recvBuffer = udpClient.Receive(ref from);
                if (from.Address.ToString() != selfIP || from.Port != selfPort)
                {
                    var dataToSend = Encoding.ASCII.GetBytes("pong");
                    udpClient.Send(dataToSend, dataToSend.Length, from);
                }
            }
            udpClient.Close();
        }
    }


    private string GetLocalIPAddress()
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip.ToString();
			}
		}
		throw new Exception("No network adapters with an IPv4 address in the system!");
	}


    private void RunClient()
    {
        status = "Run as client";
        isServer = false;
        try 
		{
			isRunning = true;
			clientReceiveThread = new Thread (new ThreadStart(ClientListenForData)); 			
			clientReceiveThread.IsBackground = true; 			
			clientReceiveThread.Start();  	
            ready = true;	
		} 		
		catch (Exception e) 
		{
            Debug.Log(e.ToString());
            //TextOutLog("On client connect exception " + e)	;
		} 	
    }


    private void ClientListenForData() 
	{ 		
		try 
		{
			socketConnection = new TcpClient();  	
            socketConnection.Connect(ep);
			//TextOutLog("Client running");
			Byte[] bytes = new Byte[1024];             
			while (isRunning) 
			{ 							
				using (NetworkStream stream = socketConnection.GetStream()) 
				{ 					
					int length; 					 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) 
					{ 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 												
						string serverMessage = Encoding.ASCII.GetString(incommingData); 						 	
                        //TextInLog("server message received as: " + serverMessage);	
                        incomingData = serverMessage;
                        isIncomingData = true;
					} 				
				} 			
			} 
			socketConnection.Close();      
		}         
		catch (SocketException socketException) 
		{  
            Debug.Log(socketException.ToString());
            //TextOutLog("Socket exception: " + socketException);      
		}
		finally
		{
			socketConnection.Close(); 
		}  
	}  	


    private void SendClientMessage(string clientMessage) 
	{         
		if (socketConnection == null) 
		{             
			return;         
		}  		

		try 
		{ 						
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) 
			{                 
				//clientMessage = "This is a message from one of your clients: " + clientMessage;               
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                //TextOutLog("Client sent his message - should be received by server");           
			}         
		} 		
		catch (SocketException socketException) 
		{   
            Debug.Log(socketException.ToString());
            //TextOutLog("Socket exception: " + socketException);    
		}     
	}


    private void RunServer()
    {
        status = "Run as server";
        isServer = true;
        isRunning = true;
        tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests)); 		
		tcpListenerThread.IsBackground = true; 		
		tcpListenerThread.Start(); 
        ready = true;
    }


    private void ListenForIncommingRequests () { 		
		try 
		{
			string localIPAddress = GetLocalIPAddress();
			// Create listener on localhost

            if (ep == null)
            {
                ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), selfPort);
            }
			tcpListener = new TcpListener(IPAddress.Any, ep.Port);			
            
			tcpListener.Start(); 
            
			string eps = localIPAddress + ":" + ep.Port;
			string st = "Server is listening at " + eps;
            Debug.Log(st);
            status = st;
           
			Byte[] bytes = new Byte[1024];  			
			while (isRunning) 
			{ 				
				using (connectedTcpClient = tcpListener.AcceptTcpClient()) 
				{
					//TextOutLog("Client connected");						
					using (NetworkStream stream = connectedTcpClient.GetStream()) 
					{ 						
						int length; 											
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) 
						{ 							
							var incommingData = new byte[length]; 							
							Array.Copy(bytes, 0, incommingData, 0, length);  														
							string clientMessage = Encoding.ASCII.GetString(incommingData);		
                            //TextInLog("client message received as: " + clientMessage); 
                            incomingData = clientMessage;
                            isIncomingData = true;
						} 					
					} 				
				} 	
				connectedTcpClient.Close();		
			} 
			tcpListener.Stop();
		} 		
		catch (SocketException socketException) 
		{ 					
            //TextOutLog("SocketException " + socketException.ToString());  
            Debug.Log(socketException.ToString());
		}  
		finally
		{
			tcpListener.Stop();
		}
	}


    private void SendServerMessage(string serverMessage) 
	{ 		
		if (connectedTcpClient == null) 
		{             
			return;         
		}  		
		
		try 
		{ 						
			NetworkStream stream = connectedTcpClient.GetStream(); 			
			if (stream.CanWrite) 
			{                 
				//serverMessage = "This is a message from your server: " + serverMessage; 			                
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage); 				             
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);      
				stream.Flush();          
                //TextOutLog("Server sent his message - should be received by client");         
			}       
		} 		
		catch (SocketException socketException) 
		{
            Debug.Log(socketException.ToString());
            //TextOutLog("Socket exception: " + socketException);         
		} 	
	} 
}
