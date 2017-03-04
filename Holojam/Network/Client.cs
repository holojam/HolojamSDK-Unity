// Client.cs
// Created by Holojam Inc. on 11.11.16

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Holojam.Network.Translation;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Holojam.Network {

  /// <summary>
  /// Native C# Holojam network client endpoint with optimized send/receive functionality.
  /// Incoming (downstream) packets are processed on a separate thread. Outgoing (upstream)
  /// packets are processed on a separate thread at a fixed rate and sent asynchronously.
  /// </summary>
  public class Client : Utility.Global<Client> {

    [SerializeField] string relayAddress = "0.0.0.0";
    [SerializeField] int upstreamPort = 9592;
    [SerializeField] string multicastAddress = "239.0.2.4";
    [SerializeField] int downstreamPort = 9591;

    /// <summary>
    /// Address of the central Holojam relay node.
    /// </summary>
    public string RelayAddress { get { return relayAddress; } }

    public int UpstreamPort { get { return upstreamPort; } }
    public string MulticastAddress { get { return multicastAddress; } }
    public int DownstreamPort { get { return downstreamPort; } }

    /// <summary>
    /// Global namespace for outgoing updates and events.
    /// </summary>
    public string sendScope = "Unity";

    public enum Rate { MOBILE, DESKTOP, HIGH, UNLOCKED };

    /// <summary>
    /// The emission rate of the Client. MOBILE = 30 updates per second, DESKTOP = 60 updates per
    /// second. Capped emission rates induce some additional latency but enforce consistency and
    /// reduce bandwidth and CPU requirements. In general, you can cap to a rate 'x' if your
    /// render FPS is at least 2 * x + 1.
    /// If you're manually capping your framerate and/or using VSync, select UNLOCKED.
    /// When unlocked, the Client will attempt to send (update) packets as fast as Unity renders.
    /// Note: Events are always sent as soon as they are pushed.
    /// </summary>
    public Rate rate = Rate.DESKTOP;

    /// <summary>
    /// Minimum time (ms) between updates.
    /// </summary>
    internal static long REFRESH_PERIOD {
      get {
        switch (global.rate) {
          case Rate.MOBILE:
            return 33;
          case Rate.DESKTOP:
            return 17;
          case Rate.HIGH:
            return 11;
        }
        return 0;
      }
    }

    List<Controller> staged, untracked;
    Emitter emitter; Sink sink;

    // Debug
    #if UNITY_EDITOR
    public int sentPPS, receivedPPS;
    public List<string> threadData = new List<string>();
    public bool advanced = false;
    float frameTime = 0;
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

    /// <summary>
    /// Changes the IP address used to send data upstream to the Holojam relay node.
    /// Automatically reconnects to the relay with the updated address.
    /// </summary>
    /// <param name="address">
    /// The IP address of the relay you want to connect to. Must be a valid IP address.
    /// </param>
    public void ChangeRelayAddress(string address) {
      relayAddress = address;
      Restart();
    }

    /// <summary>
    /// Changes the various client settings, and automatically restarts the client with the
    /// updated addresses and ports.
    /// </summary>
    /// <param name="relayAddress"></param>
    /// <param name="upstreamPort"></param>
    /// <param name="multicastAddress"></param>
    /// <param name="downstreamPort"></param>
    public void ChangeClientSettings(
      string relayAddress, int upstreamPort,
      string multicastAddress, int downstreamPort
    ){
      this.relayAddress = relayAddress;
      this.upstreamPort = upstreamPort;
      this.multicastAddress = multicastAddress;
      this.downstreamPort = downstreamPort;
      Restart();
    }

    /// <summary>
    /// Starts the threads for sending and receiving data, using the server settings configured
    /// in the inspector.
    /// </summary>
    internal void Restart() {
      Stop(); // Make sure nothing is running

      emitter = new Emitter(relayAddress, upstreamPort);
      sink = new Sink(multicastAddress, downstreamPort);

      // Start threads
      sink.Start();
      emitter.Start();
    }

    /// <summary>
    /// Stops the threads for sending and receiving data.
    /// </summary>
    internal void Stop() {
      if (sink != null) { sink.Stop(); }
      if (emitter != null) { emitter.Stop(); }
    }

    void Awake() {
      staged = new List<Controller>();
      untracked = new List<Controller>();

      Restart();
    }

    /// <summary>
    /// Publishes events, triggers emission thread.
    /// </summary>
    void Update() {
      // Process events as soon as possible
      sink.PublishEvents();

      // Debug
      #if UNITY_EDITOR
      frameTime += Time.deltaTime;
      if (frameTime > 1) {
        Display();
        frameTime = 0;
      }
      #endif

      Refresh(); // Update sink as soon as possible and stage Controllers for sending

      if (emitter.Blocking) {
        //UnityEngine.Debug.Log("Holojam.Network.Client: Cap blocking main thread");
        return;
      }

      if (REFRESH_PERIOD > 0) {
        if (emitter.PrepareUpdate(staged))
          emitter.OnRefresh(); // Trigger emitter
      } else emitter.ForceUpdate(staged); // In the case of UNLOCKED, just send immediately
    }

    /// <summary>
    /// Stages Controllers for sending/receiving and updates the sink (incoming data).
    /// </summary>
    void Refresh() {
      staged.Clear(); untracked.Clear();

      // Stage Controllers
      foreach (Controller controller in Controller.instances) { // Grab all Controllers in the scene
        if (string.IsNullOrEmpty(controller.Label)) {
          UnityEngine.Debug.LogWarning("Holojam.Network.Client: Invalid Controller label", controller);
          continue;
        } else if (controller.Sending) {
          staged.Add(controller);
        } else if (controller.Deaf) { // Ignore deaf Controllers
          continue;
        } else {
          untracked.Add(controller);
        }

        // Update the View
        #if UNITY_EDITOR
        EditorUtility.SetDirty((UnityEngine.Object)controller);
        #endif
      }

      sink.Update(untracked);
    }

    // Debug
    #if UNITY_EDITOR
    void Display() {
      threadData.Clear();
      threadData.Add(emitter.ResetDebugData());
      threadData.Add(sink.ResetDebugData());

      sentPPS = emitter.ResetPacketCount();
      receivedPPS = sink.ResetPacketCount();
    }
    #endif

    void OnDestroy() {
      Stop();
    }

    // Editor
    void OnDrawGizmos() {
      Gizmos.DrawIcon(transform.position, "holojam.png", true);
    }
  }

  /// <summary>
  /// Abstract base class for the emitter and sink. Contains socket and threading structure.
  /// Less of a design paradigm, more of a practical implementation.
  /// </summary>
  internal abstract class Exchange {

    protected const int SOCKET_TIMEOUT = 1000; //ms
    protected const int PACKET_SIZE = 65507; //bytes

    public bool Running { get { return running; } }
    bool running;

    protected byte[] buffer;
    protected string address;
    protected int port;

    protected abstract ThreadStart Run { get; } // Overriden for thread loops
    Thread thread;

    protected UnityEngine.Object ThreadLock { get { return threadLock; } }
    UnityEngine.Object threadLock;

    /// <summary>
    /// Initialize with an address and port.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    protected Exchange(string address, int port) {
      threadLock = new UnityEngine.Object();

      this.address = address;
      this.port = port;

      running = false;
      thread = new Thread(Run);
    }

    /// <summary>
    /// Start the thread.
    /// </summary>
    public void Start() {
      running = true;
      thread.Start();
    }

    /// <summary>
    /// Stop the thread.
    /// </summary>
    public void Stop() {
      running = false;
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

    /// <summary>
    /// Is the emitter blocking the main thread?
    /// </summary>
    public bool Blocking {
      get { lock (ThreadLock) { return blocking; } }
    }
    bool blocking;

    IPAddress ip;
    Socket socket;
    IPEndPoint sendEndPoint;

    ManualResetEvent refresh;
    UnityEngine.Object sendLock;

    /// <summary>
    /// Send update thread, which pushes updates at a fixed rate specified by the Client.
    /// Waits for a Refresh on the main thread before triggering a delayed send.
    /// </summary>
    protected override ThreadStart Run {
      get {
        return () => {
          // High-resolution timer
          Stopwatch timer = new Stopwatch();
          timer.Start();

          while (Running) {
            if (!refresh.WaitOne(1000)) // Wait for refresh
              continue;

            long delay = Client.REFRESH_PERIOD
              - timer.Elapsed.Milliseconds % Client.REFRESH_PERIOD;
            Thread.Sleep((int)delay);

            // Send update
            lock (ThreadLock) { blocking = false; }
            Send();
            refresh.Reset();
          }
        };
      }
    }

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
        UnityEngine.Debug.LogWarning("Holojam.Network.Client: Error binding socket: "
           + address + " " + port + " " + e.ToString());
      }

      refresh = new ManualResetEvent(false);
      blocking = false;
      sendLock = new UnityEngine.Object();
    }

    /// <summary>
    /// Called on Refresh on the main thread, triggering an update in the emitter.
    /// </summary>
    public void OnRefresh() {
      blocking = true;
      refresh.Set();
    }

    /// <summary>
    /// Asynchronously pack (but don't send) a Holojam update given a list of Controllers.
    /// </summary>
    /// <param name="controllers"></param>
    /// <returns>True if there is a nonzero number of Controllers, false otherwise.</returns>
    public bool PrepareUpdate(List<Controller> controllers) {
      if (controllers.Count == 0) return false;
      lock (sendLock) {
        buffer = Translator.BuildUpdate(controllers);
      }

      #if UNITY_EDITOR
      debugData = "(" + Canon.Origin() + ")";
      foreach (Controller controller in controllers)
        debugData += "\n   " + controller.Label;
      #endif

      return true;
    }

    /// <summary>
    /// Force an update send outside the fixed cap.
    /// </summary>
    public void ForceUpdate(List<Controller> controllers) {
      if(PrepareUpdate(controllers))
        Send();
    }

    /// <summary>
    /// Asynchronously send a Holojam event given a single Flake.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="flake"></param>
    public void SendEvent(string label, Flake flake) {
      lock (sendLock) {
        buffer = Translator.BuildEvent(label, flake);
      }
      Send();
    }

    /// <summary>
    /// Asynchronously send a Holojam notification given a label (string).
    /// </summary>
    /// <param name="label"></param>
    public void SendNotification(string label) {
      lock (sendLock) {
        buffer = Translator.BuildNotification(label);
      }
      Send();
    }

    /// <summary>
    /// Asynchronously send a buffer upstream.
    /// </summary>
    void Send() {
      lock (sendLock) {
        // Unicast
        socket.BeginSendTo(buffer, 0, buffer.Length, 0, sendEndPoint,
           (System.IAsyncResult r) => { socket.EndSendTo(r); },
        socket);

        #if UNITY_EDITOR
        packetCount++;
        #endif
      }
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

    Stack<Translation.Update> updates;
    Stack<Translation.Event> events;
    float lastUpdate;

    /// <summary>
    /// Initializes downstream, multicast socket given server address and downstream port.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    internal Sink(string address, int port) : base(address, port) {
      updates = new Stack<Translation.Update>();
      events = new Stack<Translation.Event>();
      lastUpdate = 0;
    }

    /// <summary>
    /// Given a list of untracked Controllers, pop updates off the stack
    /// (processing them chronologically). Update the untracked Controller collection.
    /// </summary>
    /// <param name="untracked"></param>
    public void Update(List<Controller> untracked) {
      lock (ThreadLock) {
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
    protected override ThreadStart Run {
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
          while (Running) {
            // Get packet (blocking)
            try {
              buffer = new byte[PACKET_SIZE];
              if (socket.Receive(buffer) == 0) continue;
            } catch (SocketException e) {
              #if UNITY_EDITOR
              if (e.ErrorCode != 10035 && e.ErrorCode != 10060) // Timeouts
                UnityEngine.Debug.LogError("Holojam.Network.Client: Socket error: " + e);
              #endif
              //UnityEngine.Debug.Log("Holojam.Client.Network: Timeout");
              continue;
            }

            Nugget nugget = Nugget.Create(ref buffer);

            // Push to stack(s)
            if (nugget is Update)
              lock (ThreadLock) { updates.Push(nugget as Translation.Update); }
            else
              lock (ThreadLock) { events.Push(nugget as Translation.Event); }

            #if UNITY_EDITOR
            packetCount++;
            #endif
          }
          socket.Close();
        };
      }
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
