# VR Physics Hands
For use with Unity. These hands are currently only compatible with Oculus Quest's hand tracking. They do not support touch controllers or the knuckle controllers, or any other controllers for that matter. They are fully physics based, meaning they can interact with the world and the world can interact with them. Controlled entirely by forces and joints.

![Handshake](https://i.imgur.com/ZlHfaNx.gif)
![HoldHands](https://i.imgur.com/Qqazzy8.gif)
![PassItOn](https://i.imgur.com/PpUAO9o.gif)
![ThumbWar](https://i.imgur.com/VFPHRAa.gif)

# Requirements
To use this in your project, you'll first need to meet some requirements:
1. Install [UnityHelpers](https://github.com/oxters168/UnityHelpers)
1. Have [Oculus Integration](https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022) set up for Oculus Quest

# Installation
Now that the requirements have been met you can either:
- Clone this repository into your project
- Download the Unity package from the releases and import it like any other package
- Download directly from github and plug into your project (not recommended due to the [git lfs issue](https://github.com/git-lfs/git-lfs/issues/903))

# How to Use
To use these hands in your project, drag the left hand and right hand prefabs from the 'Prefabs' folder into your scene. Select each hand and give them an anchor to follow. If you're using the OVRCameraRig from the Oculus Integration asset, then the anchors would be the 'LeftControllerAnchor' and 'RightControllerAnchor' child transforms. You'll also need to set 'Hand Tracking Support' to the 'Hands Only' or 'Controllers and Hands' option on the OVRManager script.
