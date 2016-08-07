using UnityEngine;
using System.Collections;
using Holojam.IO;

namespace Holojam.Crayons {
	[RequireComponent(typeof(LineFactory))]
	public class WiiCrayon : GlobalReceiver, IGlobalWiiMoteAHandler, IGlobalWiiMoteBHandler {

		protected LineFactory factory;
		protected bool isDrawing = false;
		protected bool isErasing = false;

		void Awake() {
			factory = this.GetComponent<LineFactory>();
		}

		public void OnGlobalAPressDown(WiiMoteEventData eventData) {
			if (!this.isErasing) {
				this.isDrawing = true;
				factory.AddLine(this.transform.position);
			}
		}

		public void OnGlobalAPress(WiiMoteEventData eventData) {
			if (this.isDrawing) {
				factory.AddPoint(this.transform.position);
			}
		}

		public void OnGlobalAPressUp(WiiMoteEventData eventData) {
			if (this.isDrawing) {
				this.isDrawing = false;
			}
		}

		public void OnGlobalBPressDown(WiiMoteEventData eventData) {
			if (!this.isDrawing) {
				this.isErasing = true;
				factory.Erase(this.transform.position);
			}
		}

		public void OnGlobalBPress(WiiMoteEventData eventData) {
			if (this.isErasing) {
				factory.Erase(this.transform.position);
			}
		}

		public void OnGlobalBPressUp(WiiMoteEventData eventData) {
			if (this.isErasing) {
				this.isErasing = false;
			}
		}
	}
}

