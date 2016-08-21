//DebugSphere.cs
//Created by Aaron C Gaudette on 02.07.16

using UnityEngine;

public class DebugSphere : MonoBehaviour{
	public float offset = 0;
	public Holojam.Tools.Actor target;
	
	void Update(){transform.position=target.eyes+target.look*offset;}
}