using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using ProtoBuf;
using update_protocol_v3;

namespace Holojam {
     public class MasterStream : Singleton<MasterStream> {

          private readonly Vector3 DEFAULT_VECTOR_POSITION = new Vector3(0, 0, 0);
          private readonly Quaternion DEFAULT_QUATERNION_ROTATION = new Quaternion(0, 0, 0, 0);
          private readonly int PACKET_SIZE = 65507;

          private class OtherMarker {
               public Vector3 pos;
          }
          private class LiveObjectStorage {
               public Vector3 pos;
               public Quaternion rot;
               public int buttonBits;
               public List<Vector2> axisButtons;

               // TODO: Handle extra data
          }


          private const int BLACK_BOX_CLIENT_PORT = 1611;

          private int nBytesReceived;
          private bool stopReceive;
          private IPEndPoint ipEndPoint;
          private System.Threading.Thread thread = null;
          private Socket socket;
          private byte[] b1;
          private byte[] b2;
          private byte[] b3;
          private MemoryStream b1ms;
          private MemoryStream b2ms;
          private float accum;
          private int nPackets;
          private int nFrames;

          private UnityEngine.Object lock_object;

          private long lastLoadedFrame;
          private byte[] lastLoadedBuffer;
          private MemoryStream lastLoadedBufferMS;

          private Dictionary<string, LiveObjectStorage> liveObjects = new Dictionary<string, LiveObjectStorage>();

          public void Start() {
               lock_object = new UnityEngine.Object();
               Application.runInBackground = true;
               Application.targetFrameRate = -1;
               accum = 0;
               nPackets = 0;
               nFrames = 0;
               // ~65KB buffer sizes
               b1 = new byte[65507];
               b2 = new byte[65507];
               b1ms = new MemoryStream(b1);
               b2ms = new MemoryStream(b2);
               thread = new System.Threading.Thread(ThreadRun);
               thread.Start();
          }
          // Handle new thread data / invoke Unity routines outside of the socket thread.
          public void Update() {
               accum += Time.deltaTime;
               float round_accum = (float)Math.Floor(accum);
               if (round_accum > 0) {
                    accum -= round_accum;
                    // print ("FPS: " + ((float)nFrames / round_accum).ToString());
                    print("packets per second: " + ((float)nPackets / round_accum).ToString());
                    nPackets = 0;
                    nFrames = 0;
               }
               nFrames++;
          }

          public Vector3 getLiveObjectPosition(string name) {
               LiveObjectStorage o;
               lock (lock_object) {
                    if (!liveObjects.TryGetValue(name, out o)) {
                         //print ("Body not found: " + name);
                         return DEFAULT_VECTOR_POSITION;
                    }
               }
               return o.pos;
          }
          public Quaternion getLiveObjectRotation(string name) {
               LiveObjectStorage o;
               lock (lock_object) {
                    if (!liveObjects.TryGetValue(name, out o)) {
                         //print ("Body not found: " + name);
                         return DEFAULT_QUATERNION_ROTATION;
                    }
               }
               return o.rot;
          }
          public int getLiveObjectButtonBits(string name) {
               LiveObjectStorage o;
               lock (lock_object) {
                    if (!liveObjects.TryGetValue(name, out o)) {
                         //print ("Body not found: " + name);
                         return 0;
                    }
               }
               return o.buttonBits;
          }
          public Vector2 getLiveObjectAxisButton(string name, int index) {
               LiveObjectStorage o;
               lock (lock_object) {
                    if (!liveObjects.TryGetValue(name, out o)) {
                         //print ("Body not found: " + name);
                         return Vector2.zero;
                    }
               }
               return o.axisButtons[index];
          }

