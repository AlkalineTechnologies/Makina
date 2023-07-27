cmake_minimum_required(VERSION 3.12)
project(Engine)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++20 -pthread")

file(GLOB_RECURSE SOURCES "./**.cpp" EXCLUDE "./build/CMakeFiles/*")

add_library(Engine ${SOURCES})

target_include_directories(Engine PUBLIC "${CMAKE_SOURCE_DIR}")