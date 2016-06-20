using UnityEngine;
using System.Collections.Generic;

public class FingerRenderer : MonoBehaviour {
	public Transform start, end, hand;
	public Material mat;
	public float L0, L1, L2;

	private float[] A = {0.00f, 0.50f, 0.80f, 0.95f, 1.00f};
	private float[] B = {0.00f, 0.33f, 0.55f, 0.80f, 1.00f};

	private List<GameObject> boxes;

	// Use this for initialization
	void Start () {
		boxes = new List<GameObject> ();
		for (int i = 0; i < 3; i++) {
			if (L2 == 0 && i == 2) break;
			GameObject box = GameObject.CreatePrimitive (PrimitiveType.Cube);
			box.GetComponent<Renderer> ().material = mat;
			boxes.Add (box);
		}
	}
	
	// Update is called once per frame
	void Update () {
		float angle_t = Vector3.Distance(start.position, end.position) / (L0 + L1 + L2);
		angle_t = Mathf.Min (1f, angle_t);
		Vector3 v0, v3;
		v0 = new Vector3 (0, 0, 0);
		v3 = new Vector3 (end.position.x - start.position.x, end.position.y - start.position.y, end.position.z - start.position.z);
		v3 = Quaternion.Inverse(Quaternion.LookRotation (start.forward, start.up)) * v3;
		Vector2 p0, p1, p2, p3;
		p0 = new Vector2(0, 0);
		p1 = new Vector2(0, 0);
		p2 = new Vector2(0, 0);
		p3 = new Vector2 (v3.z, v3.y);

		if (p3.magnitude > L0 + L1 + L2) {
			p3 = (L0 + L1 + L2) * p3.normalized;
		}
		float t = Mathf.Clamp ((p3.magnitude - L0) / (L1 + L2), 0, 1);
		for (int i = 0; i < A.Length - 1; i++) {
			if (t >= A [i] && t < A [i + 1]) {
				t = Mathf.Lerp(B[i], B[i+1], Mathf.InverseLerp(A[i], A[i+1], t));
			}
		}
		float u = Mathf.Lerp (p3.y, p3.x, t);
		float v = Mathf.Lerp (-p3.x, p3.y, t);
		Vector2 uv = (new Vector2 (u, v)).normalized;
		p2 = p3 - uv * L2;
		Vector2 C = new Vector2 (p2.x, p2.y);
		Vector2 D = new Vector2 (-p3.y, p3.x);
		float cc = Vector2.Dot (C, C);
		float dd = Vector2.Dot (D, D);
		float x = (1f + (L0 * L0 - L1 * L1) / cc) / 2f;
		float y = Vector2.Dot (C, D) / cc;
		D -= y * C;
		y = Mathf.Sqrt (Mathf.Max(0f, L0 * L0 - cc * x * x) / dd);
		D = x * C + y * D;
		p1 = new Vector2 (D.x, D.y);

		Vector3[] nodes = new Vector3[4];
		for (int i = 0; i < 4; i++) {
			nodes [i] = new Vector3 ();
		}
		nodes [0] = Quaternion.LookRotation (start.forward, start.up) * new Vector3 (0f, p0.y, p0.x) + start.position;
		nodes [1] = Quaternion.LookRotation (start.forward, start.up) * new Vector3 (0f, p1.y, p1.x) + start.position;
		nodes [2] = Quaternion.LookRotation (start.forward, start.up) * new Vector3 (0f, p2.y, p2.x) + start.position;
		nodes [3] = Quaternion.LookRotation (start.forward, start.up) * new Vector3 (0f, p3.y, p3.x) + start.position;

		for (int i = 0; i < nodes.Length - 1; i++) {
			if (L2 == 0 && i == 2) break;
			GameObject box = boxes [i];
			Vector3 a = nodes [i];
			Vector3 b = nodes [i + 1];
			box.transform.forward = Vector3.Normalize(b - a);
			box.transform.position = (a + b) / 2f;
			box.transform.localScale = new Vector3 (0.015f, 0.015f, Vector3.Distance (a, b));
		}
	}

	public void setColor (Color c) {
		foreach (GameObject box in boxes) {
			box.GetComponent<Renderer> ().material.SetColor ("_Color", c);
		}
	}
}
