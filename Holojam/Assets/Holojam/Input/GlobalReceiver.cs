using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Holojam.IO {

     /// <summary>
     /// Generic receiver object for global IO.
     /// <remarks>
     /// Attaching this component to a GameObject allows for it to receive
     /// global inputs from Holojam.IO input systems.
     /// </remarks>
     /// </summary>
     /// <seealso cref="UnityEngine.MonoBehaviour" />
     public class GlobalReceiver : MonoBehaviour {

          /// <summary>
          /// Optional paired module for instance. 
          /// <remarks>
          /// If populated, this instance will only receive input from this module.
          /// </remarks>
          /// </summary>
          public BaseInputModule module;

          private static List<GlobalReceiver> instances = new List<GlobalReceiver>();
          //private static System.Object instanceLock = new System.Object();

          /// <summary>
          /// Returns a copied list of instances of GlobalReceiver.
          /// </summary>
          /// <returns></returns>
          public static List<GlobalReceiver> GetInstances() {
               List<GlobalReceiver> copyList = new List<GlobalReceiver>();
               //lock (instanceLock) {
               copyList.AddRange(instances);
               //}
               return copyList;
          }

          /// <summary>
          /// Adds this instance to the list of instances, when it is enabled.
          /// </summary>
          protected virtual void OnEnable() {
               //lock (instanceLock) {
               instances.Add(this);
               //}
          }

          /// <summary>
          /// Removes this instance from the list of instances, when it is disabled.
          /// </summary>
          protected virtual void OnDestroy() {
               //lock (instanceLock) {
               instances.Remove(this);
               //}
          }
     }
}

