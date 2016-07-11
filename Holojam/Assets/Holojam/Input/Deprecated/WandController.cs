#pragma warning disable 0618 //Deprecated

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Holojam
{
	public class WandController : TrackedObject {

		private int buttonBits;
		
		new void Update () {
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