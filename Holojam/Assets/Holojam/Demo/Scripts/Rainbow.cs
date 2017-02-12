// Rainbow.cs
// Created by Holojam Inc. on 03.01.17

using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Rainbow : MonoBehaviour {

  public Gradient gradient;
  public Vector2 speedRange = new Vector2(0.25f, 0.75f);

  float start, speed;

  void Awake() {
    speed = Random.Range(speedRange.x, speedRange.y);
    start = Random.Range(0, 1f);
  }

  void Update() {
    GetComponent<Renderer>().material.color = gradient.Evaluate(
       Mathf.Repeat(start + speed * Time.time, 1)
    );
  }
}
