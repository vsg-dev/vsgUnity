cmake_minimum_required(VERSION 3.7)

project(UNITY2VSG
    VERSION 0.0.0
    DESCRIPTION "Unity VSG intergration library"
    LANGUAGES CXX
)
set(UNITY2VSG_SOVERSION 0)


find_package(vsgUnity_glslang REQUIRED CONFIG PATHS ${UNITY2VSG_SOURCE_DIR}/CMakeModules)

add_target_clang_format(
    FILES
        include/unity2vsg/*.h
        src/unity2vsg/*.cpp
        src/unity2vsg/*.h
)
add_target_cppcheck(
    FILES
        ${CMAKE_CURRENT_SOURCE_DIR}/include
        ${CMAKE_CURRENT_SOURCE_DIR}/src
)
add_target_docs(
    FILES
        include/
)

# src contains unity2vsg project source code and cmakelists
add_subdirectory(src/unity2vsg)
