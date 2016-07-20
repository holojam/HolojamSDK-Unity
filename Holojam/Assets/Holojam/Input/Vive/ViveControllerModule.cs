#if VIVE
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Valve.VR;

namespace Holojam.IO
{
	public class ViveEventData : PointerEventData
	{
    /// <summary>
    /// The ViveControllerModule that manages the instance of ViveEventData.
    /// </summary>
		public ViveControllerModule Module {
      get; private set;
    }

    /// <summary>
    /// The SteamVR Tracked Object connected to the module.
    /// </summary>
		public SteamVR_TrackedObject SteamVRTrackedObject {
      get; private set;
    }

    /// <summary>
    /// The GameObject currently hit by a raycast from the module.
    /// </summary>
		public GameObject CurrentRaycast {
      get; private set;
    }

    /// <summary>
    /// The GameObject previously hit by a raycast from the module.
    /// </summary>
		public GameObject PreviousRaycast {
      get; private set;
    }

    /// <summary>
    /// The current touchpad axis values of the controller connected to the module.
    /// </summary>
		public Vector2 TouchpadAxis {
      get; private set;
    }

    /// <summary>
    /// The current trigger axis value of the controller connected to the module.
    /// </summary>
		public Vector2 TriggerAxis {
      get; private set;
    }

    /// <summary>
    /// The GameObject bound to the current press context of the Application Menu button.
    /// </summary>
		public GameObject ApplicationMenuPress {
      get; private set;
    }

    /// <summary>
    /// The GameObject bound to the current press context of the Grip button.
    /// </summary>
		public GameObject GripPress {
      get; private set;
    }

    /// <summary>
    /// The GameObject bound to the current press context of the Touchpad button.
    /// </summary>
		public GameObject TouchpadPress {
      get; private set;
    }

    /// <summary>
    /// The GameObject bound to the current press context of the Trigger button.
    /// </summary>
		public GameObject TriggerPress {
      get; private set;
    }

    /// <summary>
    /// The GameObject bound to the current touch context of the Touchpad button.
    /// </summary>
		public GameObject TouchpadTouch {
      get; private set;
    }

    /// <summary>
    /// The GameObject bound to the current touch context of the Trigger button.
    /// </summary>
		public GameObject TriggerTouch {
      get; private set;
    }

		public ViveEventData (EventSystem eventSystem)
			: base (eventSystem)
		{
		}
	}

	[RequireComponent (typeof(SteamVR_TrackedObject))]
	public class ViveControllerModule : BaseInputModule
	{


		/// <summary>
		/// 
		/// </summary>
		/// <TODO>
		///     * Handle a global receiver joining amidst a button being pressed.
		/// </TODO>

		/////Public/////
		//References
		public Transform boundObject;
		//Primitives
		//public bool forceModuleActive = false;
		public bool debugMode = false;
		public string interactTag;
		public float interactDistance = 10f;

		/////Private/////
		//References
		private Dictionary<EVRButtonId, GameObject> pressPairings = new Dictionary<EVRButtonId, GameObject> ();
		//private Dictionary<ulong, List<ViveGlobalReceiver>> pressReceivers = new Dictionary<ulong, List<ViveGlobalReceiver>>();
		private Dictionary<EVRButtonId, GameObject> touchPairings = new Dictionary<EVRButtonId, GameObject> ();
		//private Dictionary<ulong, List<ViveGlobalReceiver>> touchReceivers = new Dictionary<ulong, List<ViveGlobalReceiver>>();
		private SteamVR_TrackedObject controller;
		private ViveEventData eventData;
		//Primitives


		//Steam Controller button and axis ids
		EVRButtonId[] buttonIds = new EVRButtonId[] {
			EVRButtonId.k_EButton_ApplicationMenu,
			EVRButtonId.k_EButton_Grip,
			EVRButtonId.k_EButton_SteamVR_Touchpad,
			EVRButtonId.k_EButton_SteamVR_Trigger
		};

		EVRButtonId[] touchIds = new EVRButtonId[] {
			EVRButtonId.k_EButton_SteamVR_Touchpad,
			EVRButtonId.k_EButton_SteamVR_Trigger
		};

		EVRButtonId[] axisIds = new EVRButtonId[] {
			EVRButtonId.k_EButton_SteamVR_Touchpad,
			EVRButtonId.k_EButton_SteamVR_Trigger
		};




