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

namespace Holojam {
     public enum LiveObjectTag {
          HEADSET1, HEADSET2, HEADSET3, HEADSET4, WAND1, WAND2, WAND3, WAND4, BOX1, BOX2, SPHERE1,
		LEFTHAND1,RIGHTHAND1,LEFTFOOT1,RIGHTFOOT1,LEFTHAND2,RIGHTHAND2,LEFTFOOT2,RIGHTFOOT2,LEFTHAND3,RIGHTHAND3,LEFTFOOT3,RIGHTFOOT3,
          LAPTOP,TABLE
     }

     public class MasterStream : Singleton<MasterStream> {
          
          public int warningThreshold = 48;
          public int packetsPerSecond;
          
          /////Readonly/////
          public static readonly Vector3 DEFAULT_VECTOR_POSITION = new Vector3(0, 0, 0);
          public static readonly Quaternion DEFAULT_QUATERNION_ROTATION = new Quaternion(0, 0, 0, 0);
          private const int PACKET_SIZE = 65507; // ~65KB buffer sizes
          private const int BLACK_BOX_CLIENT_PORT = 1611;

          private static readonly Dictionary<LiveObjectTag, string> tagToMotiveName = new Dictionary<LiveObjectTag, string>() {
               { LiveObjectTag.HEADSET1, "VR1" },
               { LiveObjectTag.HEADSET2, "VR2" },
               { LiveObjectTag.HEADSET3, "VR3" },
               { LiveObjectTag.HEADSET4, "VR4" },
               { LiveObjectTag.WAND1, "VR1_wand" },
               { LiveObjectTag.WAND2, "VR2_wand" },
               { LiveObjectTag.WAND3, "VR3_wand" },
               { LiveObjectTag.WAND4, "VR4_wand" },
               { LiveObjectTag.BOX1, "VR1_box" },
               { LiveObjectTag.LEFTHAND1, "VR1_lefthand"},
               { LiveObjectTag.RIGHTHAND1, "VR1_righthand"},
               { LiveObjectTag.LEFTFOOT1, "VR1_leftankle"},
               { LiveObjectTag.RIGHTFOOT1, "VR1_rightankle"},
			{ LiveObjectTag.LEFTHAND2, "VR2_lefthand"},
			{ LiveObjectTag.RIGHTHAND2, "VR2_righthand"},
			{ LiveObjectTag.LEFTFOOT2, "VR2_leftankle"},
			{ LiveObjectTag.RIGHTFOOT2, "VR2_rightankle"},
			{ LiveObjectTag.LEFTHAND3, "VR3_lefthand"},
			{ LiveObjectTag.RIGHTHAND3, "VR3_righthand"},
			{ LiveObjectTag.LEFTFOOT3, "VR3_leftankle"},
			{ LiveObjectTag.RIGHTFOOT3, "VR3_rightankle"},
               { LiveObjectTag.LAPTOP, "VR1_laptop"},
               { LiveObjectTag.TABLE, "VR1_table"}
          };

          /////Private/////
          //References
          private Dictionary<string, LiveObjectStorage> liveObjects = new Dictionary<string, LiveObjectStorage>();
          private UnityEngine.Object lockObject = new UnityEngine.Object();
          private PacketBuffer previousPacket = new PacketBuffer(PACKET_SIZE);
          private PacketBuffer currentPacket = new PacketBuffer(PACKET_SIZE);
          private PacketBuffer tempPacket = new PacketBuffer(PACKET_SIZE);
          private update_protocol_v3.Update update;
          //Primitives
          private int packetCount = 0;
          private bool receivingPackets = true;


          private class LiveObjectStorage {
               public string key;

               public LiveObjectStorage(string key) {
                    this.key = key;
               }

               public Vector3 position = Vector3.zero;
               public Quaternion rotation = Quaternion.identity;
               public int buttonBits = 0;
               public List<Vector2> axisButtons = new List<Vector2>();
          }

