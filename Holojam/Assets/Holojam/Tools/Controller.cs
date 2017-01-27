//Controller.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;

namespace Holojam.Tools{
   [ExecuteInEditMode, RequireComponent(typeof(Network.View))]
   public abstract class Controller : MonoBehaviour{
      public string brand{get{
         return string.IsNullOrEmpty(view.scope)?"":(view.scope+".")+view.label;
      }}

      //Update function delegate
      protected delegate void ProcessDelegate();
      protected abstract ProcessDelegate Process{get;}

      public Network.View view{get{return GetComponent<Network.View>();}}

      //Override these to modify view behavior
      protected abstract string labelField{get;}
      protected abstract string scopeField{get;}
      protected abstract bool isSending{get;}
      //Is this an event?
      protected virtual bool isIgnoringTracking{get{return false;}}

      //Override to select allocation amount (-1 = null/optional field)
      protected virtual int triplesCount{get{return -1;}}
      protected virtual int quadsCount{get{return -1;}}
      protected virtual int floatsCount{get{return -1;}}
      protected virtual int intsCount{get{return -1;}}
      protected virtual int charsCount{get{return -1;}}
      protected virtual bool hasText{get{return false;}}

      public void UpdateView(){
         view.label = labelField;
         view.scope = scopeField;
         view.sending = isSending;
         view.ignoreTracking = isIgnoringTracking;
      }

      //Call this to reset fields and re-allocate vectors
      //e.g. on a label change
      public void ResetView(){
         view.triples = triplesCount<0? null:new Vector3[triplesCount];
         view.quads = quadsCount<0? null:new Quaternion[quadsCount];
         view.floats = floatsCount<0? null:new float[floatsCount];
         view.ints = intsCount<0? null:new int[intsCount];
         view.chars = charsCount<0? null:new byte[charsCount];
         view.text = hasText? null:"";
      }

      public void UpdateTriple(int index, Vector3 input){view.triples[index] = input;}
      public void UpdateQuad(int index, Quaternion input){view.quads[index] = input;}
      public void UpdateFloat(int index, float input){view.floats[index] = input;}
      public void UpdateInt(int index, int input){view.ints[index] = input;}
      public void UpdateChar(int index, byte input){view.chars[index] = input;}
      public void UpdateText(string input){view.text = input;}

      public Vector3 GetTriple(int index){return view.triples[index];}
      public Quaternion GetQuad(int index){return view.quads[index];}
      public float GetFloat(int index){return view.floats[index];}
      public int GetInt(int index){return view.ints[index];}
      public byte GetChar(int index){return view.chars[index];}
      public string GetText(){return view.text;}

      protected virtual void Awake(){
         ResetView(); //Mandatory call
      }

      protected virtual void Update(){
         UpdateView(); //Mandatory call

         //Optional check--you probably don't want to run this code in edit mode
         if(!Application.isPlaying)return;

         Process();
      }
   }
}
