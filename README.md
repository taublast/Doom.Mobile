To compile this you need to put a custom fork https://github.com/taublast/Plugin.Maui.Audio from at the same level then this cloned repo, in parallel.
As doom is computing its walls on CPU do not try to debug this on mobile.. See performance notes below.
Actually repo already contains the doom2 wad file included for a fast usage, until going public.

# DOOM Mobile

A cross-platform DOOM game, play it on iOS, MacCatalyst, Android and Windows.

Uses a modified C# Doom engine of [ManagedDoom](https://github.com/sinshu/managed-doom).

## Why Another .NET DOOM?

* Cross-platform implementation for iOS, MacCatalyst, Android and Windows. 
* Mobile touch gestures and additionally keyboard on desktop.
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
    "freedoom1.wad"
```
You can find out more about this subject by googling one of this filenames.

### Performance

To have max FPS:

* On Android you need to compile a Release version and run it on a real device. Tested to run smoothly on a slow device.
* On Mac you can use a Debug version without debugging (important).
* On iOS both for simulator and real device you can use a Debug version without debugging. Notice a bug in Visual Studio for Windows that starts a debugging session even when you start without debug. Either use a next version (preview) or drop the connection with Mac to break the debug session.
* On Windows you can play a Debug version even when debugging.

### Controls

* Inside MENU panning replaces arrows keys.
* ESC left-top screen screen corner. ENTER is everywhere but this corner when menu is open.
* While playing panning replaces mouse, tap to FIRE and tap on your avatar to USE, open doors etc. 
* Switch weapons by tapping in the lower-left corner of the screen.
* Open auto-map tapping in the right-top corner.
* On desktop you can also use usual keyboard keys, defaults is FIRE with CONTROL, USE with SPACE..
* Mouse on desktop is acting different from original DOOM as this version is touch-screens-friendly in the first place.

## Behind The Scenes

Stack: [.NET MAUI](https://dotnet.microsoft.com/en-us/apps/maui), [SkiaSharp](https://github.com/mono/SkiaSharp), [DrawnUI](https://github.com/taublast/DrawnUi.Maui), [Plugin.Maui.Audio](https://github.com/jfversluis/Plugin.Maui.Audio).

* Reusing modified C# Doom engine of [ManagedDoom](https://github.com/sinshu/managed-doom).
* Video: hardware-accelerated SkiaSharp v3 rendering with DrawnUI for .NET MAUI engine.
* Input: DrawnUI Canvas mobile-friendly touch gestures, full keyboard support for WIndows and MacCatalyst desktop versions.
* Sound: customized `Plugin.Maui.Audio` provides a cross-platform multi-channel sound system.
* Targets .NET 9.

## Dev Notes

* Projects are separated into shared code and MAUI implementation.
* The original ManagedDoom Silk for Windows implementation was kept to serve as a development reference.
* Android Debug version is laggy, Release tested to be fine on a slow device.
* iOS simulator M-chip compatible.
* Was developed and tested using doom2.wad.

## To Do

* UI for selecting one of the WADs when many are found.
* Track selected weapon and highlight its number in custom UI.
* MIDI music

## Ancestors

[DOOM](https://github.com/id-Software/DOOM) -> [Linux Doom](https://github.com/id-Software/DOOM) -> [ManagedDoom](https://github.com/sinshu/managed-doom) -> this project.

 ## License

The [GPLv2 license](LICENSE.txt) inherited from ancestors.
