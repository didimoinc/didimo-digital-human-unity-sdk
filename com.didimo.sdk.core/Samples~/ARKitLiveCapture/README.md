# Core Examples

Welcome to the Core Examples folder. Following from the [Didimo](https://www.didimo.co/) Unity SDK main README file,
this README will give insight and exhibit the examples present in this folder. The goal is to provide a way to
demonstrate the interactions with the digital humans and their inherent functionalities. More information in
the [Developer Portal](https://developer.didimo.co/docs).


## ARKitLiveCapture

This Example was created in order to provide a way to record ARKit driven MoCap animations, and rapidly experiencing
them on a didimo, right after recording. Please, feel free to visit our Developer Portal
page [Integration: ARKit Face Capture](https://developer.didimo.co/docs/integration-arkit-face-capture) to learn, step
by step, on how to integrate this functionality.

The scene greets you with a didimo, and after the necessary setup, you can easily see the didimo being animated using
your expressions and you can record the animations at any time, using the TakeRecorder GameObject in the hierarchy. The
animations will be saved for later use inside your Unity project.

Pay special attention to the component `Didimo LiveCapture Mapper`. This script has functions implemented to handle the recording of a live capture using
Apple's ARKit Face Capture.