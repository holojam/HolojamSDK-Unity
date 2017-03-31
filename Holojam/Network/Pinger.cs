using System;
using System.Collections;
using UnityEngine;


namespace Holojam.Network {
  /// <summary>
  /// Utility for avoiding issues with the client send/receive threads.
  /// Periodically pings the relay and listens for a response. If multiple pings go unanswered,
  /// restarts the send and recieve threads on this client.
  /// </summary>
  public class Pinger : MonoBehaviour {

    /// <summary>
    /// The time interval between ping messages.
    /// </summary>
    const float SECONDS_BETWEEN_CHECKS = 5;
    /// <summary>
    /// The amount of time in seconds before a ping is considered lost and restarts.
    /// </summary>
    const float TIMEOUT = 1;
    /// <summary>
    /// Number of unanswered pings before the client threads are restarted.
    /// </summary>
    const int FAILED_CHECKS_BEFORE_RESTART = 3;

    /// <summary>
    /// The time since the last ping was sent.
    /// </summary>
    float timeSinceLastCheckSent = 0;
    /// <summary>
    /// True if we've sent a ping and are waiting for a response. No other pings should go out
    /// while we're waiting.
    /// </summary>
    bool awaitingResponse = false;
    /// <summary>
    /// The prefix to the label the ping notification uses.
    /// </summary>
    string notificationLabelPrefix = "Ping_";
    /// <summary>
    /// This gets set to a random value so that multiple clients running on a single computer
    /// can still differentiate between ping messages meant for each one.
    /// </summary>
    string randomSuffix = "?";
    /// <summary>
    /// The number of consecutive pings that have gone unanswered so far.
    /// </summary>
    int consecutiveFailedChecks = 0;

    /// <summary>
    /// Indicates whether this client is connected.
    /// A client is considered to be disconnected if enough pings have failed to restart the client
    /// threads (see FAILED_CHECKS_BEFORE_RESTART). Once a client is disconnected, a single
    /// successful ping is enough to get it to report as being connected again.
    /// </summary>
    public bool Connected { get; private set; }

    void Start() {
      Connected = false;
      randomSuffix = UnityEngine.Random.Range(1, int.MaxValue).ToString();
      Notifier.AddSubscriber(PingReceived, notificationLabelPrefix + randomSuffix,
                             Client.SEND_SCOPE);
      // Set us up to send our first ping immediately
      timeSinceLastCheckSent = SECONDS_BETWEEN_CHECKS;
    }

    void Update() {
      timeSinceLastCheckSent += Time.deltaTime;
      if (!awaitingResponse && timeSinceLastCheckSent > SECONDS_BETWEEN_CHECKS) {
        SendPing();
      }
      else if (awaitingResponse && timeSinceLastCheckSent > TIMEOUT) {
        consecutiveFailedChecks++;

        if (consecutiveFailedChecks >= FAILED_CHECKS_BEFORE_RESTART) {
          Debug.LogError("Holojam.Network.ConnectionTester: Server not responding to connection "
                         + "test messages. Restarting client...");
          Client.Restart();
          consecutiveFailedChecks = 0;
          awaitingResponse = false;
          timeSinceLastCheckSent = 0;

          Connected = false;
        }
        else {
          SendPing();
        }
      }
    }

    /// <summary>
    /// Sends out a ping notification.
    /// </summary>
    void SendPing() {
      Client.Notify(notificationLabelPrefix + randomSuffix);
      timeSinceLastCheckSent = 0;
      awaitingResponse = true;
    }

    /// <summary>
    /// Callback for when a ping notification is received.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="scope"></param>
    /// <param name="data"></param>
    void PingReceived(string source, string scope, Flake data) {
      if (source == Canon.Origin()) {
        awaitingResponse = false;
        consecutiveFailedChecks = 0;
        Connected = true;
      }
    }
  }
}