          private class PacketBuffer {

               public byte[] bytes;
               public MemoryStream stream;
               public long frame;

               public PacketBuffer(int packetSize) {
                    bytes = new byte[packetSize];
                    stream = new MemoryStream(bytes);
                    frame = 0;
               }

               public void copyFrom(PacketBuffer other) {
                    this.bytes = other.bytes;
                    this.stream = other.stream;
                    this.frame = other.frame;
               }
          }

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
                    packetsPerSecond=packetCount;
                    if(packetCount<=warningThreshold && Time.frameCount>0)
                         Debug.LogWarning("MasterStream: Received "+packetCount+" packets. Most recent packet at frame "+currentPacket.frame);
                    packetCount = 0;
               }
          }

          ///////////////////////////////////////////////////////////////////////////
          //
          // API functions
          //

          public bool IsLiveObject(LiveObjectTag tag) {
               return liveObjects.ContainsKey(tagToMotiveName[tag]);
          }

          public bool IsLiveObject(string label) {
               return liveObjects.ContainsKey(label);
          }

          public bool GetPosition(LiveObjectTag tag, out Vector3 position) {
               if (!tagToMotiveName.ContainsKey(tag)) {
                    throw new System.ArgumentException("Illegal tag.");
               }
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(tagToMotiveName[tag], out storage)) {
                         position = DEFAULT_VECTOR_POSITION;
                         return false;
                    } else {
                         position = storage.position;
                         if (position.Equals(DEFAULT_VECTOR_POSITION)) {
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
                         position = DEFAULT_VECTOR_POSITION;
                         return false;
                    } else {
                         position = storage.position;
                         if (position.Equals(DEFAULT_VECTOR_POSITION)) {
                              return false;
                         } else {
                              return true;
                         }
                    }
               }
          }

          public bool GetRotation(LiveObjectTag tag, out Quaternion rotation) {
               if (!tagToMotiveName.ContainsKey(tag)) {
                    throw new System.ArgumentException("Illegal tag.");
               }
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(tagToMotiveName[tag], out storage)) {
                         rotation = DEFAULT_QUATERNION_ROTATION;
                         return false;
                    } else {
                         rotation = storage.rotation;
                         if (rotation.Equals(DEFAULT_QUATERNION_ROTATION)) {
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
                         rotation = DEFAULT_QUATERNION_ROTATION;
                         return false;
                    } else {
                         rotation = storage.rotation;
                         if (rotation.Equals(DEFAULT_QUATERNION_ROTATION)) {
                              return false;
                         } else {
                              return true;
                         }
                    }
               }
          }

          public bool GetButtonBits(LiveObjectTag tag, out int bits) {
               if (!tagToMotiveName.ContainsKey(tag)) {
                    throw new System.ArgumentException("Illegal tag.");
               }
               LiveObjectStorage storage;
               lock(lockObject) {
                    if (!liveObjects.TryGetValue(tagToMotiveName[tag], out storage)) {
                         bits = 0;
                         return false;
                    } else {
                         bits = storage.buttonBits;
                         return true;
                    }
               }
          }

          public Vector3 getLiveObjectPosition(string name) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(name, out storage)) {
                         return DEFAULT_VECTOR_POSITION;
                    }
               }
               return storage.position;
          }

          public Quaternion getLiveObjectRotation(string name) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(name, out storage)) {
                         return DEFAULT_QUATERNION_ROTATION;
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
               return storage.buttonBits;
          }

          public Vector2 getLiveObjectAxisButton(string name, int index) {
               LiveObjectStorage storage;
               lock (lockObject) {
                    if (!liveObjects.TryGetValue(name, out storage)) {
                         //print ("Body not found: " + name);
                         return Vector2.zero;
                    }
               }
               return storage.axisButtons[index];
          }

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
                                   ow.buttonBits = or.button_bits;

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