// BNO055.cpp (Source File)
#include "BNO055.h"
#include <fcntl.h>
#include <unistd.h>
#include <linux/i2c-dev.h>
#include <android/log.h>
#include <cstdint>
#include <cstring>
#include <vector>

const int device_address = 0x28;
const char* device_path = "/dev/i2c-5";

class BNO055Device {
public:
    BNO055Device(const char* device_path, int device_address) : fileDescriptor(-1) {
        fileDescriptor = open(device_path, O_RDWR);
        if (fileDescriptor == -1) {
            __android_log_write(ANDROID_LOG_ERROR, "Error", "Failed to open the port");
        } else {
            if (ioctl(fileDescriptor, I2C_SLAVE, device_address) == -1) {
                __android_log_write(ANDROID_LOG_ERROR, "Error", "Failed to read device address");
                close();
            }
        }
    }

    ~BNO055Device() {
        close();
    }

    int getFileDescriptor() const {
        return fileDescriptor;
    }

private:
    void close() {
        if (fileDescriptor != -1) {
            ::close(fileDescriptor);
            fileDescriptor = -1;
        }
    }

    int fileDescriptor;
};

BNO055Device device(device_path, device_address);

// Helper function to read sensor data from the device
int readSensorData(BNO055Device& device, unsigned char registerAddress, unsigned char* data, size_t dataSize) {
    unsigned char buffer[1];
    buffer[0] = registerAddress;

    if (write(device.getFileDescriptor(), buffer, sizeof(buffer)) != sizeof(buffer)) {
        __android_log_write(ANDROID_LOG_ERROR, "Error", "Failed to write register address");
        return -1; // Error handling if write fails
    }

    if (read(device.getFileDescriptor(), data, dataSize) != static_cast<ssize_t>(dataSize)) {
        __android_log_write(ANDROID_LOG_ERROR, "Error", "Failed to read register data");
        return -1; // Error handling if read fails
    }

    return 0;
}

// Helper function to convert sensor data
void convertSensorData(const unsigned char sensorData[6], float& sensorX, float& sensorY, float& sensorZ, float scaleFactor) {
    int sensorRawX = (sensorData[1] << 8) | sensorData[0];
    int sensorRawY = (sensorData[3] << 8) | sensorData[2];
    int sensorRawZ = (sensorData[5] << 8) | sensorData[4];

    sensorX = static_cast<float>(static_cast<short>(sensorRawX)) * scaleFactor;
    sensorY = static_cast<float>(static_cast<short>(sensorRawY)) * scaleFactor;
    sensorZ = static_cast<float>(static_cast<short>(sensorRawZ)) * scaleFactor;
}

void convertQuat(const unsigned char sensorData[8], float& sensorX, float& sensorY, float& sensorZ, float& sensorW, float scaleFactor) {
    int sensorRawW = (sensorData[1] << 8) | sensorData[0];
    int sensorRawX = (sensorData[3] << 8) | sensorData[2];
    int sensorRawY = (sensorData[5] << 8) | sensorData[4];
    int sensorRawZ = (sensorData[7] << 8) | sensorData[6];

    sensorX = scaleFactor * static_cast<float>(static_cast<short>(sensorRawX));
    sensorY = scaleFactor * static_cast<float>(static_cast<short>(sensorRawY));
    sensorZ = scaleFactor * static_cast<float>(static_cast<short>(sensorRawZ));
    sensorW = scaleFactor * static_cast<float>(static_cast<short>(sensorRawW));
    
}


// Interface function implementations
bool setMode(const char* device_path, int device_address, unsigned char register_address, unsigned char mode) {
    __android_log_write(ANDROID_LOG_ERROR, "Unity", "salio");
    std::vector<float> accel(3, 0.0f);
    // BNO055Device device(device_path, device_address);
    __android_log_write(ANDROID_LOG_ERROR, "Unity", "salio1");
    if (device.getFileDescriptor() == -1) {
        return false;
    }
    __android_log_write(ANDROID_LOG_ERROR, "Unity", "salio2");
    unsigned char writeBuffer[WRITE_BUFFER_SIZE] = {register_address, mode};
    if (write(device.getFileDescriptor(), writeBuffer, sizeof(writeBuffer)) != sizeof(writeBuffer)) {
        __android_log_write(ANDROID_LOG_ERROR, "Error", "Failed to write register value");
        return false;
    }
    __android_log_write(ANDROID_LOG_ERROR, "Unity", "salio3");
    return true;
}

std::vector<float> getAccelerometer(const char* device_path, int device_address) {
    std::vector<float> accel(3, 0.0f);
    // BNO055Device device(device_path, device_address);
    if (device.getFileDescriptor() == -1) {
        return accel;
    }
    unsigned char sensorData[6];
    if (readSensorData(device, ACCEL_REGISTER, sensorData, sizeof(sensorData)) != 0) {
        return accel;
    }
    float accelX, accelY, accelZ;
    convertSensorData(sensorData, accelX, accelY, accelZ, ACCEL_SCALE_FACTOR);
    accel[0] = accelX;
    accel[1] = accelY;
    accel[2] = accelZ;

    return accel;
}

