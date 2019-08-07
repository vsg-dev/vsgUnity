# vsgUnity
Native Unity Plugin for exporting VulkanSceneGraph files from Unity3D.

## Project Layout
The vsgUnity project consists of two Libraries
### unity2vsg
A C++ library exposing functionality to build, save and preview VSG graphs.
### vsgUnity
A Unity3D plugin that utilises unity2vsg to export a Unity GameObject or Scene.

To aid development a complete Unity project is included in this repository and contains the
vsgUnity scripts as well as some useful test scenes. To open it select the root UnityProject
folder when opening a project in Unity.

The UnityProject/Assets/vsgUnity folder holds the main plugin scripts and can be copied into other Unity3D projects. When building from source the unity2vsg binary must be copied into UnityProject/Assets/vsgUnity/Native/Plugins/(PlatformName) where (PlatformName) is the name of the OS platform you are using, unity2vsg contains a post build step that should do this for you.

Also note that you'll need to close Unity any time you want to copy a new unity2vsg binary as it'll
be locked and the Unity only reloads the library on startup.

## Building unity2vsg
### Prerequisites
* C++17 compliant compiler i.e. g++ 7.3 or later, Clang 6.0 or later, Visual Studio S2017 or later.
* [Vulkan](https://vulkan.lunarg.com/) 1.1 or later.
* [glslang](https://github.com/KhronosGroup/glslang)
* [CMake](https://www.cmake.org) 3.7 or later.

Ensure Vulkan is installed and the VULKAN_SDK environment variable has been set and that glsllang has been built and installed and that it's install path has been added to your CMAKE_PREFIX_PATH. The [VulkanSceneGraph](https://github.com/vsg-dev/VulkanSceneGraph/blob/master/INSTALL.md#detailed-instructions-for-setting-up-your-environment-and-building-for-microsoft-windows) project has more details on these subjects.

### Building for Windows using Visual Studio 2017
Command line instructions for default build of shared library (.dll) in source:

    git clone https://github.com/tomhog/vsgUnity
    cd vsgUnity/unity2vsg
    cmake . -G "Visual Studio 15 2017 Win64"

A post build step will copy unity2vsg.dll into UnityProject/Assets/vsgUnity/Native/Plugins/Windows.
Ensure Unity is closed or the .dll file will not copy.

### Building for Unix
Command line instructions for default build of shared library (.so) in source:

    git clone https://github.com/tomhog/vsgUnity
    cd vsgUnity/unity2vsg
    cmake .
    make -j 8

A post build step will copy libunity2vsg.so into UnityProject/Assets/vsgUnity/Native/Plugins/Linux.
Ensure Unity is closed or the .so file will not copy.

## Using vsgUnity

As stated above vsgUnity consists of a collection of Unity scripts (.cs files) and the unity2vsg C++ library.
Everything needed for vsgUnity to run within a Unity project is contained in the UnityProject/Assets/vsgUnity
folder. If you want to use vsgUnity in you own project just copy that entire folder.

If you open the UnityProject folder as a project in Unity you can try out vsgUnity. Once open goto

    Windows>VulkanSceneGraph>Exporter

A window will popup allowing you to select the GameObject you want to export (leave it blank to export
the entire current scene). There are also various options and a option to preview your file ina VSG
viewer.

