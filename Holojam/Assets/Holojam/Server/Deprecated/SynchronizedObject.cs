using UnityEngine;
using System.Collections;

namespace Holojam.Network {
     public class SynchronizedObject : MonoBehaviour {

          public string label;

          [HideInInspector]
          public Vector3 position;
          [HideInInspector]
          public Quaternion rotation;
          [HideInInspector]
          public int bits = 0;
          [HideInInspector]
          public string blob = "";

          protected virtual void Update() {
               position = this.transform.position;
               rotation = this.transform.rotation;
          }
     }
}

