# Didimo Unity SDK

[Didimo](https://www.didimo.co/) provides a cloud-based automated service that enables the generation of digital humans
from a simple selfie, we call these didimos.

Utilising this code library and tools, you can easily integrate digital humans into your existing software and enable
your users to easily generate and load digital doubles of themselves directly into your experience, all at runtime.

Detailed documentation can be read on our [Developer Portal](https://link.didimo.co/3Ckogna) and all you need to get up
and running after download is included in this README.

---

Following your decision to download this repo - our goal is to make it as easy and smooth as possible to get you
building software. Your didimos can be imported into the project by simple drag-and-drop at edit time, or they can be
loaded into the scene at runtime. No matter which approach you need, we've got you covered.

## Key functionality

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

## Pre-requisites

1. This SDK has been built to work with Unity 2020.3 LTS
2. We strongly encourage the use of Unity Hub, where the correct unity version and build modules can be installed.
3. If you want to use our API with your own account, generate an API key in our [Customer Portal](HTTP://app.didimo.co)

---

# Installation

This repository, although not being a Unity Project, can be copied straight into an existing Unity Project. This can be
done in any way, such as downloading the project's zip and extracting to your Unity Project, or through a git submodule,
git subtree, etc. The only requirement is that it is placed directly under the `Assets` folder.

### Dependencies

For the project to compile, please follow these instructions to resolve dependencies

### Optional

1. **If you wish to build for Oculus**, install the package [Oculus Integration](https://link.didimo.co/3tJLcJs)
   
   1.1 Add the `Oculus Integration` package to your account through Unity's Asset Store
   
   1.2 Download and install the `Oculus Integration` package, through Unity's Package Manager. It should be listed
   
   under `Packages: My Assets`. Refresh the Package Manager if required
   
   1.3 Follow the instructions to update Oculus and restart Unity

You may be asked to update a number of Oculus related tools and restart Unity BEFORE the plugin will be installed. If
you restart - check to see if the plugin still needs to be installed. A successful install will result in an `Oculus`
folder in the Assets folder.

_**Warning:**
If you are not developing for Oculus and don't want to install the package, you will need to remove the `Oculus` module,
otherwise you will get compilation errors._

_Didimo has tested this against version 32.0 which was published on the 30th August 2021._

2. **If you wish to capture and record face movements using ARKit**, install the package [Live Capture](https://link.didimo.co/3ABEI1G) from Unity.
   
   2.1 Open Unity's Package Manager
   
   2.2 Under the `+` button, select `Add package from git URL...`
   
   2.3 Enter `com.unity.live-capture@1.0.1` and press `Add`

Additionally, you will need the companion app [Unity Face Capture](https://apple.co/3nXoGfl) installed on an iPhone or iPad that supports ARKit face tracking.

_**Warning:**
This package is marked as preview and therefore the installation process may be subject to changes._


### Required

1. If at this point you have compiler errors, you need to install the Newtonsoft Json package. 
     
   1.1 Open the Package Manager
   
   1.2 Click the `+` button, select `Add package from git URL...` 
   
   1.3 Enter `com.unity.nuget.newtonsoft-json` and press add


2. Install the package [Universal RP](https://link.didimo.co/3lw3NF5) through Unity's Package Manager and follow
instructions in Configuration section

3. TextMeshPro will be automatically installed by Unity. Unity may prompt you to "Import TMP Essentials". If that happens,
please comply.

## Configuration

1. Go to Project Settings → Graphics, and select `UniversalRP-HighQuality` as the render pipeline asset.
2. Go to Project Settings → Quality, and select `UniversalRP-HighQuality` as the render pipeline asset, for your desired
   quality level.
3. Go to Project Settings → Player → Other Settings → Rendering. Set color space to linear
4. Add a `csc.rsp` file to the Assets' folder, with the following contents, to add the compression assemblies required
   when unzipping a didimo package:

```
-r:System.IO.Compression.dll
-r:System.IO.Compression.FileSystem.dll
```

---

# Code Modules

This project contains a single folder, the `Didimo` folder. Within it, there are the following modules:

* **Core** - Everything core to the SDK, including loading didimos where it handles animations, materials, speech, etc.
* **Networking** - Allows for immediate interaction with the Didimo API.
* **Oculus** - Example integration with Oculus quest.
* **submodules** - Contains the GLTFUtility repository, that allows for loading gltf files.

Every module may contain an `Examples` folder, where example assets and scenes of said module can be found. To reduce
clutter, any `Examples` folder can be removed. Any module other than `Core` and `submodules` can also be deleted.

---

# Getting Started

## Explore the Examples

Head to the examples to "Meet a didimo". You can press play immediately to hear a didimo tell you a bit about itself and
how it's being animated.

## Quick Start

Further detail is explained in the [Developer Portal](https://link.didimo.co/3Ckogna) - "Getting Started" docs related
to software creation are:

* [Best Practices](https://link.didimo.co/3nE5cfj)
* [Unity SDK](https://link.didimo.co/3tPAWPY)
* [Accessory Fitting Service](https://link.didimo.co/3nzssv8)
* [Cloud API](https://link.didimo.co/39aNgAL)

## Creating Your first Digital Human from Unity

1. Sign up for an account in the Customer Portal
2. Create yourself a developer key
3. Follow the instructions above to get the SDK installed
4. After SDK installation - Use the Unity Didimo Manager to add the developer key
5. Follow the instructions

---

# Contributing and Reporting bugs

Thank you for contributing and trying our product!
For bug reports, use github's issue tracker.

For pull request:

1. Clone the repository and make a new branch
2. Commit your code changes
3. Open a pull request, with a detailed description of your changes
4. We will test your changes internally, and if everything goes well, we will include them in our next release

---

# License

This SDK uses the [Didimo Source Code License](https://link.didimo.co/3hDyTcW). Read
our [Privacy](https://link.didimo.co/3AiXniS) page for more information on our license and privacy policies.

---

# Support

* Feature Request: [featurerequest@didimo.co](mailto:featurerequest@didimo.co)
* Technical Support: [support@didimo.co](mailto:support@didimo.co)
* Service Uptime Checker: https://status.didimo.co/

# Known issues

* On Unity we cannot control when assets get imported. If the .glTF files of your didimos get imported before any of
  its dependencies (e.g. textures), then the didimo will fail to import. If you open the `MeetADidimo` scene and don't
  see a didimo, search your project for assets named `avatar`, right click them and select reimport.