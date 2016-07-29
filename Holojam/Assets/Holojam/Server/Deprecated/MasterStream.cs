#pragma warning disable 0618 //Deprecated

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;
using ProtoBuf;
using update_protocol_v3;
using System.Threading;

namespace Holojam.Network {

     public class MasterStream : Singleton<MasterStream> {

          public int warningThreshold = 48;
          public int packetsPerSecond;

          /////Read-only/////
          private const int BLACK_BOX_CLIENT_PORT = 1611;


          /////Private/////
          //References
          private Dictionary<string, LiveObjectStorage> liveObjects = new Dictionary<string, LiveObjectStorage>();
          private UnityEngine.Object lockObject = new UnityEngine.Object();
          private PacketBuffer previousPacket = new PacketBuffer(PacketBuffer.PACKET_SIZE);
          private PacketBuffer currentPacket = new PacketBuffer(PacketBuffer.PACKET_SIZE);
          private PacketBuffer tempPacket = new PacketBuffer(PacketBuffer.PACKET_SIZE);
          private update_protocol_v3.Update update;
          //Primitives
          private int packetCount = 0;
          private bool receivingPackets = true;

          ///////////////////////////////////////////////////////////////////////////
          //
          // Inherited from MonoBehaviour
          //

          private void Start() {
               Application.runInBackground = true;
               Application.targetFrameRate = -1;

               System.Threading.Thread thread = new System.Threading.Thread(run);
               thread.Start();
               StartCoroutine(DisplayPacketsPerSecond());
          }

          ///////////////////////////////////////////////////////////////////////////
          //
          // Debug functions
          //

          private IEnumerator DisplayPacketsPerSecond() {
               while (receivingPackets) {
                    yield return new WaitForSeconds(1f);
                    //Debug.LogWarning(string.Format("Packets per second: {0} Most recent packet frame: {1}", packetCount, currentPacket.frame));
                    packetsPerSecond = packetCount;
                    if (packetCount <= warningThreshold && Time.frameCount > 0)
                         Debug.LogWarning("MasterStream: Received " + packetCount + " packets. Most recent packet received at frame " + currentPacket.frame);
                    packetCount = 0;
               }
          }

          ///////////////////////////////////////////////////////////////////////////
          //
          // API functions
          //

          public bool IsLiveObject(Motive.Tag tag) {
               return liveObjects.ContainsKey(Motive.GetName(tag));
          }

          public bool IsLiveObject(string label) {
               return liveObjects.ContainsKey(label);
          }

