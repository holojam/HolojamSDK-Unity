using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using ProtoBuf;
using update_protocol_v3;
using System.Threading;

namespace Holojam.Network {
	public class HolojamNetwork : Singleton<HolojamNetwork> {

		public string multicastAddress = "224.1.1.1", serverAddress = "192.168.1.44";
		public int multicastPort = 1611, serverPort = 1611;

		//Editor debugging
		public int sentWarning = -1, receivedWarning = 48;
		public int sentPPS;
		public List<int> receivedPPS;
		public List<string> threadData;

		[System.NonSerialized]
		public int sentPacketsPerSecond;
		[System.NonSerialized]
		public List<int> receivedPacketsPerSecond;

		//Constant and Read-only
		//public const int PORT = 1611;

		private HolojamSendThread sendThread;
		private List<HolojamReceiveThread> receiveThreads;

		void Start() {
			receivedPacketsPerSecond = new List<int> ();
			
			sendThread = new HolojamSendThread(serverPort,serverAddress);
			receiveThreads = new List<HolojamReceiveThread> ();
			receivedPPS = new List<int> ();
			AddReceiveThread(multicastPort,multicastAddress);
			//AddReceiveThread(HOLOJAM_NONMOTIVE_PORT);

			sendThread.Start();
			foreach (HolojamThread thread in receiveThreads) {
				thread.Start();
			}

			StartCoroutine(DisplayPacketsPerSecond());
		}

		void AddReceiveThread(int port,string address){
			receiveThreads.Add(new HolojamReceiveThread(port,address));
			receivedPacketsPerSecond.Add (0);
			receivedPPS.Add (0);
		}

		void FixedUpdate() {
			List<HolojamView> viewsToSend = new List<HolojamView>();
			
			//Update timers on the receive threads (regardless if there are views)
			foreach(HolojamReceiveThread thread in receiveThreads)thread.Update(Time.deltaTime);

			foreach (HolojamView view in HolojamView.instances) {
				if (view.IsMine) {
					viewsToSend.Add(view);
				} else {
					if (string.IsNullOrEmpty(view.Label)) {
						Debug.LogWarning("Warning: No HolojamView label on object: " + view.name);
						continue;
					}
					
					HolojamObject o;
					foreach (HolojamThread thread in receiveThreads) {
						if (thread.GetObject (view.Label, out o)) {
							view.RawPosition = o.position;
							view.RawRotation = o.rotation;
							view.Bits = o.bits;
							view.Blob = o.blob;
							view.IsTracked = o.isTracked;
							break;
						}
						else view.IsTracked = false;
					}
				}
			}

			sendThread.UpdateManagedObjects(viewsToSend.ToArray());
		}

		private IEnumerator DisplayPacketsPerSecond() {
			bool running = true;
			foreach (HolojamThread thread in receiveThreads) {
				running = running && thread.IsRunning;
			}
			while (running) {
				yield return new WaitForSeconds(1f);
				
				threadData.Clear();
				threadData.Add(sendThread.ToString());

				sentPacketsPerSecond = sendThread.PacketCount;
				sendThread.PacketCount = 0;
				sentPPS = sentPacketsPerSecond;

				if (Time.frameCount > 0 && sentPPS <= sentWarning) {
					Debug.LogWarning (
						"HolojamNetwork: Sent Packets - " + sentPPS
					);
				}
				int threadIndex = 0;
				foreach (HolojamThread receiveThread in receiveThreads) {
					threadData.Add(receiveThread.ToString());
					
					receivedPacketsPerSecond[threadIndex] = receiveThread.PacketCount;
					receiveThread.PacketCount = 0;
					receivedPPS[threadIndex] = receivedPacketsPerSecond[threadIndex];
					
					if (Time.frameCount > 0 && receivedPPS[threadIndex] <= receivedWarning) {
						Debug.LogWarning (
							"HolojamNetwork: Received Packets (Thread " +
							(threadIndex+1) + ") - " + receivedPPS[threadIndex]
						);
					}
					
					threadIndex++;
				}
			}
		}

		protected override void OnDestroy () {
			base.OnDestroy ();
			sendThread.Stop ();
			foreach (HolojamThread thread in receiveThreads) {
				thread.Stop ();
			}
		}
	}


	internal abstract class HolojamThread {

		protected Thread thread;

		protected int port;
		protected string address;
		protected Dictionary<string, HolojamObject> managedObjects = new Dictionary<string, HolojamObject>();
		protected UnityEngine.Object lockObject = new UnityEngine.Object();
		protected int packetCount = 0;
		protected bool isRunning = false;
		