          // This thread handles incoming NatNet packets.
          private void ThreadRun() {
               stopReceive = false;
               Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
               ipEndPoint = new IPEndPoint(IPAddress.Any, BLACK_BOX_CLIENT_PORT);
               //Debug.Log("prebind");
               socket.Bind(ipEndPoint);
               //Debug.Log("bind");
               MulticastOption mo = new MulticastOption(IPAddress.Parse("224.1.1.1"));
               socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mo);

               nBytesReceived = 0;
               lastLoadedBuffer = b1;
               lastLoadedBufferMS = b1ms;
               lastLoadedFrame = 0;

               byte[] newPacketBuffer = b2;
               MemoryStream newPacketBufferMS = b2ms;
               long newPacketFrame = 0;

               byte[] tempBuffer;
               MemoryStream tempBufferMS;

               while (true) {
                    //Debug.Log("preRECV");
                    nBytesReceived = socket.Receive(newPacketBuffer);
                    //Debug.Log("RECV");
                    nPackets++;
                    newPacketBufferMS.Position = 0;
                    //Debug.Log ("Deserializing data...");
                    update_protocol_v3.Update update = Serializer.Deserialize<update_protocol_v3.Update>(new MemoryStream(newPacketBuffer, 0, nBytesReceived));
                    //Debug.Log ("Data deserialized. Received update of type " + update.label);

                    newPacketFrame = update.mod_version;
                    if (newPacketFrame > lastLoadedFrame) {
                         // Swap the buffers and reset the positions.
                         lastLoadedBufferMS.Position = 0;
                         newPacketBufferMS.Position = 0;
                         tempBuffer = lastLoadedBuffer;
                         tempBufferMS = lastLoadedBufferMS;
                         lastLoadedBuffer = newPacketBuffer;
                         lastLoadedBufferMS = newPacketBufferMS;
                         newPacketBuffer = tempBuffer;
                         newPacketBufferMS = tempBufferMS;
                         lastLoadedFrame = newPacketFrame;



                         for (int j = 0; j < update.live_objects.Count; j++) {
                              LiveObject or = update.live_objects[j];
                              string label = or.label;
                              if (label == "marker") {
                                   Debug.Log("marker at " + or.x + ", " + or.y + ", " + or.z);
                              }
                              LiveObjectStorage ow;
                              lock (lock_object) {
                                   if (!liveObjects.TryGetValue(label, out ow)) {
                                        ow = new LiveObjectStorage();
                                        liveObjects[label] = ow;
                                   } else {
                                        ow = liveObjects[label];
                                   }
                                   if (update.lhs_frame) {
                                        ow.pos = new Vector3(-(float)or.x, (float)or.y, (float)or.z);
                                        ow.rot = new Quaternion(-(float)or.qx, (float)or.qy, (float)or.qz, -(float)or.qw);
                                   } else {
                                        ow.pos = new Vector3((float)or.x, (float)or.y, (float)or.z);
                                        ow.rot = new Quaternion((float)or.qx, (float)or.qy, (float)or.qz, (float)or.qw);
                                   }
                                   ow.buttonBits = or.button_bits;
                              }
                         }
                    }

                    if (stopReceive) {
                         break;
                    }
               }
          }
          private void OnDestroy() {
               stopReceive = true;
          }
     }
}

//namespace Holojam {
//     public enum LiveObjectTag {
//          HEADSET1, HEADSET2, HEADSET3, HEADSET4, WAND1, WAND2, WAND3, WAND4,BOX1,BOX2,SPHERE1
//     }

//     public class MasterStream : Singleton<MasterStream> {
//          /////Readonly/////
//          public static readonly Vector3 DEFAULT_VECTOR_POSITION = new Vector3(0, 0, 0);
//          public static readonly Quaternion DEFAULT_QUATERNION_ROTATION = new Quaternion(0, 0, 0, 0);
//          private const int PACKET_SIZE = 65507; // ~65KB buffer sizes
//          private const int BLACK_BOX_CLIENT_PORT = 1611;

