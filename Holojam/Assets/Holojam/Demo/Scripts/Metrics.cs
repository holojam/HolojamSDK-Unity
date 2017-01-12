//Metrics.cs
//Created by Aaron C Gaudette on 10.01.16

using UnityEngine;
using System.Collections;

public class Metrics : MonoBehaviour{
   public bool disable = false;
   Holojam.Network.View input, output;
   int lastTick = 0;

   void Awake(){
      if(disable)return;

      input = gameObject.AddComponent<Holojam.Network.View>()
         as Holojam.Network.View;
      input.label = "Metrics";
      input.scope = "Holoscope";
      input.sending = false;

      output = gameObject.AddComponent<Holojam.Network.View>()
         as Holojam.Network.View;
      output.label = "MetricsAck";
      output.scope = Holojam.Network.Client.SEND_SCOPE;
   }

   void Update(){
      if(disable)return;

      //Toggle ACK
      if(input.rawPosition.y>lastTick){
         lastTick = (int)input.rawPosition.y;
         output.sending = true;
         StartCoroutine(Switch());
      }

      //Output render time and tick
      output.rawPosition = new Vector3(
         Time.smoothDeltaTime*1000,
         lastTick,0
      );
   }

   IEnumerator Switch(){
      yield return new WaitForSeconds(Time.fixedDeltaTime);
      output.sending = false;
   }
}
