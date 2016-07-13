using UnityEngine;
using System.Collections;
using Holojam.Network;

namespace Holojam.Demo {
     public class BallDropStateMachine : MonoBehaviour {

          enum BallDropState {
               IDLE = 0,
               PLAYING = 1,
               LOSE = 2
          }

          private BallDropState currentState = BallDropState.IDLE;
         
          public SynchronizedObject ball;
          public SynchronizedObject state;

          private MasterStream masterStream;
          private Holojam.Network.MasterServer masterServer;

          void Awake() {
               masterStream = MasterStream.Instance;
               masterServer = Holojam.Network.MasterServer.Instance;
          }

          // Update is called once per frame
          void Update() {
               if (masterServer.isMaster) {
                    //ball managed by this build.
                    if (currentState == BallDropState.IDLE) {
                         Debug.Log("Idle (master)");
                         ball.transform.position = Vector3.up * 5f;
                         ball.GetComponent<Rigidbody>().velocity = Vector3.zero;

                         currentState = BallDropState.PLAYING;
                    } else if (currentState == BallDropState.PLAYING) {
                         Debug.Log("Playing (master)");
                         if (ball.transform.position.y < 0f) {
                              currentState = BallDropState.LOSE;
                         }
                    } else {
                         Debug.Log("Lose (master)");
                         currentState = BallDropState.IDLE;
                    }

                    state.bits = (int)currentState;
               } else {
                    int stateBits;
                    if (masterStream.GetButtonBits(state.label, out stateBits)) {
                         switch ((BallDropState) stateBits) {
                              case BallDropState.IDLE:
                                   this.DoIdle();
                                   break;
                              case BallDropState.PLAYING:
                                   this.DoPlaying();
                                   break;
                              case BallDropState.LOSE:
                                   this.DoLose();
                                   break;

                         }
                    }
               }
          }

          void DoIdle() {
               Debug.Log("Idle (client)");
          }

          void DoPlaying() {
               Debug.Log("Playing (client)");
               Vector3 position;
               Quaternion rotation;
               if (masterStream.GetPosition(ball.label, out position) && masterStream.GetRotation(ball.label, out rotation)) {
                    ball.transform.position = Vector3.Lerp(ball.transform.position,position,Time.deltaTime);
                    ball.transform.rotation = Quaternion.Slerp(ball.transform.rotation, rotation, Time.deltaTime);
               }
          }

          void DoLose() {
               Debug.Log("Lose (client)");
          }
     }
}

