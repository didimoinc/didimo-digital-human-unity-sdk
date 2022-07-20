# Core Examples

Welcome to the Core Examples folder. Following from the [Didimo](https://www.didimo.co/) Unity SDK main README file,
this README will give insight and exhibit the examples present in this folder. The goal is to provide a way to
demonstrate the interactions with the digital humans and their inherent functionalities. More information in
the [Developer Portal](https://developer.didimo.co/docs).


## ExampleAnimationScripts

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

    1. Didimo GameObject Stats: (Found in `Packages/com.didimo.sdk.core/Runtime/Scripts/Utility/DidimoGameObjectStats.cs`) Used to obtain statistics related to the mesh of a didimo (or more) placed in a Unity
       scene. It provides the number of vertices and triangles.

    2. Scene Stats: (Found in `Packages/com.didimo.sdk.core/Runtime/Scripts/Stats/SceneStats.cs`) Used to provide a way to obtain the frames-per-second(fps) relative to a determined scene.

    3. Didimo Stats Example: Used to obtain performance metrics related to a didimo and the present scene. It uses the
       previous 2 scripts to obtain numbers on a didimo's mesh, implementing a button to show all active and inactive
       meshes, as well as the scene's fps.