//Notifier.cs
//Created by Aaron C Gaudette on 16.01.17

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Holojam.Network{
   public class Notifier : Utility.Global<Notifier>{
      //Single-instance, so this is ok
      Dictionary<string,List<Callback>> subscriptions = new Dictionary<string,List<Callback>>();

      public delegate void Callback(View data = null);

      public static void AddSubscriber(Callback callback, string label){
         if(!Application.isPlaying)return;
         if(!global.subscriptions.ContainsKey(label))
            global.subscriptions[label] = new List<Callback>();
         global.subscriptions[label].Add(callback);
      }
      //null checking (auto-drop)? lazy?
      public static bool RemoveSubscriber(Callback callback, string label){
         if(!Application.isPlaying)return false;
         if(!global.subscriptions.ContainsKey(label))
            return false;
         int i = global.subscriptions[label].IndexOf(callback);
         if(i<0)return false;
         global.subscriptions[label].RemoveAt(i);
         if(global.subscriptions[label].Count==0)
            global.subscriptions.Remove(label);
         return true;
      }

      static internal void Publish(Event e){
         if(!global.subscriptions.ContainsKey(e.label))
            return;

         //Later we won't use a view for this, but a dedicated class
         View view = null;
         if(!e.notification){
            view = global.gameObject.AddComponent<View>();
            e.Load(view);
         }

         foreach(Callback callback in global.subscriptions[e.label])
            callback(view);
         global.StartCoroutine(global.DestroyView(view));
      }

      //?
      IEnumerator DestroyView(View view){
         yield return new WaitForSeconds(2);
         Destroy(view);
      }
   }
}
