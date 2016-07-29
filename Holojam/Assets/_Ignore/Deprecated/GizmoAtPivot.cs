using UnityEngine;
using System.Collections;

public class GizmoAtPivot : MonoBehaviour {

	public Color color;

	void OnDrawGizmos(){
		Gizmos.color = color;
		Gizmos.DrawWireSphere (this.transform.position, 0.05f);
	}
}
