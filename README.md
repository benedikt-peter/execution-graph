# Unity ExecutionGraph

A framework for implementing execution graphs in Unity.

This project is a Work-In-Progress playground and a reimplementation of a solution I built for one of my game projects.

The goal is to provide a framework for implementing custom logic graphs in games. Contrary to very generic solutions
like Unreal Blueprints or Unity Visual Scripting, this framework allows to design graph systems for very specific
use cases with a restricted number of available actions and operations. This comes in handy when logic has to run in
an environment where special caution has to be exercised about which operations are allowed and where they may be
executed, like in multiplayer games where logic may run on both the server and the client side.

## Goals
* Allow a game designer to build custom game logic within the Unity Editor
* Allow a game programmer to easily implement actions and operators for the game designer to use
* Provide performant execution at runtime without relying on performance-heavy or non-supported features (like cloning 
a lot of Unity objects, ...)
* Provide means to debug the graphs in the Editor

## Ideas / Things to try out
* Use Code Generation to generate a runtime version of the graph
* Provide a debug runner that allows to run graphs in the Unity Editor without a domain reload (using reflection etc.)

## Inspirations and sources
* https://github.com/JamesLaFritz/GraphViewBehaviorTree
(Implementation examples for using the Unity Graph UI)
* https://gdcvault.com/play/1035300/Tools-Summit-Nodes-and-Native
(GDC 2025 "Nodes and Native Code: DECIMA's Visual Programming for Every Discipline" by Bryan Keiren)