		protected abstract ThreadStart ThreadStart {
			get;
		}

		public int PacketCount {
			get { return packetCount; }
			set { packetCount = value; }
		}

		public bool IsRunning {
			get { return isRunning; }
		}

		protected HolojamThread(int port, string address) {
			this.port = port;
			this.address = address;
			thread = new Thread(ThreadStart);
		}

		public void Start() {
			if (this.isRunning) {
				Debug.LogWarning("HolojamNetwork: Thread already started!");
				return;
			}

			isRunning = true;
			thread.Start();
		}

		public void Stop() {
			if (!this.isRunning) {
				Debug.LogWarning("Thread already stopped!");
				return;
			}
			isRunning = false;
		}

		public bool GetObject(string key, out HolojamObject holoObject) {
			holoObject = null;
			lock (lockObject) {
				if (managedObjects.ContainsKey(key)) {
					holoObject = managedObjects[key];
					return true;
				} else {
					return false;
				}
			}
		}
		
		public override string ToString() {
			lock(lockObject){
				string s = "Port "+port+":";
				
				if(managedObjects.Count==0)s+="\n  (Empty)";
				else foreach(string k in managedObjects.Keys)
					s+="\n  "+k;
				
				return s;
			}
		}
	}

	internal class HolojamReceiveThread : HolojamThread {
		//Persistent dictionary for testing object timeout
		protected Dictionary<string, float> objectTimers = new Dictionary<string, float>();
		protected const float objectTimeout = 0.4f; //Seconds until removal

		private PacketBuffer previousPacket = new PacketBuffer(PacketBuffer.PACKET_SIZE);
		private PacketBuffer currentPacket = new PacketBuffer(PacketBuffer.PACKET_SIZE);
		private PacketBuffer tempPacket = new PacketBuffer(PacketBuffer.PACKET_SIZE);
		private update_protocol_v3.Update update;
		
		//Update the timers
		public void Update(float delta){
			lock(lockObject){
				//Safe list for iteration
				List<string> keys = new List<string>(objectTimers.Keys);
				foreach(string key in keys){
					objectTimers[key]+=delta; //Increment timer
					//Remove object from both dictionaries on timeout
					if(objectTimers[key]>objectTimeout){
						objectTimers.Remove(key);
						managedObjects.Remove(key);
					}
				}
			}
		}

		protected override ThreadStart ThreadStart {
			get {
				return Receive;
			}
		}

		public HolojamReceiveThread(int port, string address) : base(port,address) { }

		public void Receive() {
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
			socket.Bind(new IPEndPoint(IPAddress.Any, port));
			socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, 
				new MulticastOption(IPAddress.Parse(address)));
			socket.ReceiveTimeout = 1000;