		void Awake ()
		{
			eventData = new ViveEventData (EventSystem.current);

			controller = this.GetComponent<SteamVR_TrackedObject> ();

			eventData.Module = this;

			foreach (EVRButtonId button in buttonIds) {
				pressPairings.Add (button, null);
			}

			foreach (EVRButtonId button in touchIds) {
				touchPairings.Add (button, null);
			}
		}

		void OnEnable ()
		{
			
		}

		void OnDisable ()
		{
			foreach (EVRButtonId button in buttonIds) {
				this.ExecutePressUp (button);
				this.ExecuteGlobalPressUp (button);
			}

			foreach (EVRButtonId button in touchIds) {
				this.ExecuteTouchUp (button);
				this.ExecuteGlobalTouchUp (button);
			}

			eventData.CurrentRaycast = null;
			this.UpdateCurrentObject ();

		}

		void Update ()
		{
			this.PositionBoundObject ();
			this.CastRayFromBoundObject ();
			this.UpdateCurrentObject ();
			//this.PlaceCursor();
			this.HandleButtons ();
		}

		public override void Process ()
		{

		}

		void PositionBoundObject ()
		{
			if (boundObject == null)
				boundObject = this.transform;
			boundObject.localPosition = controller.transform.localPosition;
			boundObject.localRotation = controller.transform.localRotation;
		}

		private List<RaycastHit> hits = new List<RaycastHit> ();
		private Ray ray;

		void CastRayFromBoundObject ()
		{
			hits.Clear ();

			//CAST RAY
			Vector3 v = boundObject.position;
			Quaternion q = boundObject.rotation;
			ray = new Ray (v, q * Vector3.forward);
			hits.AddRange (Physics.RaycastAll (ray, interactDistance));
			eventData.PreviousRaycast = eventData.CurrentRaycast;

			if (hits.Count == 0) {
				eventData.CurrentRaycast = null;
				return;
			}

			//FIND THE CLOSEST OBJECT
			RaycastHit minHit = hits [0];
			for (int i = 0; i < hits.Count; i++) {
				if (hits [i].distance < minHit.distance) {
					minHit = hits [i];
				}
			}

			//MAKE SURE CLOSEST OBJECT IS INTERACTABLE
			if (interactTag != null && interactTag.Length > 1 && !minHit.transform.tag.Equals (interactTag)) {
				eventData.CurrentRaycast = null;
				return;
			} else {
				eventData.CurrentRaycast = minHit.transform.gameObject;
			}
		}

		void UpdateCurrentObject ()
		{
			this.HandlePointerExitAndEnter (eventData);
		}

		void HandlePointerExitAndEnter (ViveEventData eventData)
		{
			if (eventData.PreviousRaycast != eventData.CurrentRaycast) {
				ExecuteEvents.Execute<IPointerEnterHandler> (eventData.CurrentRaycast, eventData, ExecuteEvents.pointerEnterHandler);
				ExecuteEvents.Execute<IPointerExitHandler> (eventData.PreviousRaycast, eventData, ExecuteEvents.pointerExitHandler);
			}
		}

		void PlaceCursor ()
		{
			//TODO.
		}

		void HandleButtons ()
		{
			int index = (int)controller.index;

			eventData.TouchpadAxis = SteamVR_Controller.Input (index).GetAxis (axisIds [0]);
			eventData.TriggerAxis = SteamVR_Controller.Input (index).GetAxis (axisIds [1]);

			//Press
			foreach (EVRButtonId button in buttonIds) {
				if (this.GetPressDown (index, button)) {
					this.ExecutePressDown (button);
					this.ExecuteGlobalPressDown (button);
				} else if (this.GetPress (index, button)) {
					this.ExecutePress (button);
					this.ExecuteGlobalPress (button);
				} else if (this.GetPressUp (index, button)) {
					this.ExecutePressUp (button);
					this.ExecuteGlobalPressUp (button);
				}
			}

			//Touch
			foreach (EVRButtonId button in touchIds) {
				if (this.GetTouchDown (index, button)) {
					this.ExecuteTouchDown (button);
					this.ExecuteGlobalTouchDown (button);
				} else if (this.GetTouch (index, button)) {
					this.ExecuteTouch (button);
					this.ExecuteGlobalTouch (button);
				} else if (this.GetTouchUp (index, button)) {
					this.ExecuteTouchUp (button);
					this.ExecuteGlobalTouchUp (button);
				}
			}
		}

