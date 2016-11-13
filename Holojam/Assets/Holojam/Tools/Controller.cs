//Controller.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;

namespace Holojam.Tools{
   [ExecuteInEditMode, RequireComponent(typeof(Network.View))]
   public abstract class Controller : MonoBehaviour{
      public string label = "Label";
      public string scope = ""; //Leave blank for default (Client send scope)
      public bool sending = false;

      public string brand{get{
         return string.IsNullOrEmpty(view.scope)?"":(view.scope+".")+view.label;
      }}

      protected delegate void ProcessDelegate();
      protected abstract ProcessDelegate Process{get;}

      //Manage view
      Network.View thisView = null;
      public Network.View view{get{
         if(thisView==null)thisView=GetComponent<Network.View>();
         return thisView;
      }}

      //Override these to modify view behavior
      protected virtual void UpdateViewLabel(string label){
         view.label = label;
      }
      protected virtual void UpdateViewScope(string scope){
         view.scope = scope!=""?scope:Network.Client.SEND_SCOPE;
      }
      protected virtual void UpdateViewSending(bool sending){
         view.sending = sending;
      }
      protected virtual void UpdateView(){
         UpdateViewLabel(label);
         UpdateViewScope(scope);
         UpdateViewSending(sending);
      }

      protected virtual void Update(){
         UpdateView(); //Mandatory call

         //Optional check--you probably don't want to run this code in edit mode
         if(!Application.isPlaying)return;

         Process();
      }
   }
}
