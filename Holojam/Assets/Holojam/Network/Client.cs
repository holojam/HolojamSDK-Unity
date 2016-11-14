//Client.cs
//Created by Aaron C Gaudette on 11.11.16
//Send and receive Holojam UDP data using protobuf (deprecated)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using ProtoBuf;
using update_protocol_v3;
using System.Threading;

namespace Holojam.Network{
   public class Client : Utility.Global<Client>{
      //Connection options
      public string serverAddress = "192.168.1.97";
      public int serverPort = 1612;
      public string multicastAddress = "224.1.1.3";
      public int multicastPort = 1611;

      public string sendScope = "Unity";
      //Global
      public static string SEND_SCOPE{
         get{return global.sendScope;}
      }

      //Debug
      #if UNITY_EDITOR
      public int sentPPS, receivedPPS;
      public List<string> threadData = new List<string>();
      #endif

      List<View> viewsToSend;
      SendThread sendThread;
      ReceiveThread receiveThread;

      void Awake(){
         viewsToSend = new List<View>();
         //Start send and receive threads
         sendThread = new SendThread(serverAddress,serverPort,sendScope);
         sendThread.Start();
         receiveThread = new ReceiveThread(multicastAddress,multicastPort);
         receiveThread.Start();
         //Debug
         #if UNITY_EDITOR
         StartCoroutine(Display());
         #endif
      }

      void FixedUpdate(){
         viewsToSend.Clear();
         //Update timers on the receive threads (regardless if there are views)
         receiveThread.UpdateTimers(Time.deltaTime);

         foreach(View view in View.instances){
            if(string.IsNullOrEmpty(view.label)){
               Debug.LogWarning("Holojam.Network.Client: Invalid view label",view);
               continue;
            //Update send thread with sending views
            }else if(view.sending)
               viewsToSend.Add(view);
            //Update active views with the receive thread
            else{
               SwapFlake f;
               if(receiveThread.GetObject(view.scope+"."+view.label,out f)){
                  view.rawPosition = f.position;
                  view.rawRotation = f.rotation;
                  view.bits = f.bits;
                  view.blob = f.blob;
                  view.tracked = f.isTracked;
               } else view.tracked = false; //Set untracked if not found
            }
         }
         sendThread.UpdateFlakes(viewsToSend.ToArray());
      }

      //Debug
      #if UNITY_EDITOR
      IEnumerator Display(){
         while(sendThread.running || receiveThread.running){
            yield return new WaitForSeconds(1);

            threadData.Clear();
            threadData.Add(sendThread.ToString());
            threadData.Add(receiveThread.ToString());

            sentPPS = sendThread.GetPacketCount();
            receivedPPS = receiveThread.GetPacketCount();
         }
      }
      #endif

      void OnDestroy(){
         sendThread.Stop();
         receiveThread.Stop();
      }
   }

   //Base thread class
   internal abstract class ClientThread{
      protected const int TIMEOUT = 1000;
      protected const int PACKET_SIZE = 65507; //Around 65kb

      protected Thread thread;
      public bool running;
      protected update_protocol_v3.Update update;
      protected byte[] packet;

      protected string address;
      protected int port;
      protected int packetCount;

      protected Dictionary<string,SwapFlake> flakes; //Data ready to be distributed
      protected UnityEngine.Object lockObject;

      protected ClientThread(string address, int port){
         this.address = address;
         this.port = port;

         running = false;
         packet = new byte[PACKET_SIZE];
         packetCount = 0;

         flakes = new Dictionary<string,SwapFlake>();
         lockObject = new UnityEngine.Object();
         thread = new Thread(ThreadStart);
      }

      protected abstract ThreadStart ThreadStart{get;}

      public void Start(){
         running = true;
         thread.Start();
      }
      public void Stop(){
         running = false;
      }

      //Access dictionary
      public bool GetObject(string key, out SwapFlake f){
         f = null;
         lock(lockObject){
            if(flakes.ContainsKey(key)){
               f = flakes[key];
               return true;
            } else return false;
         }
      }

      //Debug
      #if UNITY_EDITOR
      public int GetPacketCount(){
         int count = packetCount;
         packetCount = 0;
         return count;
      }
      public override string ToString() {
         lock(lockObject){
            string s = "Port "+port+":";

            if(flakes.Count==0)s+="\n   (Empty)";
            else foreach(string k in flakes.Keys){
               bool noSource = string.IsNullOrEmpty(flakes[k].source);
               s+="\n   "+k+(noSource?"":" ("+flakes[k].source+")");
            }

            return s;
         }
      }
      #endif
   }
   internal class SendThread : ClientThread{
      IPAddress ip;
      string scope;

