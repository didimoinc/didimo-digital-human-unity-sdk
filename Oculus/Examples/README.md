# Oculus Examples

We have created an example Oculus integration to help you get up and running builing didimos into your Oculus experiences as quick as possible. 

For more information on how we've built didimos to be compatible with Oculus lipsync - check our the didimo specification on our developer portal - [built for Oculus LipSync](https://developer.didimo.co/docs/built-for-oculus-quest) 

---

## Pre-requisites
* You have followed the root [README](https://github.com/didimoinc/didimo-digital-human-unity-sdk) and installed the SDK
* You have followed the root [README](https://github.com/didimoinc/didimo-digital-human-unity-sdk) and intalled the Oculus integration

## Scenes

In the `Scenes` folder you will find the `OculusTestApplication` scene, which lets a user experience a didimo in VR, as
well as its features, while using the Oculus Quest. It has several "modes" that change by pressing a button. The modes
are:

* **Oculus Scene Lip Sync** While the user speaks and moves its head, using the VR Oculus headset, it animates the didimo's
  mouth and head accordingly, in real-time.

* **Oculus Scene Face Animation** The didimo is animated with a JSON mocap animation text file.

* **Oculus Scene Distance Viewer** The didimo moves away from the camera, so the user can see how it looks from any
  distance.

The scene also has a GameObject called `Application Manager` that has the necessary components for the Oculus Lip Sync
mode to work and has 2 additional scripts:

* **Oculus Test Application Manager** controls in which mode we are.

* **Oculus Application Manager** Used to choose the target FPS value we want the application to run at. It runs at 90 FPS by
  default.
