cmake_minimum_required(VERSION 3.7)

project(vsgUnity
        VERSION 0.0.0
        DESCRIPTION "Unity support"
        LANGUAGES CXX
)
set(vsgUnity_RELEASE_CANDIDATE 0)

# find the vsg
if (VULKAN_SDK)
    set(ENV{VULKAN_SDK} ${VULKAN_SDK})
endif()

#find vulkan and vsg
find_package(Vulkan REQUIRED)
find_package(vsg 0.1.6 REQUIRED)

vsg_add_target_clobber()
vsg_add_target_uninstall()

vsg_add_option_maintainer(
    PREFIX vsgUnity
    RCLEVEL ${vsgUnity_RELEASE_CANDIDATE}
)

add_subdirectory(unity2vsg)

vsg_add_feature_summary()

