cmake_minimum_required(VERSION 3.16)
project(RiskCalculations)

# Set C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Set build type
if(NOT CMAKE_BUILD_TYPE)
    set(CMAKE_BUILD_TYPE Release)
endif()

# Compiler-specific options
if(MSVC)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W3")
    set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /O2 /DNDEBUG")
else()
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -pedantic")
    set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} -O3 -DNDEBUG")
endif()

# Create shared library
add_library(RiskCalculations SHARED RiskCalculations.cpp)

# Set output directory
set_target_properties(RiskCalculations PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin
    LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
    ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
)

# Platform-specific settings
if(WIN32)
    set_target_properties(RiskCalculations PROPERTIES
        OUTPUT_NAME "RiskCalculations"
        SUFFIX ".dll"
    )
elseif(APPLE)
    set_target_properties(RiskCalculations PROPERTIES
        OUTPUT_NAME "RiskCalculations"
        SUFFIX ".dylib"
    )
else()
    set_target_properties(RiskCalculations PROPERTIES
        OUTPUT_NAME "RiskCalculations"
        SUFFIX ".so"
    )
endif()

# Install targets
install(TARGETS RiskCalculations
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)

# Install headers
install(FILES RiskCalculations.h DESTINATION include)
