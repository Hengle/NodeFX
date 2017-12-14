Authoring Particle Systems in Unity through Houdini Engine
===

Hey everyone, I wanted to share the results of an experiment I had the idea for a while back! 

An engineer at work posed me the question "What would your ideal effect editor look look like?", and no matter how I tried to formulate myself, the answer kept just being a variation of "I want it to be like Houdini". But the more I thought about it, the more I realized that getting a realtime VFX particle system running using Houdini might not be that difficult after all!

 Despite being both a game developer and a big Houdini enthusiast, I never tinkered too much with Houdini Engine (a plugin that bridges Houdini with other DACs such as Unity and Unreal). Most of the materials you’ll find position it as a level art tool, but I had an idea that involved using it to author particle systems using just nodes in Houdini! This is certainly not the type of situation Engine is built to expect, so I ran in to some awkwardness along the way, but I am very excited by the results none the less!

What it does
===
Using the VOPEmitter asset, you can author a VOP network that is parsed into a particle system by a Unity importer script. I've added a number of Unity-specific nodes to create random values, gradients, curves, etc., to retain the roughly same level of features.

My goal with this tool was to use the powerful node interface of Houdini, with all its capabilities of grouping nodes, linking properties, and writing expressions, to cut down iteration times when authoring particle systems in Unity. And while I realized that it’d never be as transparent as using the in-editor tools, I figured the more seamless, the better!

Please do note that this tool does not introduce any new features or allow Unity to do stuff it couldn’t do before - it is not an extension of the particle system, but rather a creative replacement for its interface. <sub>*Of course, one might argue that artist productivity is the most important feature of all, and I personally feel it does a lot to aid in that department.*</sub>

How it works
===
The Houdini Engine plugin and asset workflow made it almost trivial to get a functional version up and running in Unity, only requiring a simple C# script to assign particle system attributes to those found in the Houdini asset. The bulk of the work was essentially copying and pasting every parameter that needed implementing.

The relationship between what is authored in Houdini and what gets translated to Unity can be described as follows:

1. `User authors a VOP network in Houdini ↴`
2. `Unity-specific parameters are stored as point attributes ↴`
3. `Top-level HDA parameters assigns types to these attributes and exposes them ↴`
4. `Houdini Engine brings the asset to Unity ↴`
5. `Parser script in Unity interprets the parameters and creates gradients or curves as needed ↴`
6. `Particle System is created and is beautiful!`

Unity allows most parameters to be controlled either by a constant value, a random value with a customizable range, or a curve. Retaining that flexibility introduced some complexity, which I solved by converting each value to a string before exporting. The string was comprised of elements that described what kind of value it represented, which the parsing script in Unity could interpret and dynamically construct a curve from. It didn't feel ideal since it added some clutter to the node network, but thankfully it helped prevent losing features. It also added the ability of easily reusing the same curve for different parameters, something which is clunky to do natively in Unity.

To begin with I chose to store the parameters as detail attributes in Houdini, but this proved problematic when I began implementing support for several emitters in the same network. Thankfully it was relatively easy to rework the system to use point attributes instead. Every module node in Houdini is assigned an ID that helps it keep track of what emitter it belongs to.

There was also some awkwardness around the fact that Engine assumes the asset definition to be static and only reactive to edits done through Unity, while this tool assumed the exact opposite. Callback scripts in Houdini were life savers, as they allowed me to automatically save the asset definition as soon as a value was changed.

Stuff to come
===
The focus so far has mainly been figuring out the most straight-forward approach for getting this kind of data from Houdini to Unity. For the next revision I’d like to abstract it a bit more, and move the Houdini-side of things closer to what I’d feel would be the most intuitive way to work with particle systems. As long as the output can be interpreted by Unity, it doesn't matter what the input looks like. So I'm looking to get creative!

Other neat things I’m looking to explore with this tool are:
* Hook geometry into the network and have that automatically be exported as a mesh particle or mesh emitter
* Add the ability to share modules between emitters, i.e. have two emitters use the same Shape module
* Create more nodes and inputs that communicate with the particle shader
* Save the output in a generalized format (such as JSON or XML) that can be interpreted by any number of applications, making the entire solution more portable. Doing this would probably involve creating Houdini-side previewing tools.
* Add shelf tools that help organize your effect library

---

To summarize, I was extremely surprised at how easy it was to get this idea up and running, especially considering that particle systems aren’t exactly known for their portability. Houdini and Unity both provide fantastic amounts of freedom and flexibility, so I’m eager to take this further and really turn it into my own ideal particle authoring tool. I hope this post inspired you to think about how you best feel our work should be authored, and how we might get our tools closer to that point. Thanks for reading!

Getting Started
===
*Please note that the software is provided as-is, and is not at a production-ready stage. I currently strongly advise against integrating this tool into your existing workflow.*

**[Download](https://github.com/ChrisJ3D/VOP_VFX/archive/master.zip)**
--

After downloading from the link above, extract the contents into a folder of your choosing. The [Houdini Engine for Unity](https://www.sidefx.com/products/houdini-engine/unity-plug-in/) plugin is included in the repository for the sake of simplicity, but please note that it requires a Houdini Engine license on your computer to load.

This tool was created using the following software versions, and compatibility is not guaranteed with newer or older versions:
* `Unity 2017.2.1f1`
* `Houdini 16.6.268`
* `Houdini Engine 3.1.11`

To explore the Houdini side of the setup, open `Houdini/hip/Main.hip`. I've added notes to the network that will hopefully help explain how it works. To create a working copy of the VOP network, use the **Save As New Effect** button in the asset interface.

To bring an asset into Unity, open the project and use the **Houdini Engine - Load Asset** menu, and select the HDA that you want to load. A VOPParser script will automatically be added to the imported asset, which handles the creation of a new particle system.