The Holojam SDK enables content creators to build complex location-based multiplayer VR experiences in a simple, unified Unity project with no advanced networking skills necessary. Its C# API provides an extensible and clean interface, allowing for rapid prototyping and extension. Additionally, it abstracts away specific VR hardware, removing the need to design single-use, bespoke creations.

## Get Holojam

Download the latest stable release [here](https://github.com/holojamvr/HolojamSDK-Unity/releases/latest). In Unity, Go to _'Assets -> Package -> Custom Package...'_ and import Holojam into your project.

You can find all the releases (including the latest pre-release) [here](https://github.com/holojamvr/HolojamSDK-Unity/releases).

Note: at the moment, Holojam is pre-configured for SteamVR (Vive). **Make sure to install the [SteamVR plugin](https://www.assetstore.unity3d.com/en/#!/content/32647) in your project alongside Holojam.**

### Developer Version

For the latest code, clone the repository within an empty or existing Unity project under the `Assets` directory. **Before opening Unity**, copy `Holojam.Core.dll` to `Holojam/Plugins/`. In the Editor Settings, make sure to set 'Version Control Mode' to 'Visible Meta Files' and 'Asset Serialization Mode' to 'Force Text'.

## Documentation

- API documentation available [here](https://acgaudette.gitlab.io/holojamsdk-unity-docs/annotated.html).
- Check out the Vive setup tutorial on the [wiki](https://github.com/holojamvr/HolojamSDK-Unity/wiki/Basic-Setup-Tutorial-(Vive)).
- Example code can be found under [Holojam/Demo/Scripts/Examples](https://github.com/holojamvr/HolojamSDK-Unity/tree/master/Holojam/Demo/Scripts/Examples).
