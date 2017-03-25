using System;
using System.Collections;
using UnityEngine;

// TODO: Doc this all

namespace Holojam.Network {
  public class ConnectionTester : MonoBehaviour {

    const float SECONDS_BETWEEN_CHECKS = 5;
    const float TIMEOUT = 1;
    //const int FAILED_CHECKS_BEFORE_RESTART = 3;

    float timeSinceLastCheckSent = 0;
    bool awaitingResponse = false;
    string notificationLabelPrefix = "ClientConnectionCheck_";
    string randomSuffix = "?";

    // TODO: Try sending multiple pings before restarting

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
        Debug.LogError("Connection check response not received from server, restarting client...");
        Client.Restart();
        awaitingResponse = false;
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
      }
    }
  }
}