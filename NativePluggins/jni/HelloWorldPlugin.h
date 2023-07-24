#ifndef HELLO_WORLD_PLUGIN_H
#define HELLO_WORLD_PLUGIN_H

#include <cstdio>
#include <fcntl.h>
#include <unistd.h>
#include <linux/i2c-dev.h>
//#include <android/log.h>

extern const int ACCEL_REGISTER;
extern const float ACCEL_SCALE_FACTOR;
extern const int GYRO_REGISTER;
extern const float GYRO_SCALE_FACTOR;
extern const int EULER_REGISTER;
extern const float EULER_SCALE_FACTOR;
extern const int QUATERNION_REGISTER;
extern const float QUATERNION_SCALE_FACTOR;
extern const int WRITE_BUFFER_SIZE;

int setupDevice(const char* device_path, int device_address);
int readRegisterData(int fileDescriptor, unsigned char registerAddress, unsigned char* data, size_t dataSize);
void convertSensorData(int fileDescriptor, int sensorAddress, float& sensorX, float& sensorY, float& sensorZ, float scaleFactor);
void convertQuat(int fileDescriptor, int sensorAddress, float& sensorX, float& sensorY, float& sensorZ, float& sensorW, float scaleFactor);

extern "C" bool setMode(const char* device_path, int device_address, unsigned char register_address, unsigned char mode);
extern "C" float* getEuler(const char* device_path, int device_address);
extern "C" float* getQuaternion(const char* device_path, int device_address);


#endif // HELLO_WORLD_PLUGIN_H
