// Client.cs
// Created by Holojam Inc. on 11.11.16

using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Holojam.Network {

  /// <summary>
  /// Native C# Holojam network client endpoint with optimized send/receive functionality.
  /// Incoming (downstream) packets are processed on a separate thread. Outgoing (upstream)
  /// packets are processed asynchronously.
  /// </summary>
  public class Client : Utility.Global<Client> {

    // Connection options
    public string serverAddress = "0.0.0.0";
    public int upstreamPort = 9592;
    public string multicastAddress = "239.0.2.4";
    public int downstreamPort = 9591;
    public string sendScope = "Unity";

    List<Controller> staged, untracked;
    Emitter emitter; Sink sink;

    // Debug
    #if UNITY_EDITOR
    public int sentPPS, receivedPPS;
    public List<string> threadData = new List<string>();
    #endif

    /// <summary>
    /// Global scope (namespace) for packets coming out of the Unity project.
    /// </summary>
    public static string SEND_SCOPE {
      get { return global.sendScope; }
    }

    /// <summary>
    /// Push a (Holojam) notification to the Holojam network from Unity.
    /// </summary>
    /// <param name="label"></param>
    public static void Notify(string label) {
      global.emitter.SendNotification(label);
    }

    /// <summary>
    /// Push a (Holojam) event to the Holojam network from Unity.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="flake"></param>
    internal static void PushEvent(string label, Flake flake) {
      global.emitter.SendEvent(label, flake);
    }

    void Awake() {
      staged = new List<Controller>();
      untracked = new List<Controller>();

      emitter = new Emitter(serverAddress, upstreamPort);
      sink = new Sink(multicastAddress, downstreamPort);
      // Start receive thread
      sink.Start();

      // Debug
      #if UNITY_EDITOR
      StartCoroutine(Display());
      #endif
    }

    /// <summary>
    /// Core update loop (update rate = Time.fixedDeltaTime).
    /// Stages Controllers for sending/receiving, updates the emitter and sink.
    /// </summary>
    void FixedUpdate() {
      staged.Clear(); untracked.Clear();

      // Stage Controllers
      foreach (Controller controller in Controller.instances) { // Grab all Controllers in the scene
        if (string.IsNullOrEmpty(controller.Label)) {
          Debug.LogWarning("Holojam.Network.Client: Invalid Controller label", controller);
          continue;
        } else if (controller.Sending) {
          staged.Add(controller);
        } else {
          untracked.Add(controller);
        }
      }

      sink.Update(untracked);
      emitter.SendUpdate(staged);
    }

    /// <summary>
    /// Process events in realtime (contrast with FixedUpdate).
    /// </summary>
    void Update() { sink.PublishEvents(); }

    // Debug
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
  /// Abstract base class for the emitter and sink.
  /// Less of a design paradigm, more of a practical implementation.
  /// </summary>
  internal abstract class Exchange {

    protected const int SOCKET_TIMEOUT = 1000; //ms
    protected const int PACKET_SIZE = 65507; //bytes

    protected byte[] buffer;
    protected string address;
    protected int port;

    /// <summary>
    /// Initialize with an address and port.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    protected Exchange(string address, int port) {
      this.address = address;
      this.port = port;
    }

    // Debug
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
  /// Asynchronously sends packets upstream.
  /// </summary>
  internal sealed class Emitter : Exchange {

    IPAddress ip;
    Socket socket;
    IPEndPoint sendEndPoint;

    /// <summary>
    /// Initializes upstream, unicast socket given server address and upstream port.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    internal Emitter(string address, int port) : base(address, port) {
      ip = IPAddress.Any;

      // Initialize
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
    /// Asynchronously pack and send a Holojam update given a list of Controllers.
    /// </summary>
    /// <param name="controllers"></param>
    public void SendUpdate(List<Controller> controllers) {
      if (controllers.Count == 0) return;
      buffer = Translator.BuildUpdate(controllers);
      Send();

      #if UNITY_EDITOR
      debugData = "(" + Canon.Origin() + ")";
      foreach (Controller controller in controllers)
        debugData += "\n   " + controller.Label;
      #endif
    }

    /// <summary>
    /// Asynchronously send a Holojam event given a single Flake.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="flake"></param>
    public void SendEvent(string label, Flake flake) {
      buffer = Translator.BuildEvent(label, flake);
      Send();
    }

    /// <summary>
    /// Asynchronously send a Holojam notification given a label (string).
    /// </summary>
    /// <param name="label"></param>
    public void SendNotification(string label) {
      buffer = Translator.BuildNotification(label);
      Send();
    }

    /// <summary>
    /// Asynchronously send a buffer upstream.
    /// </summary>
    void Send() {
      // Unicast
      socket.BeginSendTo(buffer, 0, buffer.Length, 0, sendEndPoint,
         (System.IAsyncResult r) => { socket.EndSendTo(r); },
      socket);

      #if UNITY_EDITOR
      packetCount++;
      #endif
    }

    // Debug
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
  /// Receives/accumulates and processes packets via multicast (downstream) on a dedicated thread.
  /// </summary>
  internal sealed class Sink : Exchange {
    const float UPDATE_TIMEOUT = 1; //s

    public bool running;

    Thread thread;
    UnityEngine.Object lockObject;
    Stack<Update> updates;
    Stack<Event> events;
    float lastUpdate;

    /// <summary>
    /// Initializes downstream, multicast socket given server address and downstream port.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    internal Sink(string address, int port) : base(address, port) {
      running = false;
      thread = new Thread(listener);
      lockObject = new UnityEngine.Object();
      updates = new Stack<Update>();
      events = new Stack<Event>();
      lastUpdate = 0;
    }

    /// <summary>
    /// Given a persistent list of untracked Controllers, pop updates off the stack
    /// (processing them chronologically). Update the untracked Controller collection.
    /// </summary>
    /// <param name="untracked"></param>
    public void Update(List<Controller> untracked) {
      lock (lockObject) {
        bool updated = updates.Count > 0; // Was there an update this tick?
        while (updates.Count > 0) {
          Update update = updates.Pop();

          #if UNITY_EDITOR
          update.UpdateDebug(debugData);
          #endif

          // Look for tracked flakes in the most recent update
          for (int i = 0; i < untracked.Count; ++i)
            if (update.Load(untracked[i]))
              untracked.RemoveAt(i--);
        }
        // Set untracked if the update timed out or if the Controller was not found this update
        if (updated || Time.time - lastUpdate > UPDATE_TIMEOUT) {
          foreach (Controller controller in untracked)
            controller.Tracked = false;
          lastUpdate = Time.time;
        }
      }
    }

    /// <summary>
    /// Publish accumulated events to the notifier.
    /// </summary>
    public void PublishEvents() {
      while (events.Count > 0)
        Notifier.Publish(events.Pop());
    }

    /// <summary>
    /// Core receive loop, accumulating packets as quickly as possible. Raw buffers are translated
    /// and pushed to the packet stack(s) for processing on the main thread.
    /// </summary>
    ThreadStart listener {
      get {
        return () => {
          // Initialize
          Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
          socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

          socket.Bind(new IPEndPoint(IPAddress.Any, port));
          socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
             new MulticastOption(IPAddress.Parse(address)));
          // Critical, otherwise the thread will never terminate
          socket.ReceiveTimeout = SOCKET_TIMEOUT;

          // Receive loop
          while (running) {
            // Get packet (blocking)
            try {
              buffer = new byte[PACKET_SIZE];
              if (socket.Receive(buffer) == 0) continue;
            } catch (SocketException e) {
              #if UNITY_EDITOR
              if (e.ErrorCode != 10035 && e.ErrorCode != 10060) // Timeouts
                Debug.LogError("Holojam.Network.Client: Socket error: " + e);
              #endif
              Debug.Log("Holojam.Client.Network: Timeout");
              continue;
            }

            Nugget nugget = Nugget.Create(ref buffer);

            // Push to stack(s)
            if (nugget is Update)
              lock (lockObject) { updates.Push(nugget as Update); }
            else
              lock (lockObject) { events.Push(nugget as Event); }

            #if UNITY_EDITOR
            packetCount++;
            #endif
          }
          socket.Close();
        };
      }
    }

    /// <summary>
    /// Start the receive thread.
    /// </summary>
    public void Start() {
      running = true;
      thread.Start();
    }

    /// <summary>
    /// Stop the receive thread.
    /// </summary>
    public void Stop() {
      running = false;
    }

    // Debug
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