		private void ExecutePressDown (EVRButtonId id)
		{
			GameObject go = eventData.CurrentRaycast;
			if (go == null)
				return;

			switch (id) {
			case EVRButtonId.k_EButton_ApplicationMenu:
				eventData.ApplicationMenuPress = go;
				ExecuteEvents.Execute<IApplicationMenuPressDownHandler> (eventData.ApplicationMenuPress, eventData,
					(x, y) => x.OnApplicationMenuPressDown (eventData));
				break;
			case EVRButtonId.k_EButton_Grip:
				eventData.GripPress = go;
				ExecuteEvents.Execute<IGripPressDownHandler> (eventData.GripPress, eventData,
					(x, y) => x.OnGripPressDown (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				eventData.TouchpadPress = go;
				ExecuteEvents.Execute<ITouchpadPressDownHandler> (eventData.TouchpadPress, eventData,
					(x, y) => x.OnTouchpadPressDown (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				eventData.TriggerPress = go;
				ExecuteEvents.Execute<ITriggerPressDownHandler> (eventData.TriggerPress, eventData,
					(x, y) => x.OnTriggerPressDown (eventData));
				break;
			}

			//Add pairing.
			pressPairings [id] = go;
		}

		private void ExecutePress (EVRButtonId id)
		{
			if (pressPairings [id] == null)
				return;

			switch (id) {
			case EVRButtonId.k_EButton_ApplicationMenu:
				ExecuteEvents.Execute<IApplicationMenuPressHandler> (eventData.ApplicationMenuPress, eventData,
					(x, y) => x.OnApplicationMenuPress (eventData));
				break;
			case EVRButtonId.k_EButton_Grip:
				ExecuteEvents.Execute<IGripPressHandler> (eventData.GripPress, eventData,
					(x, y) => x.OnGripPress (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				ExecuteEvents.Execute<ITouchpadPressHandler> (eventData.TouchpadPress, eventData,
					(x, y) => x.OnTouchpadPress (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				ExecuteEvents.Execute<ITriggerPressHandler> (eventData.TriggerPress, eventData,
					(x, y) => x.OnTriggerPress (eventData));
				break;
			}
		}

		private void ExecutePressUp (EVRButtonId id)
		{
			if (pressPairings [id] == null)
				return;

			switch (id) {
			case EVRButtonId.k_EButton_ApplicationMenu:
				ExecuteEvents.Execute<IApplicationMenuPressUpHandler> (eventData.ApplicationMenuPress, eventData,
					(x, y) => x.OnApplicationMenuPressUp (eventData));
				eventData.ApplicationMenuPress = null;
				break;
			case EVRButtonId.k_EButton_Grip:
				ExecuteEvents.Execute<IGripPressUpHandler> (eventData.GripPress, eventData,
					(x, y) => x.OnGripPressUp (eventData));
				eventData.GripPress = null;
				break;
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				ExecuteEvents.Execute<ITouchpadPressUpHandler> (eventData.TouchpadPress, eventData,
					(x, y) => x.OnTouchpadPressUp (eventData));
				eventData.TouchpadPress = null;
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				ExecuteEvents.Execute<ITriggerPressUpHandler> (eventData.TriggerPress, eventData,
					(x, y) => x.OnTriggerPressUp (eventData));
				eventData.TriggerPress = null;
				break;
			}

			pressPairings [id] = null;
		}

		private void ExecuteTouchDown (EVRButtonId id)
		{
			GameObject go = eventData.CurrentRaycast;
			if (go == null)
				return;

			switch (id) {
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				eventData.TouchpadTouch = go;
				ExecuteEvents.Execute<ITouchpadTouchDownHandler> (eventData.TouchpadTouch, eventData,
					(x, y) => x.OnTouchpadTouchDown (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				eventData.TriggerTouch = go;
				ExecuteEvents.Execute<ITriggerTouchDownHandler> (eventData.TriggerTouch, eventData,
					(x, y) => x.OnTriggerTouchDown (eventData));
				break;
			}

			touchPairings [id] = go;
		}

		private void ExecuteTouch (EVRButtonId id)
		{
			if (touchPairings [id] == null)
				return;

			switch (id) {
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				ExecuteEvents.Execute<ITouchpadTouchHandler> (eventData.TouchpadTouch, eventData,
					(x, y) => x.OnTouchpadTouch (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				ExecuteEvents.Execute<ITriggerTouchHandler> (eventData.TriggerTouch, eventData,
					(x, y) => x.OnTriggerTouch (eventData));
				break;
			}
		}

		private void ExecuteTouchUp (EVRButtonId id)
		{
			if (touchPairings [id] == null)
				return;

			switch (id) {
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				ExecuteEvents.Execute<ITouchpadTouchUpHandler> (eventData.TouchpadTouch, eventData,
					(x, y) => x.OnTouchpadTouchUp (eventData));
				eventData.TouchpadTouch = null;
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				ExecuteEvents.Execute<ITriggerTouchUpHandler> (eventData.TriggerTouch, eventData,
					(x, y) => x.OnTriggerTouchUp (eventData));
				eventData.TriggerTouch = null;
				break;
			}
		}

		private void ExecuteGlobalPressDown (EVRButtonId id)
		{
			switch (id) {
			case EVRButtonId.k_EButton_ApplicationMenu:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalApplicationMenuPressDownHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalApplicationMenuPressDown (eventData));
				break;
			case EVRButtonId.k_EButton_Grip:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalGripPressDownHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalGripPressDown (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTouchpadPressDownHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTouchpadPressDown (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTriggerPressDownHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTriggerPressDown (eventData));
				break;
			}
		}

		private void ExecuteGlobalPress (EVRButtonId id)
		{
			switch (id) {
			case EVRButtonId.k_EButton_ApplicationMenu:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalApplicationMenuPressHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalApplicationMenuPress (eventData));
				break;
			case EVRButtonId.k_EButton_Grip:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalGripPressHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalGripPress (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTouchpadPressHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTouchpadPress (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTriggerPressHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTriggerPress (eventData));
				break;
			}
		}

		private void ExecuteGlobalPressUp (EVRButtonId id)
		{
			switch (id) {
			case EVRButtonId.k_EButton_ApplicationMenu:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalApplicationMenuPressUpHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalApplicationMenuPressUp (eventData));
				break;
			case EVRButtonId.k_EButton_Grip:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalGripPressUpHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalGripPressUp (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTouchpadPressUpHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTouchpadPressUp (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTriggerPressUpHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTriggerPressUp (eventData));
				break;
			}
		}

		private void ExecuteGlobalTouchDown (EVRButtonId id)
		{
			switch (id) {
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTouchpadTouchDownHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTouchpadTouchDown (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTriggerTouchDownHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTriggerTouchDown (eventData));
				break;
			}
		}

		private void ExecuteGlobalTouch (EVRButtonId id)
		{
			switch (id) {
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTouchpadTouchHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTouchpadTouch (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTriggerTouchHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTriggerTouch (eventData));
				break;
			}
		}

		private void ExecuteGlobalTouchUp (EVRButtonId id)
		{
			switch (id) {
			case EVRButtonId.k_EButton_SteamVR_Touchpad:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTouchpadTouchUpHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTouchpadTouchUp (eventData));
				break;
			case EVRButtonId.k_EButton_SteamVR_Trigger:
				foreach (GlobalReceiver r in GlobalReceiver.instances)
					if (!r.module || r.module.Equals (this))
						ExecuteEvents.Execute<IGlobalTriggerTouchUpHandler> (r.gameObject, eventData,
							(x, y) => x.OnGlobalTriggerTouchUp (eventData));
				break;
			}
		}



		bool GetPressDown (int index, EVRButtonId button)
		{
			return SteamVR_Controller.Input (index).GetPressDown (button);
		}

		bool GetPress (int index, EVRButtonId button)
		{
			return SteamVR_Controller.Input (index).GetPress (button);
		}

		bool GetPressUp (int index, EVRButtonId button)
		{
			return SteamVR_Controller.Input (index).GetPressUp (button);
		}

		bool GetTouchDown (int index, EVRButtonId button)
		{
			return SteamVR_Controller.Input (index).GetTouchDown (button);
		}

		bool GetTouch (int index, EVRButtonId button)
		{
			return SteamVR_Controller.Input (index).GetTouch (button);
		}

		bool GetTouchUp (int index, EVRButtonId button)
		{
			return SteamVR_Controller.Input (index).GetTouchUp (button);
		}
	}

}
#endif
