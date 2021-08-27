cmake_minimum_required(VERSION 3.7)

project(vsgUnity
        VERSION 0.0.0
        DESCRIPTION "Unity support"
        LANGUAGES CXX
)

# find the vsg
if (VULKAN_SDK)
    set(ENV{VULKAN_SDK} ${VULKAN_SDK})
endif()

#find vulkan and vsg
find_package(Vulkan REQUIRED)
find_package(vsg REQUIRED)

add_target_clobber()
add_target_uninstall()

add_subdirectory(unity2vsg)

add_feature_summary()

