using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Holojam.IO
{
	public class GlobalReceiver : MonoBehaviour
	{
		public static List<GlobalReceiver> instances = new List<GlobalReceiver> ();

		public BaseInputModule module;

		protected virtual void OnEnable ()
		{
			instances.Add (this);
		}

		protected virtual void OnDisable ()
		{
			instances.Remove (this);
		}
	}
}

