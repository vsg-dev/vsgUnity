
SET(HEADER_PATH ${CMAKE_SOURCE_DIR}/include/unity2vsg)

set(HEADERS
    ${HEADER_PATH}/Export.h
    ${HEADER_PATH}/unity2vsg.h
	${HEADER_PATH}/Debug.h
	${HEADER_PATH}/DataTypes.h
	${HEADER_PATH}/GraphicsPipelineBuilder.h
	${HEADER_PATH}/ShaderUtils.h	
)

set(SOURCES
    unity2vsg.cpp
    Debug.cpp
    DataTypes.cpp
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


install(TARGETS unity2vsg EXPORT unity2vsgTargets
        LIBRARY DESTINATION lib
        ARCHIVE DESTINATION lib
        RUNTIME DESTINATION bin
        INCLUDES DESTINATION include
)

#if (BUILD_SHARED_LIBS)
    target_compile_definitions(unity2vsg PUBLIC UNITY2VSG_SHARED_LIBRARY)
#endif()


install(DIRECTORY ${CMAKE_SOURCE_DIR}/include/unity2vsg DESTINATION include)

# [==[
install(EXPORT unity2vsgTargets
    FILE unity2vsgTargets.cmake
    NAMESPACE unity2vsg::
    DESTINATION lib/cmake/unity2vsg
)

include(CMakePackageConfigHelpers)
write_basic_package_version_file("${CMAKE_BINARY_DIR}/src/unity2vsg/unity2vsgConfigVersion.cmake"
    VERSION ${UNITY2VSG_MAJOR_VERSION}.${UNITY2VSG_MINOR_VERSION}.${UNITY2VSG_PATCH_VERSION}
    COMPATIBILITY SameMajorVersion
)

install(FILES "unity2vsgConfig.cmake" "${CMAKE_BINARY_DIR}/src/unity2vsg/unity2vsgConfigVersion.cmake"
    DESTINATION lib/cmake/unity2vsg
)

# ]==]
