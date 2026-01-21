cmake_minimum_required(VERSION 3.20)
project(ScjTsfEngine LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_library(ScjTsfEngine SHARED
    src/dllmain.cpp
    src/class_factory.cpp
    src/guids.cpp
    src/cin_table.cpp
    src/pipe_client.cpp
    src/text_service.cpp
    src/edit_sessions.cpp

    include/class_factory.h
    include/guids.h
    include/cin_table.h
    include/pipe_client.h
    include/text_service.h
    include/edit_sessions.h
)

target_include_directories(ScjTsfEngine PRIVATE include)
target_compile_definitions(ScjTsfEngine PRIVATE UNICODE _UNICODE NOMINMAX)

target_link_libraries(ScjTsfEngine PRIVATE ole32 uuid msctf shlwapi)

set_target_properties(ScjTsfEngine PROPERTIES OUTPUT_NAME "ScjTsfEngine")