//          private static readonly Dictionary<LiveObjectTag, string> tagToMotiveName = new Dictionary<LiveObjectTag, string>() {
//               { LiveObjectTag.HEADSET1, "VR1" },
//               { LiveObjectTag.HEADSET2, "VR2" },
//               { LiveObjectTag.HEADSET3, "VR3" },
//               { LiveObjectTag.HEADSET4, "VR4" },
//               { LiveObjectTag.WAND1, "VR1_wand" },
//               { LiveObjectTag.WAND2, "VR2_wand" },
//               { LiveObjectTag.WAND3, "VR3_wand" },
//               { LiveObjectTag.WAND4, "VR4_wand" },
//               { LiveObjectTag.BOX1, "VR1_box" }
//          };

//          /////Private/////
//          //References
//          private Dictionary<string, LiveObjectStorage> liveObjects = new Dictionary<string, LiveObjectStorage>();
//          private UnityEngine.Object lockObject = new UnityEngine.Object();
//          private PacketBuffer previousPacket = new PacketBuffer(PACKET_SIZE);
//          private PacketBuffer currentPacket = new PacketBuffer(PACKET_SIZE);
//          private PacketBuffer tempPacket = new PacketBuffer(PACKET_SIZE);
//          private update_protocol_v3.Update update;
//          //Primitives
//          private int packetCount = 0;
//          private bool receivingPackets = true;

//          //private class OtherMarker
//          //{
//          //	public Vector3 position;
//          //}

//          private class LiveObjectStorage {
//               public Vector3 position = Vector3.zero;
//               public Quaternion rotation = Quaternion.identity;
//               public int buttonBits = 0;
//               public List<Vector2> axisButtons = new List<Vector2>();
//          }

//          private class PacketBuffer {

//               public byte[] bytes;
//               public MemoryStream stream;
//               public long frame;

//               public PacketBuffer(int packetSize) {
//                    bytes = new byte[packetSize];
//                    stream = new MemoryStream(bytes);
//                    frame = 0;
//               }

//               public void copyFrom(PacketBuffer other) {
//                    this.bytes = other.bytes;
//                    this.stream = other.stream;
//                    this.frame = other.frame;
//               }
//          }

//          ///////////////////////////////////////////////////////////////////////////
//          //
//          // Inherited from MonoBehaviour
//          //

//          private void Start() {
//               Application.runInBackground = true;
//               Application.targetFrameRate = -1;

//               System.Threading.Thread thread = new System.Threading.Thread(run);
//               thread.Start();
//               StartCoroutine(DisplayPacketsPerSecond());
//          }

//          ///////////////////////////////////////////////////////////////////////////
//          //
//          // Debug functions
//          //

//          private IEnumerator DisplayPacketsPerSecond() {
//               while (receivingPackets) {
//                    yield return new WaitForSeconds(1f);
//                    Debug.LogWarning(string.Format("Packets per second: {0} Most recent packet frame: {1}", packetCount,currentPacket.frame));
//                    packetCount = 0;
//               }
//          }

//          ///////////////////////////////////////////////////////////////////////////
//          //
//          // API functions
//          //

//          public bool GetPosition(LiveObjectTag tag, out Vector3 position) {
//               if (!tagToMotiveName.ContainsKey(tag)) {
//                    throw new System.ArgumentException("Illegal tag.");
//               }
//               LiveObjectStorage storage;
//               lock (lockObject) { 
//                    if (!liveObjects.TryGetValue(tagToMotiveName[tag], out storage)) {
//                         position = DEFAULT_VECTOR_POSITION;
//                         return false;
//                    } else {
//                         position = storage.position;
//                         return true;
//                    }
//               }
//          }

//          public bool GetRotation(LiveObjectTag tag, out Quaternion rotation) {
//               if (!tagToMotiveName.ContainsKey(tag)) {
//                    throw new System.ArgumentException("Illegal tag.");
//               }
//               LiveObjectStorage storage;
//               lock (lockObject) {
//                    if(!liveObjects.TryGetValue(tagToMotiveName[tag], out storage)) {
//                         rotation = DEFAULT_QUATERNION_ROTATION;
//                         return false;
//                    } else {
//                         rotation = storage.rotation;
//                         return true;
//                    }
//               }
//          }

