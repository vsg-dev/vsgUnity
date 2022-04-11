
SET(HEADER_PATH ${CMAKE_SOURCE_DIR}/unity2vsg/include/unity2vsg)

set(HEADERS
    ${HEADER_PATH}/Export.h
    ${HEADER_PATH}/unity2vsg.h
	${HEADER_PATH}/DebugLog.h
	${HEADER_PATH}/NativeUtils.h
	${HEADER_PATH}/GraphicsPipelineBuilder.h
	${HEADER_PATH}/ShaderUtils.h
	${HEADER_PATH}/SkyboxUtils.h
)

set(SOURCES
    unity2vsg.cpp
    DebugLog.cpp
	GraphicsPipelineBuilder.cpp
	ShaderUtils.cpp
	SkyboxUtils.cpp
)

add_library(unity2vsg SHARED ${HEADERS} ${SOURCES})

# check whehther glslang/build_info.h exists
if (EXISTS ${glslang_INCLUDE_DIR}/glslang/build_info.h)
    set(EXTRA_DEFINES ${EXTRA_DEFINES} GLSLANG_HAS_BUILD_INFO_H)
endif()

set_property(TARGET unity2vsg PROPERTY VERSION ${UNITY2VSG_VERSION_MAJOR}.${UNITY2VSG_VERSION_MINOR}.${UNITY2VSG_VERSION_PATCH})
set_property(TARGET unity2vsg PROPERTY NO_SONAME 1)
set_property(TARGET unity2vsg PROPERTY POSITION_INDEPENDENT_CODE ON)
set_property(TARGET unity2vsg PROPERTY CXX_STANDARD 17)

target_compile_definitions(unity2vsg PRIVATE ${EXTRA_DEFINES})

target_include_directories(unity2vsg PUBLIC
    $<BUILD_INTERFACE:${CMAKE_SOURCE_DIR}/unity2vsg/include>
    $<INSTALL_INTERFACE:include>
)

target_link_libraries(unity2vsg PUBLIC
    vsg::vsg
)

#if (BUILD_SHARED_LIBS)
    target_compile_definitions(unity2vsg PUBLIC UNITY2VSG_SHARED_LIBRARY)
#endif()

# copy the binary into the unity project

if(WIN32)
    set(PLATFORM_PLUGIN_FOLDER Windows)
elseif(APPLE)
    set(PLATFORM_PLUGIN_FOLDER Mac)
elseif(UNIX)
    set(PLATFORM_PLUGIN_FOLDER Linux)
endif()

set(PLATFORM_PLUGIN_PATH ${CMAKE_SOURCE_DIR}/../UnityProject/Assets/vsgUnity/Native/Plugins/${PLATFORM_PLUGIN_FOLDER})

add_custom_command(TARGET unity2vsg POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E make_directory ${PLATFORM_PLUGIN_PATH}
)

add_custom_command(TARGET unity2vsg POST_BUILD
    COMMENT "*** Copying plugin to Unity project folder ${PLATFORM_PLUGIN_PATH} ensure Unity is closed or the plugin can't be copied ***"
    COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:unity2vsg> ${PLATFORM_PLUGIN_PATH}
)

