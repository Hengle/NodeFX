Authoring Particle Systems in Unity through Houdini
===

![nodefx_icon](Unity/Assets/Plugins/NodeFX/Resources/nodefx_logo.png)

Hey everyone, I wanted to share the results of an experiment I had the idea for a while back! 

An engineer at work posed me the question "What would your ideal effect editor look look like?", and no matter how I tried to formulate myself, the answer kept just being a variation of "I want it to be like Houdini". But the more I thought about it, the more I realized that getting a realtime VFX particle system running using Houdini might not be that difficult after all!

Getting Started
---

*Please note that the software is provided as-is, and is not at a production-ready stage. I currently strongly advise against integrating this tool into your existing workflow.*

**[Download](https://github.com/ChrisJ3D/NodeFX/archive/master.zip)**



After downloading from the link above, extract the contents into a folder of your choosing. Start by copying the contents of `Houdini/houdini_config_dir` to your Houdini $HOME directory, which on Windows usually is `[User]/Documents/Houdini16.X` and on macOS/Linux normally is located at `~/Houdini16.X`. You should now have access to the NodeFX and Unity nodes from within Houdini.

To explore the Houdini side of the setup, open `Houdini/hip/Campfire_Example.hip`. I've added notes to the network that will hopefully help explain how it works.

To bring an asset into Unity, open the Unity folder using the Unity launcher. Go to the GameObject menu, and under _Effects_ you'll find the _NodeFX System_ entry. Simply assign an effect definition that you've saved out from Houdini, and you should be good to go.

This tool was created using the following software versions, and compatibility is not guaranteed with newer or older versions:

* `Unity 2018.1.0b8`
* `Houdini 16.5.323`

What it does
---

Using the NodeFX asset, you can author a VOP network that is parsed into a particle system by a Unity importer script. I've added a number of Unity-specific nodes to create random values, gradients, curves, etc., to retain the roughly same level of features that the Shuriken interface provides.

My goal with this tool was to use the powerful node interface of Houdini -- with all its capabilities of grouping nodes, linking properties, and writing expressions -- to cut down iteration times when authoring particle systems in Unity. And while I realized that it’d never be as transparent as using the in-editor tools, having a workflow that was invisible and responsive was a priority.

Please do note that this tool does not introduce any new features or allow Unity to do stuff it couldn’t do before - it is not an extension of the particle system, but rather a creative replacement for its interface. <sub>*Of course, one might argue that artist productivity is the most important feature of all, and I personally feel it does a lot to aid in that department.*</sub>

Stuff to come
---

The focus so far has mainly been figuring out the most straight-forward approach for getting this kind of data from Houdini to Unity. For the next revision I’d like to abstract it a bit more, and move the Houdini-side of things closer to what I’d feel would be the most intuitive way to work with particle systems. As long as the output can be interpreted by Unity, it doesn't matter what the input looks like. So I'm looking to get creative!

Other neat things I’m looking to explore with this tool are:

* Hook geometry into the network and have that automatically be exported as a mesh particle or mesh emitter
* Add the ability to share modules between emitters, i.e. have two emitters use the same Shape module
* Create more nodes and inputs that communicate with the particle shader
* Add shelf tools that help organize your effect library

---

To summarize, I was extremely surprised at how easy it was to get this idea up and running, especially considering that particle systems aren’t exactly known for their portability. Houdini and Unity both provide fantastic amounts of freedom and flexibility, so I’m eager to take this further and really turn it into my own ideal particle authoring tool. I hope this post inspired you to think about how you best feel our work should be authored, and how we might get our tools closer to that point. Thanks for reading!