std::vector<float> getMagnetometer(const char* device_path, int device_address) {
    std::vector<float> mag(3, 0.0f);

    // BNO055Device device(device_path, device_address);
    if (device.getFileDescriptor() == -1) {
        return mag;
    }

    unsigned char sensorData[6];
    if (readSensorData(device, MAG_REGISTER, sensorData, sizeof(sensorData)) != 0) {
        return mag;
    }

    float magX, magY, magZ;
    convertSensorData(sensorData, magX, magY, magZ, MAG_SCALE_FACTOR);

    mag[0] = magX;
    mag[1] = magY;
    mag[2] = magZ;

    return mag;
}


std::vector<float> getGyroscope(const char* device_path, int device_address) {
    std::vector<float> gyro(3, 0.0f);

    // BNO055Device device(device_path, device_address);
    if (device.getFileDescriptor() == -1) {
        return gyro;
    }

    unsigned char sensorData[6];
    if (readSensorData(device, GYRO_REGISTER, sensorData, sizeof(sensorData)) != 0) {
        return gyro;
    }

    float gyroX, gyroY, gyroZ;
    convertSensorData(sensorData, gyroX, gyroY, gyroZ, GYRO_SCALE_FACTOR);

    gyro[0] = gyroX;
    gyro[1] = gyroY;
    gyro[2] = gyroZ;

    return gyro;
}


std::vector<float> getEuler(const char* device_path, int device_address) {
    std::vector<float> euler(3, 0.0f);

    // BNO055Device device(device_path, device_address);
    if (device.getFileDescriptor() == -1) {
        return euler;
    }

    unsigned char sensorData[6];
    if (readSensorData(device, EULER_REGISTER, sensorData, sizeof(sensorData)) != 0) {
        return euler;
    }

    float eulerX, eulerY, eulerZ;
    convertSensorData(sensorData, eulerZ, eulerX, eulerY, EULER_SCALE_FACTOR);

    euler[0] = eulerX;
    euler[1] = eulerY;
    euler[2] = eulerZ;

    return euler;
}


std::vector<float> getQuaternion(const char* device_path, int device_address) {
    std::vector<float> quat(4, 0.0f);

    // BNO055Device device(device_path, device_address);
    if (device.getFileDescriptor() == -1) {
        return quat;
    }

    unsigned char sensorData[8];
    if (readSensorData(device, QUATERNION_REGISTER, sensorData, sizeof(sensorData)) != 0) {
        return quat;
    }

    float quatX, quatY, quatZ, quatW;
    convertQuat(sensorData, quatW, quatX, quatY, quatZ, QUATERNION_SCALE_FACTOR);

    quat[0] = quatX;
    quat[1] = quatY;
    quat[2] = quatZ;
    quat[3] = quatW;

    return quat;
}

std::vector<float> getLinearAccel(const char* device_path, int device_address) {
    std::vector<float> linearAccel(3, 0.0f);

    // BNO055Device device(device_path, device_address);
    if (device.getFileDescriptor() == -1) {
        return linearAccel;
    }

    unsigned char sensorData[6];
    if (readSensorData(device, LINEAR_ACC_REGISTER, sensorData, sizeof(sensorData)) != 0) {
        return linearAccel;
    }

    float linearAccelX, linearAccelY, linearAccelZ;
    convertSensorData(sensorData, linearAccelX, linearAccelY, linearAccelZ, LINEAR_ACC_SCALE_FACTOR);

    linearAccel[0] = linearAccelX;
    linearAccel[1] = linearAccelY;
    linearAccel[2] = linearAccelZ;

    return linearAccel;
}


std::vector<float> getGravity(const char* device_path, int device_address) {
    std::vector<float> gravity(3, 0.0f);

    // BNO055Device device(device_path, device_address);
    if (device.getFileDescriptor() == -1) {
        return gravity;
    }

    unsigned char sensorData[6];
    if (readSensorData(device, GRAVITY_REGISTER, sensorData, sizeof(sensorData)) != 0) {
        return gravity;
    }

    float gravityX, gravityY, gravityZ;
    convertSensorData(sensorData, gravityX, gravityY, gravityZ, GRAVITY_SCALE_FACTOR);

    gravity[0] = gravityX;
    gravity[1] = gravityY;
    gravity[2] = gravityZ;

    return gravity;
}


std::vector<int> getSystemStatus(const char* device_path, int device_address) {
    std::vector<int> status(3, -1);

    // BNO055Device device(device_path, device_address);
    if (device.getFileDescriptor() == -1) {
        return status;
    }

    unsigned char data0[1];
    if (readSensorData(device, SYS_STAT_ADDR, data0, sizeof(data0)) != 0) {
        status[0] = -1;
        return status; // Return the array on error
    }

    unsigned char data1[1];
    if (readSensorData(device, SYS_ERR_ADDR, data1, sizeof(data1)) != 0) {
        status[1] = -1;
        return status; // Return the array on error
    }

    unsigned char data2[1];
    if (readSensorData(device, SELFTEST_RESULT_ADDR, data2, sizeof(data2)) != 0) {
        status[2] = -1;
        return status; // Return the array on error
    }

    status[0] = static_cast<int>(data0[0]);
    status[1] = static_cast<int>(data1[0]);
    status[2] = static_cast<int>(data2[0]);

    return status;
}

void closeDevice() {
    // No need to explicitly close the device since it's managed by RAII (BNO055Device class).
    // The destructor of BNO055Device will close the device automatically when it goes out of scope.
}
