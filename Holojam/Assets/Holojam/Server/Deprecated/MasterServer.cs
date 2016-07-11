#pragma warning disable 0618 //Deprecated

using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using ProtoBuf;
using update_protocol_v3;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Holojam.Network {
     public class MasterServer : Singleton<MasterServer> {

          public bool isMaster;
          public List<SynchronizedObject> synchronizedObjects = new List<SynchronizedObject>();

          /////Read-only/////
          private const int BLACK_BOX_SERVER_PORT = 1615;

          /////Private/////
          //References
          private Dictionary<string, LiveObjectStorage> liveObjects = new Dictionary<string, LiveObjectStorage>();
          private update_protocol_v3.Update update;
          //Primitives
          //private int packetCount = 0;
          private bool sendingPackets = true;

          private int lastLoadedFrame;
          private byte[] packetBytes;

          // Use this for initialization
          void Start() {
               this.CreateServerThread();
          }

          public void CreateServerThread() {
               System.Threading.Thread thread = new System.Threading.Thread(ServerThread);
               thread.Start();
          }


          void ServerThread() {
               Debug.Log("Starting server thread");
               Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
               IPAddress ip = IPAddress.Parse("192.168.1.44");
               IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.44"), 0);
               IPEndPoint send_ipEndPoint = new IPEndPoint(ip, BLACK_BOX_SERVER_PORT);

               try {
                    socket.Bind(ipEndPoint);
               } catch (SocketException e) {
                    Debug.Log("Error binding socket: " + e.ToString());
                    sendingPackets = false;
               }

               while (sendingPackets) {

                    update = new update_protocol_v3.Update();
                    update.label = "neuron";
                    update.mod_version = lastLoadedFrame;
                    update.lhs_frame = false;
                    lastLoadedFrame++;

                    foreach (KeyValuePair<string, LiveObjectStorage> entry in liveObjects) {
                         LiveObject o = entry.Value.ToLiveObject();
                         update.live_objects.Add(o);
                    }

                    using (MemoryStream stream = new MemoryStream()) {
                         Serializer.Serialize<Update>(stream, update);
                         packetBytes = stream.GetBuffer();
                         socket.SendTo(packetBytes, send_ipEndPoint);
                    }

                    if (!sendingPackets) {
                         socket.Close();
                         break;
                    }
               }
          }

          // Update is called once per frame
          void Update() {
               liveObjects.Clear();
               foreach(SynchronizedObject sync in synchronizedObjects) {
                    if (string.IsNullOrEmpty(sync.label)) {
                         Debug.LogWarning("Warning! There is an unlabeled object in the synchronized object pool.");
                         continue;
                    }


                    LiveObjectStorage storage = new LiveObjectStorage(sync.label);
                    storage.position = sync.position;
                    storage.rotation = sync.rotation;
                    storage.bits = sync.bits;
                    storage.blob = sync.blob;

                    liveObjects.Add(storage.label,storage);
               }
          }

          protected override void OnDestroy() {
               base.OnDestroy();
               sendingPackets = false;
          }
     }

}
