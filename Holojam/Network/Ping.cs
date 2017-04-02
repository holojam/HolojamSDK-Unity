// Ping.cs
// Created by Holojam Inc. on 25.03.17

using System;
using System.Collections;
using UnityEngine;

namespace Holojam.Network {

  /// <summary>
  /// Periodically pings the relay and listens for a response.
  /// Attempts to restart the Client after multiple unanswered pings.
  /// </summary>
  public class Ping : MonoBehaviour {

    /// <summary>
    /// Interval between pings (seconds).
    /// </summary>
    const float PING_INTERVAL = 5;

    /// <summary>
    /// The duration (seconds) after which a ping is considered lost.
    /// </summary>
    const float TIMEOUT = 1;

    /// <summary>
    /// Number of unanswered pings before restarting the Client.
    /// </summary>
    const int MAX_FAILURES = 3;

    /// <summary>
    /// Indicates whether or not this client is connected.
    /// A client is considered to be disconnected if several pings have gone unanswered.
    /// If this value is false, one successful ping is required to set it to true.
    /// </summary>
    public bool Connected { get; private set; }

    /// <summary>
    /// The raw round-trip latency of this client, in milliseconds.
    /// Includes delay caused by render loop.
    /// </summary>
    public float LastRoundTripLatency { get; private set; }

    /// <summary>
    /// The round-trip latency of this client, in milliseconds.
    /// Approximately corrects the delay in LastRoundTripLatency using the frame delta time.
    /// Accurate to +/- (500 / FPS) ms.
    /// </summary>
    public int CorrectedLatency {
      get {
        return (int)Mathf.Max(0, Mathf.Round(LastRoundTripLatency - .5f * lastDelta * 1000));
      }
    }

    /// <summary>
    /// The timestamp of the last ping.
    /// </summary>
    float lastTime = 0;

    /// <summary>
    /// The delta time at the last ping.
    /// </summary>
    float lastDelta = 0;

    /// <summary>
    /// True if we've sent a ping and are waiting for a response.
    /// No pings are sent while waiting.
    /// </summary>
    bool awaitingResponse = false;

    /// <summary>
    /// The number of consecutive pings that have gone unanswered so far.
    /// </summary>
    int failures = 0;

    /// <summary>
    /// A random number to differentiate between multiple clients on the same machine.
    /// </summary>
    string randomSuffix = "";

    /// <summary>
    /// Allocate one int for latency calculation.
    /// </summary>
    Flake data = new Flake(0, 0, 0, 1);

    void Start() {
      Connected = false;
      randomSuffix = UnityEngine.Random.Range(1, int.MaxValue).ToString();

      Notifier.AddSubscriber(
        PingReceived, "Ping" + randomSuffix,
        Client.SEND_SCOPE
      );

      SendPing(); // Send immediately
    }

    void Update() {
      float difference = Time.unscaledTime - lastTime;

      // Regular ping
      if (!awaitingResponse && difference > PING_INTERVAL) {
        SendPing();
      // Timeout
      } else if (awaitingResponse && difference > TIMEOUT) {
        failures++;

        if (failures >= MAX_FAILURES) {
          Connected = false;

          Debug.LogWarning(
            "Holojam.Network.Ping: Relay connection attempts failed. Restarting client..."
          );

          Client.Restart();

          failures = 0;
          awaitingResponse = false;
          lastTime = Time.unscaledTime;
        } else {
          SendPing(); // Try again
        }
      }
    }

    /// <summary>
    /// Pings the relay by sending a notification.
    /// </summary>
    void SendPing() {
      data.ints[0] = (int)System.Diagnostics.Stopwatch.GetTimestamp();
      Client.PushEvent("Ping" + randomSuffix, data);
      awaitingResponse = true;
      lastTime = Time.unscaledTime;
      StartCoroutine(SetPingDelta());
    }

    IEnumerator SetPingDelta() {
      yield return null;
      lastDelta = Time.unscaledDeltaTime;
    }

    /// <summary>
    /// Callback for when a ping notification is received.
    /// Updates connection status, resets failure count.
    /// </summary>
    void PingReceived(string source, string scope, Flake input) {
      if (source == Canon.Origin()) {
        Connected = true;
        LastRoundTripLatency = ((int)System.Diagnostics.Stopwatch.GetTimestamp() - input.ints[0])
          * 1000 / (float)System.Diagnostics.Stopwatch.Frequency;

        failures = 0;
        awaitingResponse = false;
      }
    }
  }
}
