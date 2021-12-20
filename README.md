# Didimo Unity SDK

[Didimo](https://www.didimo.co/) provides a cloud-based automated service that enables the generation of digital humans
from a simple selfie, we call these didimos.

Utilising this code library and tools, you can easily integrate digital humans into your existing software and enable
your users to easily generate and load digital doubles of themselves directly into your experience, all at runtime.

All you need to get up and running after download is included in this README.

---

Following your decision to download this repo - our goal is to make it as easy and smooth as possible to get you
building software. Your didimos can be imported into the project by simple drag-and-drop at edit time, or they can be
loaded into the scene at runtime. No matter which approach you need, we've got you covered.

## Pre-requisites

This SDK has been built to work with Unity 2020.3.x LTS (from Unity 2020.3.12 onwards).


## Setup Process

1. This project MUST be placed at `Assets/Didimo`, in your Unity project. The didimos will not 
render if you don't do this.


2. The Newtonsoft Json package is required. We recommend adding it this way:
     
   2.1 Open the Package Manager
   
   2.2 Click the `+` button, select `Add package from git URL...` 
   
   2.3 Enter `com.unity.nuget.newtonsoft-json` and press add.


3. Install the package [Universal RP](https://link.didimo.co/3lw3NF5) through Unity's Package Manager and follow
instructions in Configuration section

4. Restart Unity

5. Go to Project Settings → Graphics, and select `UniversalRP-HighQuality` as the render pipeline asset.

6. Go to Project Settings → Quality, and select `UniversalRP-HighQuality` as the render pipeline asset, for your desired
   quality level.

7. Go to Project Settings → Player → Other Settings → Rendering. Set the colour space to linear.

8. Create an empty `csc.rsp` file in the `Assets` folder, with the following contents: to add the compression assemblies required
   when unzipping a didimo package:

```
-r:System.IO.Compression.dll
-r:System.IO.Compression.FileSystem.dll
```

9. Open the `MeetADidimo` scene.

10. TextMeshPro will be automatically installed by Unity at this point if it wasn't already, by prompting you to
   Import TMP Essentials, which is required for the examples.

11. You should now be able to press play and see the included talking didimos.
   **If you do not**, then please **regenerate didimos**
   See [Known Import Issues](#Known-Import-Issues) and [Support](#Support) for further assistance.

12. Its now possible to generate a new didimo via the didimo API directly from Unity.
   Simply [Create an account](https://developer.didimo.co/docs/creating-your-account)
   Then see [Generating a didimo](https://developer.didimo.co/docs/creating-a-didimo).


---

# SDK Contents

This project contains a single folder, the `Didimo` folder. Within it, there are the following modules:

* **Core** - Everything core to the SDK, including loading didimos where it handles animations, materials, speech, etc.
* **Mobile** - Add the ability of bi-directional communication between Unity and native Android or iOS applications.
* **Networking** - Allows for immediate interaction with the Didimo API.
* **Oculus** - Example integration with Oculus quest.
* **submodules** - Contains the GLTFUtility repository, that allows for loading gltf files.

Every module may contain an `Examples` folder, where example assets and scenes of said module can be found. To reduce
clutter, any `Examples` folder can be removed. Any module other than `Core` and `submodules` can also be deleted.



---

## Included

* Code Examples
* Set up tools / authentication
* Importing tools (edit/runtime)
* Cloud API Tools
* Integrations with other platforms such as AWS Polly and ARKit
* Animation tools
* Hair library and 'Fitting Service'
* Idle Animation Library

Be sure to check out the [Digital Human Specifications](https://link.didimo.co/39dkEH0), best practices and
the [SDK Guide](https://link.didimo.co/3tPAWPY) on our [Developer Portal](https://link.didimo.co/3Ckogna).


---

## Oculus Integration


1. If you wish to build for Oculus, install the package [Oculus Integration](https://link.didimo.co/3tJLcJs)
   
   1.1 Add the `Oculus Integration` package to your account through Unity's Asset Store
   
   1.2 Download and install the `Oculus Integration` package, through Unity's Package Manager. It should be listed
   
   under `Packages: My Assets`. Refresh the Package Manager if required
   
   1.3 Follow the instructions to update Oculus and restart Unity

You may be asked to update a number of Oculus related tools and restart Unity BEFORE the plugin will be installed. If
you restart - check to see if the plugin still needs to be installed. A successful install will result in an `Oculus`
folder in the Assets folder.

_Didimo has tested this against version 32.0 which was published on the 30th August 2021._


2. [Continue with setting up the SDK](#Setup-Process).

---

## ARKit Integration

1. **If you wish to capture and record face movements using ARKit**, install the package [Live Capture](https://link.didimo.co/3ABEI1G) from Unity.
   
   2.1 Open Unity's Package Manager
   
   2.2 Under the `+` button, select `Add package from git URL...`
   
   2.3 Enter `com.unity.live-capture@1.0.1` and press `Add`

Additionally, you will need the companion app [Unity Face Capture](https://apple.co/3nXoGfl) installed on an iPhone or iPad that supports ARKit face tracking.

_**Warning:**
This package is marked as preview and therefore the installation process may be subject to changes._

2. [Continue with setting up the SDK](#Setup-Process).

---

## Known Import Issues

* We cannot control the order with which Unity imports assets. If the .glTF files of your didimos get imported before any of
  its dependencies, then the didimo will fail to import.
* If you open the `MeetADidimo` scene and don't see any didimos, go to Didimo → Didimo Manager, and click the `Reimport didimos`
  button.
* The SDK folders `Core`, `Networking`, etc., must be in `/Assets/Didimo/`, otherwise the didimos will fail to render.

---

# Further Documentation

Further detail is explained in the [Developer Portal](https://link.didimo.co/3Ckogna) - "Getting Started" docs related
to software creation are:

* [Best Practices](https://link.didimo.co/3nE5cfj)
* [Unity SDK](https://link.didimo.co/3tPAWPY)
* [Accessory Fitting Service](https://link.didimo.co/3nzssv8)
* [Cloud API](https://link.didimo.co/39aNgAL)

---

# Contributing and Reporting bugs

Thank you for contributing and trying our product!
For bug reports, use github's issue tracker.

For pull requests:

1. Fork the repository to your GitHub account
2. Clone the repository to your machine
3. Create a new branch with a short descriptive name
4. Commit your code changes
5. Open a pull request, with a detailed description of your changes
6. We will test your changes internally, and if everything goes well, we will include them in our next release

---

# License

This SDK uses the [Didimo Source Code License](https://link.didimo.co/3hDyTcW). Read
our [Privacy](https://link.didimo.co/3AiXniS) page for more information on our license and privacy policies.

---

# Support


Detailed documentation can be read on our [Developer Portal](https://link.didimo.co/3Ckogna) and 

* Feature Request: [featurerequest@didimo.co](mailto:featurerequest@didimo.co)
* Technical Support: [support@didimo.co](mailto:support@didimo.co)
* Service Uptime Checker: https://status.didimo.co/
