#ifndef FREETRACK20ENHANCED_H
#define FREETRACK20ENHANCED_H
#define _USE_MATH_DEFINES // for C++
#include <cstddef>
#include <cmath>
#include <windows.h>
#include "freetrackclient/fttypes.h"
#include "compat/shm.h"
#include <memory>
#include <iostream>
#include <algorithm>

#include <chrono>
#include <thread>


class Freetrack20Enhanced {
private:
    int intGameID = -1;
    shm_wrapper shm { FREETRACK_HEAP, FREETRACK_MUTEX, sizeof(FTHeap) };
    FTHeap* pMemData { (FTHeap*) shm.ptr() };
    enum Axis : int
    {
        NonAxis = -1,
        TX = 0, TY = 1, TZ = 2,

        Yaw = 3, Pitch = 4, Roll = 5,
        Axis_MIN = TX, Axis_MAX = 5,

        Axis_COUNT = 6,
    };
public:
    Freetrack20Enhanced();
    ~Freetrack20Enhanced();
    bool initialize();
    void center_view();
    void pose(
        const double headpose_pitch,
        const double headpose_yaw,
        const double headpose_roll,
        const double headpose_X,
        const double headpose_Y,
        const double headpose_Z,

        const double raw_pitch,
        const double raw_yaw,
        const double raw_roll,
        const double raw_X,
        const double raw_Y,
        const double raw_Z
    );
};

#endif // FREETRACK20ENHANCED_H
