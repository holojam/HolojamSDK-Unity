// LookInteraction.cs
// Created by Holojam Inc. on 03.01.17

using UnityEngine;

public class LookInteraction : MonoBehaviour {

  Spinnable lastSpinnable = null;
  RaycastHit hit;

  void FixedUpdate() {
    if (lastSpinnable) lastSpinnable.active = false;

    if (Physics.Raycast(transform.position, transform.forward, out hit)) {
      GameObject target = hit.collider.gameObject;
      Spinnable spinnable = target.GetComponent<Spinnable>() as Spinnable;

      if (spinnable) {
        spinnable.active = true;
        lastSpinnable = spinnable;
      }
    }
  }
}
