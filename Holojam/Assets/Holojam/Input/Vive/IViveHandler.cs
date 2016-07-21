#if VIVE
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Holojam.IO {
	public interface IViveHandler : IPointerViveHandler, IGlobalViveHandler { }

	public interface IPointerViveHandler : IApplicationMenuHandler, IGripHandler, ITouchpadHandler, ITriggerHandler { }

	//APPLICATION MENU HANDLER
	public interface IApplicationMenuHandler : IApplicationMenuPressDownHandler, IApplicationMenuPressHandler, IApplicationMenuPressUpHandler { }

	public interface IApplicationMenuPressDownHandler : IEventSystemHandler {
		void OnApplicationMenuPressDown(ViveControllerModule.EventData eventData);
	}

	public interface IApplicationMenuPressHandler : IEventSystemHandler {
		void OnApplicationMenuPress(ViveControllerModule.EventData eventData);
	}

	public interface IApplicationMenuPressUpHandler : IEventSystemHandler {
		void OnApplicationMenuPressUp(ViveControllerModule.EventData eventData);
	}

	//GRIP HANDLER
	public interface IGripHandler : IGripPressDownHandler, IGripPressHandler, IGripPressUpHandler { }

	public interface IGripPressDownHandler : IEventSystemHandler {
		void OnGripPressDown(ViveControllerModule.EventData eventData);
	}

	public interface IGripPressHandler : IEventSystemHandler {
		void OnGripPress(ViveControllerModule.EventData eventData);
	}
	public interface IGripPressUpHandler : IEventSystemHandler {
		void OnGripPressUp(ViveControllerModule.EventData eventData);
	}

	//TOUCHPAD HANDLER
	public interface ITouchpadHandler : ITouchpadPressSetHandler, ITouchpadTouchSetHandler { }
	public interface ITouchpadPressSetHandler : ITouchpadPressDownHandler, ITouchpadPressHandler, ITouchpadPressUpHandler { }
	public interface ITouchpadTouchSetHandler : ITouchpadTouchDownHandler, ITouchpadTouchHandler, ITouchpadTouchUpHandler { }

	public interface ITouchpadPressDownHandler : IEventSystemHandler {
		void OnTouchpadPressDown(ViveControllerModule.EventData eventData);
	}

	public interface ITouchpadPressHandler : IEventSystemHandler {
		void OnTouchpadPress(ViveControllerModule.EventData eventData);
	}

	public interface ITouchpadPressUpHandler : IEventSystemHandler {
		void OnTouchpadPressUp(ViveControllerModule.EventData eventData);
	}

	public interface ITouchpadTouchDownHandler : IEventSystemHandler {
		void OnTouchpadTouchDown(ViveControllerModule.EventData eventData);
	}

	public interface ITouchpadTouchHandler : IEventSystemHandler {
		void OnTouchpadTouch(ViveControllerModule.EventData eventData);
	}

	public interface ITouchpadTouchUpHandler : IEventSystemHandler {
		void OnTouchpadTouchUp(ViveControllerModule.EventData eventData);
	}

	//TRIGGER HANDLER
	public interface ITriggerHandler : ITriggerPressSetHandler, ITriggerTouchSetHandler { }
	public interface ITriggerPressSetHandler : ITriggerPressDownHandler, ITriggerPressHandler, ITriggerPressUpHandler { }
	public interface ITriggerTouchSetHandler : ITriggerTouchDownHandler, ITriggerTouchHandler, ITriggerTouchUpHandler { }

	public interface ITriggerPressDownHandler : IEventSystemHandler {
		void OnTriggerPressDown(ViveControllerModule.EventData eventData);
	}

	public interface ITriggerPressHandler : IEventSystemHandler {
		void OnTriggerPress(ViveControllerModule.EventData eventData);
	}

	public interface ITriggerPressUpHandler : IEventSystemHandler {
		void OnTriggerPressUp(ViveControllerModule.EventData eventData);
	}

	public interface ITriggerTouchDownHandler : IEventSystemHandler {
		void OnTriggerTouchDown(ViveControllerModule.EventData eventData);
	}

	public interface ITriggerTouchHandler : IEventSystemHandler {
		void OnTriggerTouch(ViveControllerModule.EventData eventData);
	}

	public interface ITriggerTouchUpHandler : IEventSystemHandler {
		void OnTriggerTouchUp(ViveControllerModule.EventData eventData);
	}


	//GLOBAL VIVE HANDLER: ALL Global BUTTON SETS
	public interface IGlobalViveHandler : IGlobalGripHandler, IGlobalTriggerHandler, IGlobalApplicationMenuHandler, IGlobalTouchpadHandler { }

	/// GLOBAL GRIP HANDLER
	public interface IGlobalGripHandler : IGlobalGripPressDownHandler, IGlobalGripPressHandler, IGlobalGripPressUpHandler { }

	public interface IGlobalGripPressDownHandler : IEventSystemHandler {
		void OnGlobalGripPressDown(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalGripPressHandler : IEventSystemHandler {
		void OnGlobalGripPress(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalGripPressUpHandler : IEventSystemHandler {
		void OnGlobalGripPressUp(ViveControllerModule.EventData eventData);
	}


	//GLOBAL TRIGGER HANDLER
	public interface IGlobalTriggerHandler : IGlobalTriggerPressSetHandler, IGlobalTriggerTouchSetHandler { }
	public interface IGlobalTriggerPressSetHandler : IGlobalTriggerPressDownHandler, IGlobalTriggerPressHandler, IGlobalTriggerPressUpHandler { }
	public interface IGlobalTriggerTouchSetHandler : IGlobalTriggerTouchDownHandler, IGlobalTriggerTouchHandler, IGlobalTriggerTouchUpHandler { }

	public interface IGlobalTriggerPressDownHandler : IEventSystemHandler {
		void OnGlobalTriggerPressDown(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalTriggerPressHandler : IEventSystemHandler {
		void OnGlobalTriggerPress(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalTriggerPressUpHandler : IEventSystemHandler {
		void OnGlobalTriggerPressUp(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalTriggerTouchDownHandler : IEventSystemHandler {
		void OnGlobalTriggerTouchDown(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalTriggerTouchHandler : IEventSystemHandler {
		void OnGlobalTriggerTouch(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalTriggerTouchUpHandler : IEventSystemHandler {
		void OnGlobalTriggerTouchUp(ViveControllerModule.EventData eventData);
	}

	//GLOBAL APPLICATION MENU
	public interface IGlobalApplicationMenuHandler : IGlobalApplicationMenuPressDownHandler, IGlobalApplicationMenuPressHandler, IGlobalApplicationMenuPressUpHandler { }

	public interface IGlobalApplicationMenuPressDownHandler : IEventSystemHandler {
		void OnGlobalApplicationMenuPressDown(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalApplicationMenuPressHandler : IEventSystemHandler {
		void OnGlobalApplicationMenuPress(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalApplicationMenuPressUpHandler : IEventSystemHandler {
		void OnGlobalApplicationMenuPressUp(ViveControllerModule.EventData eventData);
	}

	//GLOBAL TOUCHPAD 
	public interface IGlobalTouchpadHandler : IGlobalTouchpadPressSetHandler, IGlobalTouchpadTouchSetHandler { }

	public interface IGlobalTouchpadPressSetHandler : IGlobalTouchpadPressDownHandler, IGlobalTouchpadPressHandler, IGlobalTouchpadPressUpHandler { }
	public interface IGlobalTouchpadTouchSetHandler : IGlobalTouchpadTouchDownHandler, IGlobalTouchpadTouchHandler, IGlobalTouchpadTouchUpHandler { }

	public interface IGlobalTouchpadPressDownHandler : IEventSystemHandler {
		void OnGlobalTouchpadPressDown(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalTouchpadPressHandler : IEventSystemHandler {
		void OnGlobalTouchpadPress(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalTouchpadPressUpHandler : IEventSystemHandler {
		void OnGlobalTouchpadPressUp(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalTouchpadTouchDownHandler : IEventSystemHandler {
		void OnGlobalTouchpadTouchDown(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalTouchpadTouchHandler : IEventSystemHandler {
		void OnGlobalTouchpadTouch(ViveControllerModule.EventData eventData);
	}

	public interface IGlobalTouchpadTouchUpHandler : IEventSystemHandler {
		void OnGlobalTouchpadTouchUp(ViveControllerModule.EventData eventData);
	}
}
#endif