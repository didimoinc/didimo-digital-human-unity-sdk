# Core Examples

Welcome to the Core Examples folder. Following from the [Didimo](https://www.didimo.co/) Unity SDK main README file,
this README will give insight and exhibit the examples present in this folder. The goal is to provide a way to
demonstrate the interactions with the digital humans and their inherent functionalities. More information in
the [Developer Portal](https://developer.didimo.co/docs).

---

## Microsoft Azure Text To Speech Integration

Integration with Microsoft Azure Text To Speech. Quickly and easily play animations (or save them locally) from a text source.

Find the AzureTTSManager in the scene's Hierarchy.

If you wish to generate files to use later, use the Azure TTS Creation component: specify your API Connection Details and File information and choose your desired creation mode. If you choose the String Mode, write the text you want to see animated and choose the voice you want to use from Azure's catalog. The SSML Mode lets you synthesize your speech in a more detailed way, just include the SSML file location. Visit [Microsoft Azure Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/speech-synthesis-markup?tabs=csharp) to learn your possibilities with SSML.
To generate the files, press "Create TTS Files" button in the Inspector.

If you just want to playback the animations you just created or want to use the Stream Mode to playback animations without saving them locally, use the Azure TTS Playback component: once again specify your API Connection Details and drag your Audio Source and didimo you want to use. Choose your prefered Playback Mode, either from files or Stream Mode. If you wish to use Stream Mode, once more specify the String or SSML Mode details. 
Enter Playmode and press "Playback TTS" to view the result. 

If you need further guidance, please visit our [Developer Portal](https://developer.didimo.co/docs/integration-microsoft-azure-text-to-speech).