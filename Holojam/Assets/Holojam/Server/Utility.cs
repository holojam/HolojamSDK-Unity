//Utility.cs
//Created by Aaron C Gaudette on 27.07.16

using UnityEngine;

namespace Holojam{
	class Utility{
		public static bool IsMasterPC(){
			switch(Application.platform){
				case RuntimePlatform.OSXEditor: return true;
				case RuntimePlatform.OSXPlayer: return true;
				case RuntimePlatform.WindowsEditor: return true;
				case RuntimePlatform.WindowsPlayer: return true;
				case RuntimePlatform.LinuxPlayer: return true;
			}
			return false;
		}
	}
}