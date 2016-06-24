using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Holojam
{
	public class WandController : TrackedObject {

		private int buttonBits;
		
		void Update () {
               base.Update();

               int bits = 0;
               if (masterStream.GetButtonBits(liveObjectTag, out bits)) {
                    buttonBits = bits;
               } else {
                    buttonBits = bits;
               }
		}

		public int getButtonBits() {
			return buttonBits;
		}
	}
}