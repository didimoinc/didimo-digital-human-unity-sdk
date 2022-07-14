# Didimo Unity SDK



The [Didimo](https://www.didimo.co/) SDK Networking package allows for immediate interaction with the Didimo API.

All you need to get up and running after install is included in this README.

To get started, extract the sample contained in this package, and use the sample code provided as a starting point.
Please don't forget to follow the [Setup Process Section](#setup-process) for properly configuring your project.

For a more detailed description of the Didimo SDK, please visit our [Github Repository](https://github.com/didimoinc/didimo-digital-human-unity-sdk).

---

## Pre-requisites

This SDK has been built to work with Unity 2021.3.x LTS (from Unity 2021.3.1 onwards).


## Setup Process

1. Setup your project. You can do this by going to Window → Didimo Manager, and following the instructions, or by following these steps 
   1. Go to Project Settings → Graphics, and select `UniversalRP-HighQuality` as the render pipeline asset. 
   2. Go to Project Settings → Quality, and select `UniversalRP-HighQuality` as the render pipeline asset, for your desired
      quality level.
   3. Go to Project Settings → Player → Other Settings → Rendering. Set the colour space to linear.
2. Setup your API Key. Go to Window → Didimo Manager → Networking, and follow the instructions.

---

# Already have a glTF importer?

For runtime loading of didimos, you can keep both importers without any issues. But for the **Unity Editor**, to import didimos directly into the project, Unity only allows one scripted importer per file extension. 
If you already have a glTF importer on your project, we will have a clash. To fix this, either:
* Delete your glTF library and use ours instead. Our glTF importer is based on [GLTFUtility](https://github.com/Siccity/GLTFUtility)
* If you need to keep your importer, you can add the script define symbol `USE_DIDIMO_CUSTOM_FILE_EXTENSION` to your project. This will make our importer register to the extension `.gltfd` instead. You will then be responsible to rename your didimo `.gltf` files into `.gltfd`. 
This will break our sample scenes, so if you need to evaluate the SDK first, we suggest you do it on a clean project.  

---

## Included

* Code Examples
* Set up tools
* Importing tools (edit/runtime)
* Integrations with other platforms such as AWS Polly and ARKit
* Animation tools
* Hair library and 'Fitting Service'
* Idle Animation Library

Be sure to check out the [Digital Human Specifications](https://link.didimo.co/39dkEH0), best practices and
the [SDK Guide](https://link.didimo.co/3tPAWPY) on our [Developer Portal](https://link.didimo.co/3Ckogna).


---

## Known Import Issues

* We cannot control the order with which Unity imports assets. If the .glTF files of your didimos get imported before any of
  its dependencies, then the didimo will fail to import.
* If you open the `MeetADidimo` scene and don't see any didimos, go to Window → Didimo Manager, and click the `Reimport didimos`
  button.

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