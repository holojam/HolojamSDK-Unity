The Holojam Unity SDK provides a comprehensive–
* Operates alongside and on top of existing device-specific VR SDKs
* No need to use another networking solution–the Holojam network stack, while specialized for virtual reality, is very capable otherwise
* Scale up to as many users as you want in a **single** project

–accessible–
* Written from the ground up for Unity, with custom inspectors, prefabs, and components
* Example code and demo scene included
* No strict guidelines enforced: do it your own way

–and turnkey–
* Single Holojam object required in the scene
* Quickly add new Actors with our provided template and synchronize them with Holojam Nexus at the click of a button
* No need to learn complex networking

–solution for managing and developing for multiple VR users, regardless of the hardware (HMDs) being used. It allows developers to treat users generically without having to worry about custom setups per-player. Furthermore, it integrates with the Holojam network stack, enabling highly-flexible, fast, and low-latency communication. By pairing the Holojam messaging protocol with the adaptive Unity component, `Holojam.Controller`, content creators can:
* Define network objects (variable fields) on the fly
* Push dynamic (modifiable at runtime) events and notifications throughout the network
* Use cases include remote logging
* Synchronize data across multiple "headless" clients
* Synchronize game data across multiple clients and a virtual server

Building multiplayer content is all about effective communication. And the Holojam Unity SDK was created with communication in mind from the very beginning.
