using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Holojam {
	[RequireComponent(typeof(LineRenderer))]
	public class LineFactory : MonoBehaviour {


		/// <summary>
		/// Factory creating a single, continuous line out of multiple lines, allowing for reduced batch calls.
		/// </summary>
		public static List<LineFactory> factories = new List<LineFactory>();

		/////Protected//////
		//References
		//Primitives
		[Tooltip("The minimum distance between each point in the line. Increase this for lower quality but higher performance.")]
		public float detailDistance = 0.01f;
		[Tooltip("The width of the line being drawn.")]
		public float lineWidth = 0.02f;
		[Tooltip("The maximum distance from a brush at which a point will be erased.")]
		public float eraseDistance = 0.1f;
		[Tooltip("The speed at which an erased line will fade out.")]
		public float eraseSpeed = 1f;
		[Tooltip("The maximum number of points allowed in total.")]
		[Range(2, 10000)]
		public int maxPointCount = 100;

		/////Protected//////
		//References
		protected Dictionary<int, List<Point>> hashGrid = new Dictionary<int, List<Point>>();
		protected List<Line> lines = new List<Line>();
		protected LineRenderer lineRenderer;
		protected Texture2D texture;
		//Primitives
		protected Vector3 previousPosition = Vector3.zero; // last position
		protected Vector3 penultimatePosition = Vector3.zero; //second to last position
		protected int pointCount = 0;

		/////Private/////
		//References
		//Primitives
		private const int h1 = 12178051;
		private const int h2 = 12381319;
		private const int h3 = 15485863;
		private const float granularity = 0.1f;

		///////////////////////////////////////////////////////////////////////////
		//
		// Inherited from Monobehaviour
		//

		void Awake() {
			factories.Add(this);
			lineRenderer = this.GetComponent<LineRenderer>();
			texture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
			texture.filterMode = FilterMode.Point;
			lineRenderer.material.SetTexture("_Detail", texture);
			lineRenderer.sortingOrder = 1000;
		}

		void OnDestroy() {
			factories.Remove(this);
		}

		void Update() {
			this.lineRenderer.SetWidth(this.lineWidth, this.lineWidth);
			this.ReduceOpacityAndDequeue();
			//if (pointCount == 0 && lineRenderer.enabled) {
			//    lineRenderer.enabled = false;
			//} else if (pointCount > 0 && !lineRenderer.enabled) {
			//    lineRenderer.enabled = true;
			//}
		}

		///////////////////////////////////////////////////////////////////////////
		//
		// Internal LineFactory Functions
		//

		protected void ReduceOpacityAndDequeue() {
			bool hasChanged = false;

			for (int i = 0; i < lines.Count; i++) {
				if (lines[i].opacity < 1) {
					//we are changing the overall line somehow, the texture will need to be updated.
					hasChanged = true;
					//Reduce the opacity of the current line by the eraseSpeed.
					lines[i].opacity -= eraseSpeed * Time.deltaTime;
					//remove line once it has faded out completely.
					if (lines[i].opacity < 0) {
						this.DequeueLine(i);
					}
				}
			}

			if (hasChanged) {
				this.UpdateTexture();
			}
		}


		protected void DequeueLine(int index) {
			pointCount -= lines[index].points.Count;
			lines.RemoveAt(index);
			this.RebuildLine();
		}

		protected void RebuildLine() {
			//Rebuilds line after a line has been dequeued
			int count = 0;
			lineRenderer.SetVertexCount(pointCount);
			for (int i = 0; i < lines.Count; i++) {
				for (int j = 0; j < lines[i].points.Count; j++) {
					lineRenderer.SetPosition(count++, lines[i].points[j].position);
				}
			}
			lineRenderer.material.SetTextureScale("_MainTex", new Vector2(pointCount, 1));
		}

		protected void UpdateTexture() {
			texture.Resize(pointCount, 1);

			int count = 0;
			for (int i = 0; i < lines.Count; i++) {
				for (int j = 0; j < lines[i].points.Count; j++) {
					float a;
					//Set the alpha to be invisible between lines.
					if (j == 0 || j == lines[i].points.Count - 1) {
						a = 0;
					} else {
						a = lines[i].opacity;
					}
					texture.SetPixel(count++, 0, new Color(1, 1, 1, a));
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////
		//
		// Public LineFactory Functions
		//

		public void ClearFactory() {
			//set all lines to fade out
			foreach (Line line in lines) {
				if (line.opacity == 1f) {
					line.opacity = 0.99f;
				}
			}
		}

		public void AddLine(Vector3 startPosition) {
			int lineCount = lines.Count;
			int count = (lineCount > 0) ? (lines[lineCount - 1].points.Count) : 0;


			Debug.Log("adding line");
			//if there are lines AND the point count of the last line > 0
			if (count > 0) {
				//Add a point to have its alpha set to 0 (invisible connector)
				Point point = new Point(lines[lineCount - 1].points[count - 1].position, lines[lineCount - 1]);
				lines[lineCount - 1].points.Add(point);
				pointCount++;
			}

			Line line = new Line();
			Point startPoint = new Point(startPosition, line);
			line.points.Add(startPoint);
			lines.Add(line);
			pointCount++;
		}

		public void AddPoint(Vector3 position) {

			//Return if the position isn't far enough away from the previousPosition
			if (Vector3.Distance(previousPosition, position) < this.detailDistance)
				return;

            previousPosition = position;

			//Add the point to the current line.
			Line currentLine = lines[lines.Count - 1];
			Point point = new Point(position, currentLine);
			currentLine.points.Add(point);
			pointCount++;

			//Remove last index if maxPoints has been hit.
			if (pointCount > maxPointCount) {
				lines[0].points.RemoveAt(0);
				if (lines[0].points.Count == 0) {
					lines.RemoveAt(0);
				}
				pointCount--;
                Debug.Log(pointCount);
			}

			//Rebuild the line and update the texture.
			this.RebuildLine();
			this.UpdateTexture();

			//Add the point to the hashGrid (for erasing).
			int cell = GetHashedCell(position);
			if (hashGrid.ContainsKey(cell)) {
				hashGrid[cell].Add(point);
			} else {
				hashGrid[cell] = new List<Point>();
				hashGrid[cell].Add(point);
			}
		}

		public void Erase(Vector3 position) {
			//Only erase if there are points to erase!
			if (pointCount == 0)
				return;

			List<Point> pointsToCheck = new List<Point>();

			for (int i = -1; i <= 1; i++) {
				for (int j = -1; j <= 1; j++) {
					for (int k = -1; k <= 1; k++) {
						Vector3 offset = new Vector3(i, j, k) * granularity;
						int cell = GetHashedCell(position + offset);
						if (hashGrid.ContainsKey(cell)) {
							pointsToCheck.AddRange(hashGrid[cell]);
						}
					}
				}
			}

			foreach (Point point in pointsToCheck) {
				if (Vector3.Distance(point.position, position) < eraseDistance) {
					if (point.parent.opacity == 1)
						point.parent.opacity = 0.99f;
				}
			}
		}


		///////////////////////////////////////////////////////////////////////////
		//
		// Helper Functions
		//

		private int GetHashedCell(Vector3 position) {
			int x = Mathf.FloorToInt(position.x / granularity);
			int y = Mathf.FloorToInt(position.y / granularity);
			int z = Mathf.FloorToInt(position.z / granularity);
			return x * h1 + y * h2 + z * h3;
		}

	}

	public class Line {
		public List<Point> points = new List<Point>();
		public float opacity = 1f;

		public Line() { }

		public Line(float o) {
			opacity = o;
		}
	}

	public class Point {
		public Vector3 position = Vector3.zero;
		public Line parent = null;

		//A point must have a position and a parent.
		public Point(Vector3 p, Line rent) {
			this.position = p;
			this.parent = rent;
		}
	}
}

