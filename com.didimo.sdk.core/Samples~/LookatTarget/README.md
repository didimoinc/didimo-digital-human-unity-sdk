# Core Examples

Welcome to the Core Examples folder. Following from the [Didimo](https://www.didimo.co/) Unity SDK main README file,
this README will give insight and exhibit the examples present in this folder. The goal is to provide a way to
demonstrate the interactions with the digital humans and their inherent functionalities. More information in
the [Developer Portal](https://developer.didimo.co/docs).

---

## LookatTarget

This example shows how the head and eyes can track an object.

The setup of this scene is as follows:

* We've applied the **LookAtConstraint** Unity component to the **Head**, **LeftEye** and **RightEye** GameObjects and set the target to
  follow a sphere. The target is then animated around to demonstrate in Play Mode how this looks.
* We add further realism by enabling MoCap animation, as captured with ARKit, to add blinking and some subtle facial
  expressions. To trigger the animation, select the **avatar** GameObject, and click "Play Selected Mocap Animation", on the inspector.