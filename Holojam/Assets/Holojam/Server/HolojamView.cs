using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Holojam.Network {
  public class HolojamView : MonoBehaviour {
    public static List<HolojamView> instances = new List<HolojamView>();

    [SerializeField]
    private string label;
    [SerializeField]
    private bool isMine;

    private Vector3 rawPosition;
    private Quaternion rawRotation;
    private int bits;
    private string blob;
    private bool isTracked = false;
    private bool inObjectPool = false;

    public string Label {
      get {
        return label;
      }
      set {
        label = value;
      }
    }

    public bool IsMine {
      get {
        return isMine;
      }
      set {
        isMine = value;
      }
    }

    public Vector3 RawPosition {
      get {
        return rawPosition;
      }
      set {
        rawPosition = value;
      }
    }

    public Quaternion RawRotation {
      get {
        return rawRotation;
      }
      set {
        rawRotation = value;
      }
    }

    public int Bits {
      get {
        return bits;
      }
      set {
        bits = value;
      }
    }

    public string Blob {
      get {
        return blob;
      }
      set {
        blob = value;
      }
    }

    public bool IsTracked {
      get {
        return Application.isPlaying && instances.Contains(this) && (isTracked || isMine);
      }
      set {
        isTracked = value;
      }
    }

    public bool InObjectPool {
      get {
        return inObjectPool;
      }
      set {
        inObjectPool = value;
      }
    }

    private void OnEnable() {
      instances.Add(this);
    }

    private void OnDisable() {
      instances.Remove(this);
    }
  }
}