			int nBytesReceived = 0;
			while (isRunning) {
				nBytesReceived = socket.Receive(currentPacket.bytes);
				currentPacket.stream.Position = 0;

				update = Serializer.Deserialize<update_protocol_v3.Update>(
							new MemoryStream(currentPacket.bytes, 0, nBytesReceived)
							);

				//currentPacket.frame = update.mod_version;
				//if(currentPacket.frame>previousPacket.frame){
				
				packetCount++;
				
				previousPacket.stream.Position = 0;
				currentPacket.stream.Position = 0;
				tempPacket.copyFrom(previousPacket);
				previousPacket.copyFrom(currentPacket);
				currentPacket.copyFrom(tempPacket);
				lock (lockObject) {
					//managedObjects.Clear();
					
					for (int j = 0; j < update.live_objects.Count; j++) {
						LiveObject or = update.live_objects[j];
						string label = or.label;
						
						objectTimers[label]=0; //Reset timer--we received data for this object

						HolojamObject ho;

						//Reform managedObjects every frame.
						//Inefficient for now, but will allow us to determine
						//if an object is registered.

						ho = new HolojamObject(label);
						managedObjects[label] = ho;

						if (update.lhs_frame) {
							ho.position = new Vector3(-(float)or.x, (float)or.y, (float)or.z);
							ho.rotation = new Quaternion(-(float)or.qx,
														  (float)or.qy, 
														  (float)or.qz, 
														 -(float)or.qw);
						} else {
							ho.position = new Vector3((float)or.x, (float)or.y, (float)or.z);
							ho.rotation = new Quaternion((float)or.qx, 
														 (float)or.qy, 
								       					 (float)or.qz, 
														 (float)or.qw);
						}
						ho.bits = or.button_bits;

						//Get blob if it's there. Inefficient
						ho.blob = or.extra_data;
						
						ho.isTracked = or.is_tracked;
					}
				}
				
				if (!isRunning) {
					socket.Close();
					break;
				}
			}
		}
	}

	internal class HolojamSendThread : HolojamThread {

		private int lastLoadedFrame;
		private byte[] packetBytes;
		private IPAddress ip = IPAddress.Any;
		private update_protocol_v3.Update update;

		protected override ThreadStart ThreadStart {
			get {
				return Send;
			}
		}

		public HolojamSendThread(int port, string address) : base(port,address) { }

		public void Send() {
			//Debug.Log("Attempting to open send thread with ip/port: " + ip.ToString() + " " + port);
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
			socket.SendTimeout = 1000;

			IPEndPoint ipEndPoint = new IPEndPoint(ip, 0);
			//string sendingIP = "192.168.1.44";
			string sendingIP = address;
			IPEndPoint send_ipEndPoint = new IPEndPoint(IPAddress.Parse(sendingIP), port);

			try {
				socket.Bind(ipEndPoint);
			} catch (SocketException e) {
				Debug.LogWarning("Error binding socket: " + ip.ToString() + " " + port + " " + e.ToString());
				isRunning = false;
			}

			//Checks if sending ip is local. If not, then we'll ping
			//the server for a unicast stream
			if (!sendingIP.StartsWith("192.")) {
				update = new update_protocol_v3.Update ();
				update.label = "ping";
				update.mod_version = 0;
				update.lhs_frame = false;

				using (MemoryStream stream = new MemoryStream ()) {
					Serializer.Serialize<Update> (stream, update);
					packetBytes = stream.ToArray();
					socket.SendTo (packetBytes, send_ipEndPoint);
				}
			}

			while (isRunning) {
				System.Threading.Thread.Sleep(10);
				if (managedObjects.Values.Count == 0)
					continue;
				lock (lockObject) {

					update = new update_protocol_v3.Update();
					update.label = "SendData";
					update.mod_version = lastLoadedFrame;
					update.lhs_frame = false;
					lastLoadedFrame++;
					
					foreach (KeyValuePair<string, HolojamObject> entry in managedObjects) {
						LiveObject o = entry.Value.ToLiveObject();
						update.live_objects.Add(o);
					}
					using (MemoryStream stream = new MemoryStream()) {
						packetCount++;
						Serializer.Serialize<Update>(stream, update);
						packetBytes = stream.ToArray();
						socket.SendTo(packetBytes, send_ipEndPoint);
					}
				}

				if (!isRunning) {
					socket.Close();
					break;
				}
			}
		}

		public void UpdateManagedObjects(HolojamView[] views) {
			lock (lockObject) {
				managedObjects.Clear();

				foreach (HolojamView view in views) {
					HolojamObject o = HolojamObject.FromView(view);
					managedObjects[o.label] = o;
				}
			}
		}
	}

	internal class HolojamObject {
		public static readonly Vector3 DEFAULT_POSITION = Vector3.zero;
		public static readonly Quaternion DEFAULT_ROTATION = Quaternion.identity;

		public string label;
		public Vector3 position = DEFAULT_POSITION;
		public Quaternion rotation = DEFAULT_ROTATION;
		public int bits = 0;
		public string blob = "";
		public bool isTracked = false;

		public HolojamObject(string label) {
			this.label = label;
		}

		public update_protocol_v3.LiveObject ToLiveObject() {
			update_protocol_v3.LiveObject o = new update_protocol_v3.LiveObject();
			o.label = this.label;

			o.x = position.x;
			o.y = position.y;
			o.z = position.z;

			o.qx = rotation.x;
			o.qy = rotation.y;
			o.qz = rotation.z;
			o.qw = rotation.w;

			o.button_bits = bits;

			if (!string.IsNullOrEmpty(blob)) {
				o.extra_data = blob;
			}
			
			o.is_tracked = isTracked;

			return o;
		}

		public static HolojamObject FromView(HolojamView view) {
			HolojamObject o = new HolojamObject(view.Label);

			o.position = view.RawPosition;
			o.rotation = view.RawRotation;
			o.bits = view.Bits;
			o.blob = view.Blob;
			o.isTracked = view.IsTracked;

			return o;
		}
	}
}