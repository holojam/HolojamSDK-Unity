// Metrics.cs
// Created by Holojam Inc. on 10.01.16

using UnityEngine;

namespace Holojam.Utility {

  /// <summary>
  /// Routes round-trip metrics packet, tags with the latest Unity render time.
  /// Requires MetricsInput and MetricsOutput classes.
  /// </summary>
  public sealed class Metrics : Network.EventIO<MetricsInput, MetricsOutput> {

    protected override void OnInput(string source, string scope, Network.Flake input) {
      output.Tick = input.ints[0];
      output.Push();
    }

    void Update() {
      // Send back time to render
      output.RenderTime = Time.smoothDeltaTime * 1000;
    }
  }

  public sealed class MetricsInput : Network.EventInput<MetricsInput, MetricsOutput> {

    public override string Scope { get { return "Holoscope"; } }
    public override string Label { get { return "Metrics"; } }
  }

  public sealed class MetricsOutput : Network.EventPusher {

    public override string Label { get { return "MetricsACK"; } }

    // Proxies
    public int Tick { set { data.ints[0] = value; } }
    public float RenderTime { set { data.floats[0] = value; } }

    public override void ResetData() {
      data = new Network.Flake(0, 0, 1, 1);
    }
  }
}
