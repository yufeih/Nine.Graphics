# Nine.Graphics [![NuGet Version](http://img.shields.io/nuget/v/Nine.Graphics.svg)](https://www.nuget.org/packages/Nine.Graphics) [![Build status](https://ci.appveyor.com/api/projects/status/lj0j06cxmlhymr3f)](https://ci.appveyor.com/project/yufeih/nine-graphics)

> This project is still a work in progress, a lot of things are not implemented yet.

Nine.Graphics is an open source graphics framework for building games and applications that takes advance of modern graphics hardware. It is the successor of [Engine Nine](http://nine.codeplex.com).

With Nine.Graphics we are making a number of architectual changes that makes the core foundational  graphics framework much leaner and more moduler:

## Cross-Platform Runtime

Nine.Graphics targets [.NET core](https://github.com/dotnet/corefx) and [DNX](https://github.com/aspnet/dnx) as the .NET runtime environment, the graphics stack sits directly on top of [OpenTK](https://github.com/opentk/opentk)(OpenGL managed invoker) and [SharpDX](https://github.com/sharpdx/sharpdx)(DirectX managed invoker), making it capable of running on Windows, Linux, Mac, iOS and Android. 

## Modern Interactive Development

Traditionally, programs are developed using the *Edit -> Compile -> Run* loop, Nine.Graphics takes advantage of dynamic compilation provided by [Roslyn](https://github.com/dotnet/roslyn) to streamline developer experience, each time a change is made to the source code, the application automatically refresh itself with the execution state preserved. You gain the productivity of an interpreted language without sacrificing the benifits of a compiled language.

The content pipeline is build directly into the runtime, so changes to assets are watched and automatically reloaded without the need to rebuild and rerun the whole program.

## Multi-threaded Architecture

Nine.Graphics makes it easy to write correct multi-threaded programs that separates simulation and rendering into different threads at large scale. The rendering pipeline itself also takes advantage of multi-threaded rendering whenever possible to reduce frame time.

## Immutable Data Driven and Pure Functions

The core rendering API is exposed though an *immutable struct object model*, the rendering pipeline is simply a function that takes this *immutable object state*, performs a series of *pure* data transforms to generate triangles and commands *with no side effects*, then submit those commands to the GPU to produce a predictable image. This makes it easy to write parallel code that separates simulation and rendering.

```csharp
nextState = update(state)
draw(state)
```

Because states are immutable and complete, it is a lot simpler for the rendering system to batch operations whenever possible, which yields better performance.

## Serializable

The object models are all flat *POCO*s, the fields only expose basic primitive types and equivalents, with no nested objects or complex reference graph, making it extreamly easy to integrate with any binary or text serializers.

## Flexible and Extensible

Nine.Graphics is written with dependency injection in mind, just about every interface or functionality can be dependency injected and replaced with a custom implementation. Most of the classes exposed are dependency injection friendly and can be used directly with major IoC containers.
