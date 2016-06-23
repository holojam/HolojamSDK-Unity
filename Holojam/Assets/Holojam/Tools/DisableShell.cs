using UnityEngine;
public class DisableShell : MonoBehaviour{
	Transform shell = null;
	void Update(){
		shell=transform.Find("Shell");
		if(shell!=null)shell.gameObject.SetActive(false);
	}
}