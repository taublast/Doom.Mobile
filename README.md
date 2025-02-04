# DOOM Mobile

A cross-platform DOOM game, play it on **iOS, MacCatalyst, Android and Windows**.

https://github.com/user-attachments/assets/0367ba8f-4461-489d-9862-885892e405e6

Uses a modified C# Doom engine from [ManagedDoom](https://github.com/sinshu/managed-doom).

## Why Another .NET DOOM?

* Cross-platform implementation for iOS, MacCatalyst, Android and Windows. 
* Mobile touch gestures and additionally keyboard for desktop.
* Custom UI to select weapons on mobile, tap left-bottom corner to open.
* Multi-channel stereo sound working on all platforms.

## How To Play

### Setup

While DOOM source code was openely released for non-profit use, it requires you to own a real copy of one of the DOOMs. 
This project code would look for doom data `.wad` file placed inside `Resources/Raw` folder as `MauiAsset`. It looks for one of the following:
```
    "doom2.wad",
    "plutonia.wad",
    "tnt.wad",
    "doom.wad",
    "doom1.wad",
    "freedoom2.wad",
    "freedoom1.wad",
```
You can find out more about this subject by googling one of this filenames.

This repo contains `freedoom2.wad` for a fast start, [free content under BSD licence](https://freedoom.github.io/). You can replace it with your own file.

You can set the `static bool KeepAspectRatio` inside `MauiProgram` to `false` if you want a more immersive experience.  
As doom is computing its walls on CPU do not try to debug this on mobile.. See performance notes below.  
The final texture is rendered using a shader so it can be modified in the future to do more work on GPU for the DOOM engine.  

### Performance

* On Windows you can play a Debug version even when debugging.
* On Android to have playable fps you need to compile a Release and run it on a real device, it runs smoothly even on a slow device.
* On iOS both for simulator and real device are fine to play without debugging. Just notice a temporary bug in Visual Studio for Windows that still debugs your app even when started on iOS without debug. Use VS preview version or drop connection with Mac and re-run app.
* On Mac (Catalyst) when starting without debugging you can play even a Debug build.

### Controls

* Inside MENU panning replaces arrows keys.
* ESC left-top screen screen corner. ENTER is everywhere but this corner when menu is open.
* While playing panning replaces mouse, tap to FIRE and tap on your avatar to USE, open doors etc. 
* Switch weapons by tapping in the lower-left corner of the screen.
* Open auto-map tapping in the right-top corner.
* On desktop you can also use usual keyboard keys, defaults is FIRE with CONTROL, USE with SPACE..
* Mouse on desktop is acting different from original DOOM as this version is touch-screens-friendly in the first place.

Could to be much improved, not only the gestures code, but also maybe could add some HUD buttons for movement. Please leave your thoughts in Discussions.

## Behind The Scenes

Stack: [.NET MAUI](https://dotnet.microsoft.com/en-us/apps/maui), [SkiaSharp](https://github.com/mono/SkiaSharp), [DrawnUI](https://github.com/taublast/DrawnUi.Maui), [Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio).

* Reusing modified C# Doom engine of [ManagedDoom](https://github.com/sinshu/managed-doom).
* Video: hardware-accelerated SkiaSharp v3 rendering with DrawnUI for .NET MAUI engine.
* Input: DrawnUI Canvas mobile-friendly touch gestures, full keyboard support for WIndows and MacCatalyst desktop versions.
* Sound: [customized](https://github.com/taublast/Plugin.Maui.Audio) `Plugin.Maui.Audio` provides a cross-platform multi-channel sound system.
* Targets .NET 9.

## Dev Notes

* Projects are separated into shared code and MAUI implementation.
* The original ManagedDoom Silk for Windows implementation was kept to serve as a development reference.
* iOS simulator M-chip compatible.
* iOS real device requires interpreter OFF to work with System.Threadig.Tasks.Parellel.

## To Do

* Enhance Input with controller support and maybe add HUD buttons.
* UI for selecting one of the WADs when many are found.
* Track selected weapon and highlight its number in custom UI.
* MIDI music, totally doable with `Plugin.Maui.Audio`, just need more free time for that.

## Ancestors

[DOOM](https://github.com/id-Software/DOOM) -> [Linux Doom](https://github.com/id-Software/DOOM) -> [ManagedDoom](https://github.com/sinshu/managed-doom) -> this project.

 ## License

The [GPLv2 license](LICENSE.txt) inherited from ancestors.
