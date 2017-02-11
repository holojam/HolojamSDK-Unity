//Client.cs
//Created by Holojam Inc. on 11.11.16

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Holojam.Network {
  /// <summary>
  /// 
  /// </summary>
  public class Client : Utility.Global<Client> {

    //Connection options
    public string serverAddress = "0.0.0.0";
    public int upstreamPort = 9592;
    public string multicastAddress = "239.0.2.4";
    public int downstreamPort = 9591;
    public string sendScope = "Unity";

    List<View> staged, untracked;
    Emitter emitter; Sink sink;

    //Debug
#if UNITY_EDITOR
    public int sentPPS, receivedPPS;
    public List<string> threadData = new List<string>();
#endif

    /// <summary>
    /// 
    /// </summary>
    public static string SEND_SCOPE {
      get { return global.sendScope; }
    }

    //Global event functions
    /// <summary>
    /// 
    /// </summary>
    /// <param name="view"></param>
    public static void PushEvent(View view) {
      global.emitter.SendEvent(view);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="label"></param>
    public static void Notify(string label) {
      global.emitter.SendNotification(label);
    }

    /// <summary>
    /// 
    /// </summary>
    void Awake() {
      staged = new List<View>();
      untracked = new List<View>();

      emitter = new Emitter(serverAddress, upstreamPort);
      sink = new Sink(multicastAddress, downstreamPort);
      //Start receive thread
      sink.Start();

      //Debug
      #if UNITY_EDITOR
      StartCoroutine(Display());
      #endif
    }

    /// <summary>
    /// Core update loop (rate = Time.fixedDeltaTime)
    /// </summary>
    void FixedUpdate() {
      staged.Clear(); untracked.Clear();

      //Stage views
      foreach (View view in View.instances) {
        if (string.IsNullOrEmpty(view.label)) {
          Debug.LogWarning("Holojam.Network.Client: Invalid view label", view);
          continue;
        } else if (view.ignoreTracking)
          continue;
        else if (view.sending)
          staged.Add(view);
        else {
          //view.tracked = false;
          untracked.Add(view);
        }
      }

      sink.Update(untracked);
      emitter.Send(staged);
    }

    /// <summary>
    /// Process events in realtime
    /// </summary>
    void Update() { sink.PublishEvents(); }

    //Debug
#if UNITY_EDITOR
    IEnumerator Display() {
      while (sink.running) {
        yield return new WaitForSeconds(1);

        threadData.Clear();
        threadData.Add(emitter.ResetDebugData());
        threadData.Add(sink.ResetDebugData());

        sentPPS = emitter.ResetPacketCount();
        receivedPPS = sink.ResetPacketCount();
      }
    }
#endif

    void OnDestroy() {
      sink.Stop();
    }
  }
  
  /// <summary>
  /// 
  /// </summary>
  internal abstract class Exchange {

    protected const int SOCKET_TIMEOUT = 1000; //ms
    protected const int PACKET_SIZE = 65507; //bytes

    protected byte[] buffer;
    protected string address;
    protected int port;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    protected Exchange(string address, int port) {
      this.address = address;
      this.port = port;
    }

    //Debug
#if UNITY_EDITOR
    protected int packetCount = 0;
    public int ResetPacketCount() {
      int count = packetCount;
      packetCount = 0;
      return count;
    }
    public abstract string ResetDebugData();
#endif
  }

  /// <summary>
  /// 
  /// </summary>
  internal class Emitter : Exchange {

    IPAddress ip;
    Socket socket;
    IPEndPoint sendEndPoint;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    internal Emitter(string address, int port) : base(address, port) {
      ip = IPAddress.Any;

      //Initialize
      socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
      socket.SendTimeout = SOCKET_TIMEOUT;

      IPEndPoint endPoint = new IPEndPoint(ip, 0);
      sendEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

      try { socket.Bind(endPoint); } catch (SocketException e) {
        Debug.LogWarning("Holojam.Network.Client: Error binding socket: "
           + address + " " + port + " " + e.ToString());
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="views"></param>
    public void Send(List<View> views) {
      if (views.Count == 0) return;
      buffer = Translator.BuildUpdate(Client.SEND_SCOPE, views);
      Send();

#if UNITY_EDITOR
      debugData = "(" + Translator.Origin() + ")";
      foreach (View view in views)
        debugData += "\n   " + view.label;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="view"></param>
    public void SendEvent(View view) {
      buffer = Translator.BuildEvent(Client.SEND_SCOPE, view);
      Send();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="label"></param>
    public void SendNotification(string label) {
      buffer = Translator.BuildNotification(Client.SEND_SCOPE, label);
      Send();
    }

    /// <summary>
    /// 
    /// </summary>
    void Send() {
      //Unicast
      socket.BeginSendTo(buffer, 0, buffer.Length, 0, sendEndPoint,
         (System.IAsyncResult r) => { socket.EndSendTo(r); },
      socket);

#if UNITY_EDITOR
      packetCount++;
#endif
    }

#if UNITY_EDITOR
    string debugData = "";
    public override string ResetDebugData() {
      string data = "Port " + port + ":\n"
         + (debugData == "" ? "   (Empty)" : debugData);
      debugData = "";
      return data;
    }
#endif

  }

  /// <summary>
  /// 
  /// </summary>
  internal class Sink : Exchange {
    const float UPDATE_TIMEOUT = 1; //s

    public bool running;

    Thread thread;
    UnityEngine.Object lockObject;
    Stack<Update> updates;
    Stack<Event> events;
    float lastUpdate;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    public Sink(string address, int port) : base(address, port) {
      running = false;
      thread = new Thread(listener);
      lockObject = new UnityEngine.Object();
      updates = new Stack<Update>();
      events = new Stack<Event>();
      lastUpdate = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="untracked"></param>
    public void Update(List<View> untracked) {
      lock (lockObject) {
        bool updated = updates.Count > 0; //Was there an update this tick?
        while (updates.Count > 0) {
          Update update = updates.Pop();
#if UNITY_EDITOR
          update.UpdateDebug(debugData);
#endif
          //Look for tracked flakes in the most recent update
          for (int i = 0; i < untracked.Count; ++i)
            if (update.Load(untracked[i]))
              untracked.RemoveAt(i--);
        }
        //Set untracked if the update timed out or if the view was not found this update
        if (updated || Time.time - lastUpdate > UPDATE_TIMEOUT) {
          foreach (View view in untracked)
            view.tracked = false;
          lastUpdate = Time.time;
        }
      }
    }

    /// <summary>
    /// Publish events to the notifier.
    /// </summary>
    public void PublishEvents() {
      while (events.Count > 0)
        Notifier.Publish(events.Pop());
    }

    /// <summary>
    /// 
    /// </summary>
    ThreadStart listener {
      get {
        return () => {
          //Initialize
          Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
          socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

          socket.Bind(new IPEndPoint(IPAddress.Any, port));
          socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
             new MulticastOption(IPAddress.Parse(address)));
          //Critical, otherwise the thread will never terminate
          socket.ReceiveTimeout = SOCKET_TIMEOUT;

          //Receive loop
          while (running) {
            //Get packet (blocking)
            try {
              buffer = new byte[PACKET_SIZE];
              if (socket.Receive(buffer) == 0) continue;
            } catch (SocketException e) {
#if UNITY_EDITOR
              if (e.ErrorCode != 10035 && e.ErrorCode != 10060)
                Debug.LogError("Holojam.Network.Client: Socket error: " + e);
#endif
              Debug.Log("Holojam.Client.Network: Timeout");
              continue;
            }

            Nugget nugget = Nugget.Create(ref buffer);
            if (nugget is Update) {
              lock (lockObject) { updates.Push(nugget as Update); }
            } else {
              lock (lockObject) { events.Push(nugget as Event); }
            }

#if UNITY_EDITOR
            packetCount++;
#endif
          }
          socket.Close();
        };
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Start() {
      running = true;
      thread.Start();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Stop() {
      running = false;
    }

#if UNITY_EDITOR
    Dictionary<string, string> debugData = new Dictionary<string, string>();
    public override string ResetDebugData() {
      string data = "Port " + port + ":"
         + (debugData.Count == 0 ? "\n   (Empty)" : "");
      foreach (var entry in debugData)
        data += "\n   " + entry.Key + " (" + entry.Value + ")";
      debugData.Clear();
      return data;
    }
#endif
  }
}
