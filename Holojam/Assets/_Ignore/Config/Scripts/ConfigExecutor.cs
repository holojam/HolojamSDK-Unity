using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Holojam.Config {
     public class ConfigExecutor : MonoBehaviour {

          public Text headsetNumberText;
          public Button configButton;

          private AndroidJavaClass holojamJavaClass;
          private AndroidJavaObject currentActivity;

          void Awake() {
               if (!UnityEngine.Application.isMobilePlatform)
                    return;
               currentActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
               holojamJavaClass = new AndroidJavaClass("edu.nyu.mrl.mrlconfig.HolojamJavaService");
          }

          void Update() {
               if (headsetNumberText != null && !string.IsNullOrEmpty(headsetNumberText.text)) {
                    configButton.gameObject.SetActive(true);
               } else {
                    configButton.gameObject.SetActive(false);
               }
          }

          public void OnGenerateConfigFile() {
               bool success = holojamJavaClass.CallStatic<bool>("setConfigNumber", currentActivity, int.Parse(headsetNumberText.text));
               if (success) {
                    Debug.Log("Successfully set config number to be: " + int.Parse(headsetNumberText.text));
               } else {
                    Debug.Log("Failed to set config number.");
               }
          }
     }
}

