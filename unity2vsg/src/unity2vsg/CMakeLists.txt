
SET(HEADER_PATH ${CMAKE_SOURCE_DIR}/include/unity2vsg)

set(HEADERS
    ${HEADER_PATH}/Export.h
    ${HEADER_PATH}/unity2vsg.h
	${HEADER_PATH}/DebugLog.h
	${HEADER_PATH}/NativeUtils.h
	${HEADER_PATH}/GraphicsPipelineBuilder.h
	${HEADER_PATH}/ShaderUtils.h	
)

set(SOURCES
    unity2vsg.cpp
    DebugLog.cpp
	GraphicsPipelineBuilder.cpp
	ShaderUtils.cpp
    glsllang/ResourceLimits.cpp
)

add_library(unity2vsg SHARED ${HEADERS} ${SOURCES})

set_property(TARGET unity2vsg PROPERTY VERSION ${UNITY2VSG_VERSION_MAJOR}.${UNITY2VSG_VERSION_MINOR}.${UNITY2VSG_VERSION_PATCH})
set_property(TARGET unity2vsg PROPERTY SOVERSION ${UNITY2VSG_SOVERSION})
set_property(TARGET unity2vsg PROPERTY POSITION_INDEPENDENT_CODE ON)
set_property(TARGET unity2vsg PROPERTY CXX_STANDARD 17)

target_include_directories(unity2vsg PUBLIC
    $<BUILD_INTERFACE:${CMAKE_SOURCE_DIR}/include>
    $<INSTALL_INTERFACE:include>
)

target_link_libraries(unity2vsg PUBLIC
    vsg::vsg
    ${GLSLANG}
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


add_custom_command(TARGET unity2vsg POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E make_directory ${CMAKE_SOURCE_DIR}/../UnityProject/Assets/vsgUnity/Native/Plugins/${PLATFORM_PLUGIN_FOLDER}
)

add_custom_command(TARGET unity2vsg POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:unity2vsg> ${CMAKE_SOURCE_DIR}/../UnityProject/Assets/vsgUnity/Native/Plugins/${PLATFORM_PLUGIN_FOLDER}
)

