LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

# Replace "your_cpp_library_name" with the desired name for your shared library
LOCAL_MODULE := testimu

# Add all your C++ source files here
LOCAL_SRC_FILES := HelloWorldPlugin.cpp

include $(BUILD_SHARED_LIBRARY)

