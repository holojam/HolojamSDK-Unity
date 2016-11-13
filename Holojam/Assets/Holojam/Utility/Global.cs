//Global.cs
//Created by Aaron C Gaudette on 11.11.16
//

using UnityEngine;

namespace Holojam.Utility{
   public abstract class Global<T> : MonoBehaviour where T : MonoBehaviour{
      static T t;
      static UnityEngine.Object lockObject = new UnityEngine.Object();

      protected static T global{
         get{lock(lockObject){
            if(t==null){
               T[] objects = FindObjectsOfType(typeof(T)) as T[];
               if(objects.Length==0){
                  Debug.LogError("Global: "+
                     "No instances of "+typeof(T)+" in scene! (static access failure)"
                  );
                  return null;
               }
               t = objects[0];
               if(objects.Length>1){
                  Debug.LogWarning("Global: "+
                     "More than one instance ("+objects.Length+") of "+typeof(T)+" in scene! "+
                     "(expect undefined behavior)",t
                  );
               }
            }
            return t;
         }}
      }
   }
}
