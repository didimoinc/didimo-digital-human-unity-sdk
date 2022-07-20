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