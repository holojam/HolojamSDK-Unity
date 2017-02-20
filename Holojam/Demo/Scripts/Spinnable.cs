// Spinnable.cs
// Created by Holojam Inc. on 03.01.17

using UnityEngine;

public class Spinnable : MonoBehaviour {

  public float maxSpeed = 8, increase = 300, decrease = 250;
  public bool active = false;
  float velocity;

  void FixedUpdate() {
    if (active)
      velocity += Time.deltaTime * increase;
    transform.Rotate(new Vector3(
       0, Mathf.Min(velocity * Time.deltaTime, maxSpeed), 0
    ));
    velocity = Mathf.Max(velocity - (active ? 0 : Time.deltaTime * decrease), 0);
  }
}
