using UnityEngine;
using System.Collections;

public class GizmoAtPivot : MonoBehaviour {

	void OnDrawGizmos(){
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere (this.transform.position, 0.05f);
	}
}