          public bool GetPosition(Motive.Tag tag, out Vector3 position) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(Motive.GetName(tag), out storage)) {
                         position = LiveObjectStorage.DEFAULT_VECTOR_POSITION;
                         return false;
                    } else {
                         position = storage.position;
                         if (position.Equals(LiveObjectStorage.DEFAULT_VECTOR_POSITION)) {
                              return false;
                         } else {
                              return true;
                         }
                    }
               }
          }

          public bool GetPosition(string tag, out Vector3 position) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(tag, out storage)) {
                         position = LiveObjectStorage.DEFAULT_VECTOR_POSITION;
                         return false;
                    } else {
                         position = storage.position;
                         if (position.Equals(LiveObjectStorage.DEFAULT_VECTOR_POSITION)) {
                              return false;
                         } else {
                              return true;
                         }
                    }
               }
          }

          public bool GetRotation(Motive.Tag tag, out Quaternion rotation) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(Motive.GetName(tag), out storage)) {
                         rotation = LiveObjectStorage.DEFAULT_QUATERNION_ROTATION;
                         return false;
                    } else {
                         rotation = storage.rotation;
                         if (rotation.Equals(LiveObjectStorage.DEFAULT_QUATERNION_ROTATION)) {
                              return false;
                         } else {
                              return true;
                         }
                    }
               }
          }


          public bool GetRotation(string tag, out Quaternion rotation) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(tag, out storage)) {
                         rotation = LiveObjectStorage.DEFAULT_QUATERNION_ROTATION;
                         return false;
                    } else {
                         rotation = storage.rotation;
                         if (rotation.Equals(LiveObjectStorage.DEFAULT_QUATERNION_ROTATION)) {
                              return false;
                         } else {
                              return true;
                         }
                    }
               }
          }

          public bool GetButtonBits(Motive.Tag tag, out int bits) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(Motive.GetName(tag), out storage)) {
                         bits = 0;
                         return false;
                    } else {
                         bits = storage.bits;
                         return true;
                    }
               }
          }

          public bool GetButtonBits(string label, out int bits) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(label, out storage)) {
                         bits = 0;
                         return false;
                    } else {
                         bits = storage.bits;
                         return true;
                    }
               }
          }

          public Vector3 getLiveObjectPosition(string name) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(name, out storage)) {
                         return LiveObjectStorage.DEFAULT_VECTOR_POSITION;
                    }
               }
               return storage.position;
          }

          public Quaternion getLiveObjectRotation(string name) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(name, out storage)) {
                         return LiveObjectStorage.DEFAULT_QUATERNION_ROTATION;
                    }
               }
               return storage.rotation;
          }

          public int getLiveObjectButtonBits(string name) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(name, out storage)) {
                         return 0;
                    }
               }
               return storage.bits;
          }

          //public Vector2 getLiveObjectAxisButton(string name, int index) {
          //     LiveObjectStorage storage;
          //     lock (lockObject) {
          //          if (!liveObjects.TryGetValue(name, out storage)) {
          //               //print ("Body not found: " + name);
          //               return Vector2.zero;
          //          }
          //     }
          //     return storage.axisButtons[index];
          //}

          ///////////////////////////////////////////////////////////////////////////
          //
          // Thread methods
          //

          // This thread handles incoming NatNet packets.
          private void run() {
               Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
               socket.Bind(new IPEndPoint(IPAddress.Any, BLACK_BOX_CLIENT_PORT));
               socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse("224.1.1.1")));

               int nBytesReceived = 0;
               while (receivingPackets) {
                    nBytesReceived = socket.Receive(currentPacket.bytes);
                    currentPacket.stream.Position = 0;

                    update = Serializer.Deserialize<update_protocol_v3.Update>(new MemoryStream(currentPacket.bytes, 0, nBytesReceived));
                    
                    currentPacket.frame = update.mod_version;
                    if (currentPacket.frame > previousPacket.frame) {
                         packetCount++;

                         previousPacket.stream.Position = 0;
                         currentPacket.stream.Position = 0;
                         tempPacket.copyFrom(previousPacket);
                         previousPacket.copyFrom(currentPacket);
                         currentPacket.copyFrom(tempPacket);
                         for (int j = 0; j < update.live_objects.Count; j++) {
                              LiveObject or = update.live_objects[j];
                              string label = or.label;

                              LiveObjectStorage ow;
                              lock (lockObject) {
                                   //List<LiveObjectStorage> objectsToRemove = new List<LiveObjectStorage>();
                                   //objectsToRemove.AddRange(liveObjects.Values);

                                   //Add objects that exist
                                   if (!liveObjects.TryGetValue(label, out ow)) {
                                        ow = new LiveObjectStorage(label);
                                        liveObjects[label] = ow;
                                   } else {
                                        ow = liveObjects[label];
                                        //objectsToRemove.Remove(ow);
                                   }
                                   if (update.lhs_frame) {
                                        ow.position = new Vector3(-(float)or.x, (float)or.y, (float)or.z);
                                        ow.rotation = new Quaternion(-(float)or.qx, (float)or.qy, (float)or.qz, -(float)or.qw);
                                   } else {
                                        ow.position = new Vector3((float)or.x, (float)or.y, (float)or.z);
                                        ow.rotation = new Quaternion((float)or.qx, (float)or.qy, (float)or.qz, (float)or.qw);
                                   }
                                   ow.bits = or.button_bits;

                                   //Remove objects from pool that aren't there.
                                   //foreach (LiveObjectStorage missingObject in objectsToRemove) {
                                   //     liveObjects.Remove(missingObject.key);
                                   //}
                              }
                         }
                    }

                    if (!receivingPackets) {
                         socket.Close();
                         break;
                    }
               }
          }

          void OnApplicationQuit() {
               this.receivingPackets = false;
          }
     }
}