//          public Vector3 getLiveObjectPosition(string name) {
//               LiveObjectStorage storage;
//               lock (lockObject) {
//                    if (!liveObjects.TryGetValue(name, out storage)) {
//                         return DEFAULT_VECTOR_POSITION;
//                    }
//               }
//               return storage.position;
//          }

//          public Quaternion getLiveObjectRotation(string name) {
//               LiveObjectStorage storage;
//               lock (lockObject) {
//                    if (!liveObjects.TryGetValue(name, out storage)) {
//                         return DEFAULT_QUATERNION_ROTATION;
//                    }
//               }
//               return storage.rotation;
//          }

//          public int getLiveObjectButtonBits(string name) {
//               LiveObjectStorage storage;
//               lock (lockObject) {
//                    if (!liveObjects.TryGetValue(name, out storage)) {
//                         return 0;
//                    }
//               }
//               return storage.buttonBits;
//          }

//          public Vector2 getLiveObjectAxisButton(string name, int index) {
//               LiveObjectStorage storage;
//               lock (lockObject) {
//                    if (!liveObjects.TryGetValue(name, out storage)) {
//                         //print ("Body not found: " + name);
//                         return Vector2.zero;
//                    }
//               }
//               return storage.axisButtons[index];
//          }

//          ///////////////////////////////////////////////////////////////////////////
//          //
//          // Thread methods
//          //

//          // This thread handles incoming NatNet packets.
//          private void run() {
//               Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//               socket.Bind(new IPEndPoint(IPAddress.Any, BLACK_BOX_CLIENT_PORT));
//               socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse("224.1.1.1")));

//               int nBytesReceived = 0;
//               while (receivingPackets) {
//                    nBytesReceived = socket.Receive(currentPacket.bytes);
//                    currentPacket.stream.Position = 0;

//                    update = Serializer.Deserialize<update_protocol_v3.Update>(new MemoryStream(currentPacket.bytes, 0, nBytesReceived));

//                    currentPacket.frame = update.mod_version;
//                    if (currentPacket.frame > previousPacket.frame) {
//                         packetCount++;

//                         previousPacket.stream.Position = 0;
//                         currentPacket.stream.Position = 0;
//                         tempPacket.copyFrom(previousPacket);
//                         previousPacket.copyFrom(currentPacket);
//                         currentPacket.copyFrom(tempPacket);

//                         for (int j = 0; j < update.live_objects.Count; j++) {
//                              LiveObject or = update.live_objects[j];
//                              string label = or.label;

//                              LiveObjectStorage ow;
//                              lock (lockObject) {
//                                   if (!liveObjects.TryGetValue(label, out ow)) {
//                                        ow = new LiveObjectStorage();
//                                        liveObjects[label] = ow;
//                                   } else {
//                                        ow = liveObjects[label];
//                                   }
//                                   if (update.lhs_frame) {
//                                        ow.position = new Vector3(-(float)or.x, (float)or.y, (float)or.z);
//                                        ow.rotation = new Quaternion(-(float)or.qx, (float)or.qy, (float)or.qz, -(float)or.qw);
//                                   } else {
//                                        ow.position = new Vector3((float)or.x, (float)or.y, (float)or.z);
//                                        ow.rotation = new Quaternion((float)or.qx, (float)or.qy, (float)or.qz, (float)or.qw);
//                                   }
//                                   ow.buttonBits = or.button_bits;
//                              }
//                         }
//                    }

//                    if (!receivingPackets) {
//                         socket.Close();
//                    }
//               }
//          }

//          void OnApplicationQuit() {
//               this.receivingPackets = false;
//          }
//     }
//}