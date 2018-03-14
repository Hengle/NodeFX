# Authoring Particle Systems in Unity through Houdini


![nodefx_icon](Unity/Assets/Plugins/NodeFX/Resources/nodefx_logo.png)

An engineer at work posed me the question "What would your ideal effect editor look look like?", and no matter how I tried to formulate myself, the answer kept just being a variation of "I want it to be like Houdini". But the more I thought about it, the more I realized that getting a realtime VFX particle system running using Houdini might not be that difficult after all! After a lot of experimenting back and forth, I managed to create a tool that allows you to author particle systems in Unity through Houdini.

## What it does

Using the NodeFX asset in Houdini, you can use a VOP network to create an _effect definition_, which is parsed into a particle system by the Unity importer script. I've added a number of Unity-specific nodes to create random values, gradients, curves, etc., to retain the roughly same level of features that the Shuriken interface provides.

My goal with this tool was to use the powerful node interface of Houdini -- with all its capabilities of grouping nodes, linking properties, and writing expressions -- to cut down iteration times when authoring particle systems in Unity. And while I realized that it’d never be as transparent as using the in-editor tools, having a workflow that was as invisible and responsive as possible was a priority.

Please do note that this tool does not introduce any new features or allow Unity to do stuff it couldn’t do before - it is not an extension of the particle system, but rather a creative replacement for its interface. <sub>*Of course, one might argue that artist productivity is the most important feature of all, and I personally feel it does a lot to aid in that department.*</sub>

## Getting Started

*Please note that the software is not at a production-ready stage, and is provided as-is. I strongly advise against integrating this tool into your existing workflow at this time.*

## **[Download](https://github.com/ChrisJ3D/NodeFX/archive/master.zip)**

This tool was created using the following software versions, and compatibility is not guaranteed with newer or older versions:

* `Unity 2018.1.0b8`
* `Houdini 16.5.323`

Start by extracting the contents of the archive to disk. To explore the Houdini side of the setup, copy the contents of the `Houdini/houdini_config_dir` folder to the Houdini $HOME folder on your disk. On Windows this is usually `[User]/Documents/Houdini16.x`, and on macOS/Linux you can normally find it in `~/Houdini16.x`. After copying the contents of the folder, simply open Houdini, enter the Geometry context, and put down a NodeFX node. In the VOP context you'll find a number of Unity-specific nodes under the _Unity_ tools category.  I've included an example network that you can find in `Houdini/hip/CampFire_Example.hip`.

To bring an effect definition into Unity, open the Unity folder from the Unity launcher, and then simply open the GameObject menu and browse to Effects -> NodeFX System. The resulting GameObject allows you to assign effect definitions created from the NodeFX asset in Houdini.

## Usage

* Your first step should simply be to export a default definition from Houdini, and load that into Unity using a NodeFX GameObject (instructions above).
* To make iterating as quick as possible, I highly recommend you have Unity running in Play Mode, so that it can look for updates even when out of focus
* To quickly save the defitinion from within Houdini, just have one of your VOP nodes selected and press CTRL-ALT-S, and on macOS, CMD-ALT-S.

### Shorthand Notations

* To quickly have a value be linearly interpolated without having to use a UnityRampToCurve node, simply enter `[Start Value] ~ [End Value]`. You can enter as many "keys" as you'd like, although I would consider it somewhat bad practice since it quickly becomes difficult to read.
* `0~1` creates a linear curve from 0 to 1.
* `0~1~0` creates a linear curve from 0 to 1, then back down to 0.
* `2~-1~0~-1` creates a linear curve from 2 to -1 to 0 to -1.
* To quickly have a value be randomized between a Min and Max value without having to use a UnityFloatToConstantRange node, simply enter `[MinValue] ; [MaxValue]`. This notation only takes two values. It's perfectly functional to enter a larger number first and a lesser number second, but I would recommend you to keep it consistent.
* `0;1` will return a random value between 0 and 1
* `20:-.23` will return a random value between 20 and -0.23.

Misc
----

* Unity will automatically name the particle system according to what you name your NodeFX node, and each emitter will inherit the name of whatever name its EmitterModule has.

Stuff to come
---

The focus so far has mainly been figuring out the most straight-forward approach for getting this kind of data from Houdini to Unity. For the next revision I’d like to abstract it a bit more, and move the Houdini-side of things closer to what I’d feel would be the most intuitive way to work with particle systems. As long as the output can be interpreted by Unity, it doesn't matter what the input looks like. So I'm looking to get creative!

Other neat things I’m looking to explore with this tool are:

* Hook geometry into the network and have that automatically be exported as a mesh particle or mesh emitter
* Add the ability to share modules between emitters, e.g. have two emitters use the same Shape module
* Create more nodes and inputs that communicate with the particle shader
* Add shelf tools that help with versioning and organization of your effect library

---

To summarize, I was extremely surprised at how easy it was to get this idea up and running, especially considering that particle systems aren’t exactly known for their portability. Houdini and Unity both provide fantastic amounts of freedom and flexibility, so I’m eager to take this further and really turn it into my own ideal particle authoring tool. I hope this post inspired you to think about how you best feel our work should be authored, and how we might get our tools closer to that point. Thank you for reading!