// ActorAvatar.cs
// Created by Holojam Inc. on 05.07.16
// Example Actor extension

using UnityEngine;
using System.Collections;

public class ActorAvatar : Holojam.Tools.Actor {

  const float BLINK_TIME = 0.03f;
  readonly Vector2 BLINK_DELAY = new Vector2(1, 11);

  public Transform head;
  public GameObject mask;

  public Color motif = Holojam.Utility.Palette.Select(DEFAULT_COLOR);
  public Transform animatedEyes;
  public Material skinMaterial;

  float blinkDelay = 0, lastBlink = 0;

  // Override the orientation accessor to match the rotation assignment below
  public override Quaternion Orientation {
    get { return head != null ? head.rotation : Quaternion.identity; }
  }

  protected override void UpdateTracking() {
    if (Tracked) {
      transform.position = TrackedPosition;

      // Use a separate transform for rotation (a head) instead of the default (Actor transform)
      if (head != null) {
        head.localPosition = Vector3.zero;
        head.rotation = TrackedRotation;
      } else Debug.LogWarning("ActorAvatar: No head found for " + gameObject.name, this);
    }

    // Toggle mask--if this is a build actor, we don't want to render our mesh in
    // front of the camera
    if (mask != null)
      mask.SetActive(!IsBuild);
  }

  // Toggle the head object on fade in and fade out to hide the attached mesh

  protected override void FadeIn() {
    head.gameObject.SetActive(true);
  }

  protected override void FadeOut() {
    head.gameObject.SetActive(false);
  }

  void Start() { ApplyMotif(); }

  // Assign color and skin material to sub-renderers
  void ApplyMotif() {
    debugColor = motif;

    if (Application.isPlaying)
      foreach (Renderer r in GetComponentsInChildren<Renderer>(true)) {
        if (r.gameObject.tag == "Motif") r.material.color = motif;
        if (r.gameObject.tag == "Skin") r.material = skinMaterial;
      }
  }

  // Blink effect

  void LateUpdate() {
    if (Application.isPlaying && animatedEyes != null
      && Time.time > lastBlink + blinkDelay) {
      Blink();
    }
  }

  void Blink() {
    StartCoroutine(ToggleEyes(animatedEyes.localScale));
    blinkDelay = Random.Range(BLINK_DELAY.x, BLINK_DELAY.y);
    lastBlink = Time.time;
  }

  IEnumerator ToggleEyes(Vector3 initialScale, bool close = true) {
    float initialTime = Time.time;
    Vector3 shut = new Vector3(initialScale.x, 0, initialScale.z);
    while (close ? animatedEyes.localScale.y > 0 : animatedEyes.localScale.y < initialScale.y) {
      animatedEyes.localScale = Vector3.Lerp(
         close ? initialScale : shut, close ? shut : initialScale,
         (Time.time - initialTime) / BLINK_TIME
      );
      yield return null;
    }
    if (close) StartCoroutine(ToggleEyes(initialScale, false));
  }
}
