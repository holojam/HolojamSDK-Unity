using System;
using System.Collections;
using UnityEngine;

// TODO: Doc this all

namespace Holojam.Network {
  public class ConnectionTester : MonoBehaviour {

    const float SECONDS_BETWEEN_CHECKS = 5;
    const float TIMEOUT = 1;
    const int FAILED_CHECKS_BEFORE_RESTART = 3;

    float timeSinceLastCheckSent = 0;
    bool awaitingResponse = false;
    string notificationLabelPrefix = "ClientConnectionCheck_";
    string randomSuffix = "?";
    int consecutiveFailedChecks = 0;

    void Start() {
      randomSuffix = UnityEngine.Random.Range(1, int.MaxValue).ToString();
      Notifier.AddSubscriber(PingReceived, notificationLabelPrefix + randomSuffix,
                             Client.SEND_SCOPE);
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
        }
        else {
          SendPing();
        }
      }
    }

    void SendPing() {
      Client.Notify(notificationLabelPrefix + randomSuffix);
      timeSinceLastCheckSent = 0;
      awaitingResponse = true;
    }

    void PingReceived(string source, string scope, Flake data) {
      if (source == Canon.Origin()) {
        awaitingResponse = false;
        consecutiveFailedChecks = 0;
      }
    }
  }
}