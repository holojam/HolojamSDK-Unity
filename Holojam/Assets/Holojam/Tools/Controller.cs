//Controller.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;

namespace Holojam.Tools{
   [ExecuteInEditMode, RequireComponent(typeof(Network.View))]
   public abstract class Controller : MonoBehaviour{
      public string brand{get{
         return scopeField + "." + labelField;
      }}

      //Update function delegate
      protected delegate void ProcessDelegate();
      protected abstract ProcessDelegate Process{get;}

      public Network.View view{
         //Undefined selection
         get{return GetComponent<Network.View>();}
      }

      //Override these to modify view behavior
      public abstract string labelField{get;}
      public abstract string scopeField{get;}
      public abstract bool isSending{get;}
      //Is this an event?
      public virtual bool isIgnoringTracking{get{return false;}}

      //Override to select allocation amount (-1 = null/optional field)
      public virtual int tripleCount{get{return -1;}}
      public virtual int quadCount{get{return -1;}}
      public virtual int floatCount{get{return -1;}}
      public virtual int intCount{get{return -1;}}
      public virtual int charCount{get{return -1;}}
      public virtual bool hasText{get{return false;}}

      public void UpdateView(){
         view.label = labelField;
         view.scope = scopeField;
         view.sending = isSending;
         view.ignoreTracking = isIgnoringTracking;
      }

      //Call this to reset fields and re-allocate vectors
      //e.g. on a label change
      public void ResetView(){
         view.triples = tripleCount<0? null:new Vector3[tripleCount];
         view.quads = quadCount<0? null:new Quaternion[quadCount];
         view.floats = floatCount<0? null:new float[floatCount];
         view.ints = intCount<0? null:new int[intCount];
         view.chars = charCount<0? null:new byte[charCount];
         view.text = hasText? null:"";
      }

      public void SetTriple(int index, Vector3 input){view.triples[index] = input;}
      public void SetQuad(int index, Quaternion input){view.quads[index] = input;}
      public void SetFloat(int index, float input){view.floats[index] = input;}
      public void SetInt(int index, int input){view.ints[index] = input;}
      public void SetChar(int index, byte input){view.chars[index] = input;}
      public void SetText(string input){view.text = input;}

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
