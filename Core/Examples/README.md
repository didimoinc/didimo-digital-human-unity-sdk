# Core Examples

Welcome to the Core Examples folder. Following from the [Didimo](https://www.didimo.co/) Unity SDK main README file,
this README will give insight and exhibit the examples present in this folder. The goal is to provide a way to
demonstrate the interactions with the digital humans and their inherent functionalities. More information in
the [Developer Portal](https://developer.didimo.co/docs).

---

## MeetADidimo

This example consists in a scene where two didimos automatically demonstrate their built-in functionalities. It can be
accessed either by searching the Examples folder inside Unity, or from
the [Didimo Manager](https://developer.didimo.co/docs/didimo-manager).

Entering the scene in Play Mode you will be greeted with two didimos, each powered by their respective speech
functionality:

* The first didimo's speech is produced using Apple ARKit Face Capture, which uses a real person's voice and movement to
  animate the didimo. More information on how to use this technology on our Developer Portal
  page [Integration: ARKit Face Capture](https://developer.didimo.co/docs/integration-arkit-face-capture).

* The second didimo uses Amazon's Polly Text-To-Speech. Polly is a service which, provided a written text with the
  intended speech, returns an audio file of the speech and a JSON file with the respective animations for the mouth
  poses (visemes), to be used on the didimo. More on how to implement it in our Developer Portal
  page [Integration: Text-To-Speech](https://developer.didimo.co/docs/integration-text-to-speech).

* Both didimos, besides using the speech related movements, also have implemented idle animations, which provide a
  degree of realism to our digital humans. More on how to implement facial animations in
  the [Developer Portal - Facial Animations](https://developer.didimo.co/docs/facial-animations).

---

## IdleAnimation

This Example folder is used to give the user means of experiencing the Didimo Animation system using a didimo hooked up
with the various animation components, to demonstrate how the idle animations interact with other expressions by playing
them sequentially. More on facial expression on our Developer Portal
page [Facial Animations](https://developer.didimo.co/docs/facial-animations).

* Mocap Idle Animations: We provide 3 different animations for idling, each producing mostly head and eye movements, ideal to
  integrate with speech functionalities.

* Expressions: Six different expressions for emotions are provided.

---

## ARKitLiveCapture

This Example was created in order to provide a way to record ARKit driven MoCap animations, and rapidly experiencing
them on a didimo, right after recording. Please, feel free to visit our Developer Portal
page [Integration: ARKit Face Capture](https://developer.didimo.co/docs/integration-arkit-face-capture) to learn, step
by step, on how to integrate this functionality.

The scene greets you with a didimo, and after the necessary setup, you can easily see the didimo being animated using
your expressions and you can record the animations at any time, using the TakeRecorder GameObject in the hierarchy. The
animations will be saved for later use inside your Unity project.

---

## DidimoInspector

With this scene, after entering play mode, you will be able to inspect a didimo and some metrics such as number of
meshes, vertices, triangles, and performance (FPS).

---

## LookatTarget

This example shows how the head and eyes can track an object.

The setup of this scene is as follows:

* We've applied the **LookAtConstraint** Unity component to the **Head**, **LeftEye** and **RightEye** GameObjects and set the target to
  follow a sphere. The target is then animated around to demonstrate in Play Mode how this looks.
* We add further realism by enabling MoCap animation, as captured with ARKit, to add blinking and some subtle facial
  expressions. To trigger the animation, select the **avatar** GameObject, and click "Play Selected Mocap Animation", on the inspector.

---

## Scripts

This folder provides examples of scripts that can be used to control or implement various features.

* Scripts related with animations (can be attached to a didimo GameObject and used to control animations):

    1. Didimo Animation Example Expressions: It lists all the available expressions and has implemented functions to
       play these expressions and also stop them, using Editor buttons.

    2. Didimo Animation Example Mocap: Given a mocap animation file, this script plays and stops them using Editor
       buttons.

    3. Didimo Animation Example Poses: Again, through the use of buttons, the script sets and resets a given set of
       poses.

    4. Didimo Animation Example TTS: Given an audio clip and respective TTS data, this script implements a button to
       make a didimo play the selected TTS audio and associated animations.


* Scripts related with statistics:

    1. Didimo GameObject Stats: Used to obtain statistics related to the mesh of a didimo (or more) placed in a Unity
       scene. It provides the number of vertices and triangles.

    2. Scene Stats: Used to provide a way to obtain the frames-per-second(fps) relative to a determined scene.

    3. Didimo Stats Example: Used to obtain performance metrics related to a didimo and the present scene. It uses the
       previous 2 scripts to obtain numbers on a didimo's mesh, implementing a button to show all active and inactive
       meshes, as well as the scene's fps.

* Other Scripts:

    1. Didimo LiveCapture Mapper: This script has functions implemented to handle the recording of a live capture using
       Apple's ARKit Face Capture.

---

## Contents

This Example folder essentially contains three gLTF generic didimos, used on all the examples.
  