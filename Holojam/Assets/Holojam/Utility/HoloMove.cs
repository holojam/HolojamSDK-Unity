using UnityEngine;
using System.Collections;
using Holojam.Network;

namespace Holojam {
     public class HoloMove : MonoBehaviour {

          HolojamView view;

          // Use this for initialization
          void Start() {
               view = this.GetComponent<HolojamView>();
          }

          // Update is called once per frame
          void Update() {
               if (!view.isMine) {
                    this.transform.position = view.rawPosition;
                    this.transform.rotation = view.rawRotation;
               }
          }
     }
}

