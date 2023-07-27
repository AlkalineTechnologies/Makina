cmake_minimum_required(VERSION 3.15)
project(MAKINA)

# enable_language(CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
# set(CMAKE_CXX_EXTENSIONS ON)
# set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++20 -pthread")

file(GLOB_RECURSE SOURCES "${CMAKE_SOURCE_DIR}/**.cpp" )
list(FILTER SOURCES EXCLUDE REGEX ".*CompilerIdCXX.*")

add_executable(MAKINA ${SOURCES})

target_include_directories(MAKINA PUBLIC "${CMAKE_SOURCE_DIR}")

target_link_directories(MAKINA PUBLIC "${CMAKE_SOURCE_DIR}/lib")
target_link_libraries(MAKINA PRIVATE Engine)
