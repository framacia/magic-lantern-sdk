#include "HelloWorldPlugin.h"
#include <iostream>
#include <cstring>
#include <cstdio>
#include <fcntl.h>
#include <unistd.h>
#include <linux/i2c-dev.h>
// #include <android/log.h>


const int ACCEL_REGISTER = 0x08;
const float ACCEL_SCALE_FACTOR = 1.0 / 100;
const int GYRO_REGISTER = 0x14;
const float GYRO_SCALE_FACTOR = 1.0 / 16.0;
const int EULER_REGISTER = 0x1a;
const float EULER_SCALE_FACTOR = 1.0 / 16.0;
const int QUATERNION_REGISTER = 0X20;
const float QUATERNION_SCALE_FACTOR = (1.0 / (1 << 14));;
const int WRITE_BUFFER_SIZE = 2;


int setupDevice(const char* device_path, int device_address) {
    int fileDescriptor = open(device_path, O_RDWR);
    if (fileDescriptor == -1) {
        // std::cerr << "Failed to open i2c port" << std::endl;
        return -1;
    }

    if (ioctl(fileDescriptor, I2C_SLAVE, device_address) == -1) {
        close(fileDescriptor);
        // std::cerr << "Failed to read device address" << std::endl;
        return -1;
    }

    return fileDescriptor;
}

int readRegisterData(int fileDescriptor, unsigned char registerAddress, unsigned char* data, size_t dataSize) {
    unsigned char buffer[1];
    buffer[0] = registerAddress;

    if (write(fileDescriptor, buffer, sizeof(buffer)) != sizeof(buffer)) {
        close(fileDescriptor);
        // __android_log_write(ANDROID_LOG_ERROR, "Error", "Failed to write register address");
        return -1; // Error handling if write fails
    }

    if (read(fileDescriptor, data, dataSize) != static_cast<ssize_t>(dataSize)) {
        close(fileDescriptor);
        // __android_log_write(ANDROID_LOG_ERROR, "Error", "Failed to read register data");
        return -1; // Error handling if read fails
    }

    return 0;
}

void convertSensorData(int fileDescriptor, int sensorAddress, float& sensorX, float& sensorY, float& sensorZ, float scaleFactor) {
    unsigned char sensorData[6];
    if (readRegisterData(fileDescriptor, sensorAddress, sensorData, sizeof(sensorData)) != 0) {
        close(fileDescriptor);
        sensorX = sensorY = sensorZ = 0.0f;
        return; // Error handling in the readRegisterData function
    }

    int sensorRawZ = (sensorData[1] << 8) | sensorData[0];
    int sensorRawX = (sensorData[3] << 8) | sensorData[2];
    int sensorRawY = (sensorData[5] << 8) | sensorData[4];

    sensorX = static_cast<float>(static_cast<short>(sensorRawX)) * scaleFactor;
    sensorY = static_cast<float>(static_cast<short>(sensorRawY)) * scaleFactor;
    sensorZ = static_cast<float>(static_cast<short>(sensorRawZ)) * scaleFactor;
}

void convertQuat(int fileDescriptor, int sensorAddress, float& sensorX, float& sensorY, float& sensorZ, float& sensorW, float scaleFactor) {
    unsigned char sensorData[8];
    if (readRegisterData(fileDescriptor, sensorAddress, sensorData, sizeof(sensorData)) != 0) {
        close(fileDescriptor);
        sensorX = sensorY = sensorZ = sensorW = 0.0f;
        return; // Error handling in the readRegisterData function
    }

    int sensorRawW = (sensorData[1] << 8) | sensorData[0];
    int sensorRawX = (sensorData[3] << 8) | sensorData[2];
    int sensorRawY = (sensorData[5] << 8) | sensorData[4];
    int sensorRawZ = (sensorData[7] << 8) | sensorData[6];

    sensorX = scaleFactor * static_cast<float>(static_cast<short>(sensorRawX));
    sensorY = scaleFactor * static_cast<float>(static_cast<short>(sensorRawY));
    sensorZ = scaleFactor * static_cast<float>(static_cast<short>(sensorRawZ));
    sensorW = scaleFactor * static_cast<float>(static_cast<short>(sensorRawW));
    
}



extern "C" bool setMode(const char* device_path, int device_address, unsigned char register_address, unsigned char mode) {
    int fileDescriptor = setupDevice(device_path, device_address);
    if (fileDescriptor == -1) {
        return false; // Return the array on error
    }

    unsigned char writeBuffer[WRITE_BUFFER_SIZE] = {register_address, mode};
    if (write(fileDescriptor, writeBuffer, sizeof(writeBuffer)) != sizeof(writeBuffer)) {
        // __android_log_write(ANDROID_LOG_ERROR, "Error", "Failed to write register value");
        close(fileDescriptor);
        return false;
    }

    close(fileDescriptor);
    return true;
}

extern "C" float* getEuler(const char* device_path, int device_address) {
    static float euler[3] = {0.0f, 0.0f, 0.0f}; // Initialize a static float array with 3 elements and set them to 0.0f

    int fileDescriptor = setupDevice(device_path, device_address);
    if (fileDescriptor == -1) {
        return euler; // Return the array on error
    }

    float eulerX, eulerY, eulerZ;
    convertSensorData(fileDescriptor, EULER_REGISTER, eulerX, eulerY, eulerZ, EULER_SCALE_FACTOR);
    // std::cout << "Angular data: Roll = " << eulerX << ", Pitch = " << eulerY << ", Yaw = " << eulerZ << std::endl;

    close(fileDescriptor);

    euler[0] = eulerZ;
    euler[1] = eulerX;
    euler[2] = eulerY;

    return euler;
}

extern "C" float* getQuaternion(const char* device_path, int device_address) {
    static float Quaternion[4] = {0.0f, 0.0f, 0.0f, 0.0f}; // Initialize a static float array with 3 elements and set them to 0.0f

    int fileDescriptor = setupDevice(device_path, device_address);
    if (fileDescriptor == -1) {
        return Quaternion; // Return the array on error
    }

    float QuatX, QuatY, QuatZ, QuatW;
    convertQuat(fileDescriptor, QUATERNION_REGISTER, QuatX, QuatY, QuatZ, QuatW, QUATERNION_SCALE_FACTOR);
    // std::cout << "Angular data: Roll = " << eulerX << ", Pitch = " << eulerY << ", Yaw = " << eulerZ << std::endl;

    close(fileDescriptor);

    Quaternion[0] = QuatW;
    Quaternion[1] = QuatX;
    Quaternion[2] = QuatY;
    Quaternion[3] = QuatZ;

    return Quaternion;
}



