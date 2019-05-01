# vsgUnity
Native Unity Plugin for exporting VulkanSceneGraph files from Unity3D.

## Project Layout
The vsgUnity project consists of two Libraries
### unity2vsg
A C++ library exposing functionality to build, save and preview VSG graphs.
### vsgUnity
A Unity3D project that utilises unity2vsg to export a Unity GameObject. The Assets/vsgUnity folder holds the main plugin code and can be copied into other Unity3D projects. When building from source the unity2vsg binary you build must be copied into  Assets/vsgUnity/Native/Plugins/(PlatformName) where (PlatformName) is the name of the OS platform you are using.

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

Once built copy unity2vsg.dll into vsgUnity/Assets/vsgUnity/Native/Plugins/Windows.