      public SendThread(string address, int port, string scope):base(address,port){
         ip = IPAddress.Any;
         this.scope = scope;
      }

      protected override ThreadStart ThreadStart{
         get{return Send;}
      }

      public void Send(){
         //UDP
         Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
         socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,1);
         socket.SendTimeout = TIMEOUT;

         IPEndPoint ipEndPoint = new IPEndPoint(ip,0);
         IPEndPoint send_ipEndPoint = new IPEndPoint(IPAddress.Parse(address),port);

         try{socket.Bind(ipEndPoint);}
         catch(SocketException e){
            Debug.LogWarning("Holojam.Network.Client: Error binding socket: "+address+" "+port+" "+e.ToString());
            running = false;
         }

         //Send loop
         while(running){
            System.Threading.Thread.Sleep(10); //For stability
            if(flakes.Values.Count==0)continue;

            lock(lockObject){
               //Create an update
               update = new update_protocol_v3.Update();
               update.label = scope;
               update.extra_data = System.Environment.MachineName;
               //Populate
               foreach(KeyValuePair<string,SwapFlake> entry in flakes){
                  update_protocol_v3.LiveObject f = entry.Value.ToFlake();
                  update.live_objects.Add(f);
               }
               //Serialize and send
               using (MemoryStream stream = new MemoryStream()){
                  packetCount++;
                  Serializer.Serialize<Update>(stream,update);
                  packet = stream.ToArray();
                  socket.SendTo(packet,send_ipEndPoint);
               }
            }

            if(!running){
               socket.Close();
               break;
            }
         }
      }

      //Given sending views, stage swaps for serialization and sending
      public void UpdateFlakes(View[] views){
         lock(lockObject){
            flakes.Clear();
            foreach(View view in views){
               SwapFlake f = SwapFlake.FromView(view);
               flakes[f.label] = f;
            }
         }
      }
   }
   internal class ReceiveThread : ClientThread{
      const float flakeTimeout = 0.4f; //Seconds until removal

      //Persistent dictionary for testing object timeout
      protected Dictionary<string,float> timers = new Dictionary<string,float>();

      public ReceiveThread(string address, int port):base(address,port){}

      protected override ThreadStart ThreadStart{
         get{return Receive;}
      }

      //Update the timers
      public void UpdateTimers(float delta){
         lock(lockObject){
            //Safe list for iteration
            List<string> keys = new List<string>(timers.Keys);
            foreach(string key in keys){
               timers[key]+=delta; //Increment timer
               //Remove object from both dictionaries on timeout
               if(timers[key]>flakeTimeout){
                  timers.Remove(key);
                  flakes.Remove(key);
               }
            }
         }
      }

      public void Receive(){
         Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
         socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,1);

         socket.Bind(new IPEndPoint(IPAddress.Any,port));
         socket.SetSocketOption(SocketOptionLevel.IP,SocketOptionName.AddMembership, 
            new MulticastOption(IPAddress.Parse(address)));
         socket.ReceiveTimeout = TIMEOUT;

         int byteCount = 0;
         //Receive loop
         while(running){
            //Get packet
            byteCount = socket.Receive(packet);
            //Deserialize incoming update
            update = Serializer.Deserialize<update_protocol_v3.Update>(
               new MemoryStream(packet,0,byteCount)
            );
            packetCount++;

            //Iterate through this update's flakes
            lock(lockObject){
               for(int i=0;i<update.live_objects.Count;++i){
                  update_protocol_v3.LiveObject flake = update.live_objects[i];
                  string label = flake.label;

                  timers[label]=0; //Reset timer--we received data for this object

                  SwapFlake swapFlake = new SwapFlake(update.label,update.extra_data,label);
                  flakes[update.label+"."+label] = swapFlake;

                  //Read to swap
                  swapFlake.isTracked = flake.is_tracked;
                  swapFlake.position = new Vector3(
                     (float)flake.x,(float)flake.y,(float)flake.z
                  );
                  swapFlake.rotation = new Quaternion(
                     (float)flake.qx,(float)flake.qy,(float)flake.qz,(float)flake.qw
                  );
                  swapFlake.bits = flake.button_bits;
                  swapFlake.blob = flake.extra_data;
               }
            }

            if(!running){
               socket.Close();
               break;
            }
         }
      }
   }
}
