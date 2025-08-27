#include "freetrack20enhanced.h"

Freetrack20Enhanced::Freetrack20Enhanced(){
    std::cout << "<freetrack20enhanced::freetrack20enhanced>" << std::endl;
}

Freetrack20Enhanced::~Freetrack20Enhanced(){
    center_view();
    std::cout << "<freetrack20enhanced::~freetrack20enhanced" << std::endl;
}

static_assert(sizeof(LONG) == sizeof(std::int32_t));
static_assert(sizeof(LONG) == 4u);

never_inline void store(float volatile& place, const float value)
{
    union
    {
        float f32;
        LONG i32;
    } value_ {};

    value_.f32 = value;

    static_assert(sizeof(value_) == sizeof(float));
    static_assert(offsetof(decltype(value_), f32) == offsetof(decltype(value_), i32));

    (void)InterlockedExchange((LONG volatile*)&place, value_.i32);
}

template<typename t>
static void store(t volatile& place, t value)
{
    static_assert(sizeof(t) == 4u);
    (void)InterlockedExchange((LONG volatile*) &place, (LONG)value);
}

static std::int32_t load(std::int32_t volatile& place)
{
    return InterlockedCompareExchange((volatile LONG*) &place, 0, 0);
}

bool getGameData(int id, unsigned char* table)
{
    for (int i = 0; i < 8; i++)
        table[i] = 0;

    unsigned tmp[8];
    unsigned fuzz[3];

    // 717;
    // Star Citizen;
    // FreeTrack20;
    // V170;
    // ;
    // ;
    // 3450; International ID
    // 02CDF4CE4E343EC6B4A200 FaceTrackNoIR ID
    const char * id_cstr = "02CDF4CE4E343EC6B4A200";

    sscanf(id_cstr,
        "%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x%02x",
        fuzz + 2,
        fuzz + 0,
        tmp + 3, tmp + 2, tmp + 1, tmp + 0,
        tmp + 7, tmp + 6, tmp + 5, tmp + 4,
        fuzz + 1);

    using uchar = unsigned char;
    for (int i = 0; i < 8; i++)
        table[i] = uchar(tmp[i]);

    return true;
}

/*void Freetrack20Enhanced::pose(const double* headpose, const double* raw)
{
    constexpr double d2r = M_PI/180;

    const float yaw = float(-headpose[Yaw] * d2r);
    const float roll = float(headpose[Roll] * d2r);
    const float tx = float(headpose[TX] * 10);
    const float ty = float(headpose[TY] * 10);
    const float tz = float(headpose[TZ] * 10);

    // HACK: Falcon BMS makes a "bump" if pitch is over the value -sh 20170615
    const bool is_crossing_90 = std::fabs(headpose[Pitch] - 90) < .15;
    const float pitch = float(-d2r * (is_crossing_90 ? 89.86 : headpose[Pitch]));

    FTHeap* const ft = pMemData;
    FTData* const data = &ft->data;

    store(data->X, tx);
    store(data->Y, ty);
    store(data->Z, tz);

    store(data->Yaw, yaw);
    store(data->Pitch, pitch);
    store(data->Roll, roll);

    store(data->RawYaw, float(-raw[Yaw] * d2r));
    store(data->RawPitch, float(raw[Pitch] * d2r));
    store(data->RawRoll, float(raw[Roll] * d2r));
    store(data->RawX, float(raw[TX] * 10));
    store(data->RawY, float(raw[TY] * 10));
    store(data->RawZ, float(raw[TZ] * 10));

    const std::int32_t id = load(ft->GameID);

    if (intGameID != id)
    {
        
        union  {
            unsigned char table[8];
            std::int32_t ints[2];
        } t {};

        t.ints[0] = 0; t.ints[1] = 0;

        (void)getGameData(id, t.table);

        {
            // FTHeap pMemData happens to be aligned on a page boundary by virtue of
            // memory mapping usage (MS Windows equivalent of mmap(2)).
            static_assert((offsetof(FTHeap, table) & (sizeof(LONG)-1)) == 0);

            for (unsigned k = 0; k < 2; k++)
                store(pMemData->table_ints[k], t.ints[k]);
        }

        store(ft->GameID2, id);
        store(data->DataID, 0u);

        intGameID = id;

    }
    else
        (void)InterlockedAdd((LONG volatile*)&data->DataID, 1);
}*/

void Freetrack20Enhanced::pose(
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
) {

    constexpr double d2r = M_PI / 180;

    const float yaw = float(-headpose_yaw * d2r);
    const float roll = float(headpose_roll * d2r);
    const float tx = float(headpose_X * 10);
    const float ty = float(headpose_Y * 10);
    const float tz = float(headpose_Z * 10);

    // HACK: Falcon BMS makes a "bump" if pitch is over the value -sh 20170615
    const bool is_crossing_90 = std::fabs(headpose_pitch - 90) < .15;
    const float pitch = float(-d2r * (is_crossing_90 ? 89.86 : headpose_pitch));

    FTHeap* const ft = pMemData;
    FTData* const data = &ft->data;

    store(data->X, tx);
    store(data->Y, ty);
    store(data->Z, tz);

    store(data->Yaw, yaw);
    store(data->Pitch, pitch);
    store(data->Roll, roll);

    store(data->RawYaw, float(-raw_yaw * d2r));
    store(data->RawPitch, float(raw_pitch * d2r));
    store(data->RawRoll, float(raw_roll * d2r));
    store(data->RawX, float(raw_X * 10));
    store(data->RawY, float(raw_Y * 10));
    store(data->RawZ, float(raw_Z * 10));

    const std::int32_t id = load(ft->GameID);

    if (intGameID != id)
    {

        union {
            unsigned char table[8];
            std::int32_t ints[2];
        } t{};

        t.ints[0] = 0; t.ints[1] = 0;

        (void)getGameData(id, t.table);

        {
            // FTHeap pMemData happens to be aligned on a page boundary by virtue of
            // memory mapping usage (MS Windows equivalent of mmap(2)).
            static_assert((offsetof(FTHeap, table) & (sizeof(LONG) - 1)) == 0);

            for (unsigned k = 0; k < 2; k++)
                store(pMemData->table_ints[k], t.ints[k]);
        }

        store(ft->GameID2, id);
        store(data->DataID, 0u);

        intGameID = id;

    }
    else
        (void)InterlockedAdd((LONG volatile*)&data->DataID, 1);
}


void Freetrack20Enhanced::center_view(){
    store(pMemData->data.X, 0.);
    store(pMemData->data.Y, 0.);
    store(pMemData->data.Z, 0.);
    store(pMemData->data.Yaw, 0.);
    store(pMemData->data.Pitch, 0.);
    store(pMemData->data.Roll, 0.);

    store(pMemData->data.X1, float(100));
    store(pMemData->data.Y1, float(200));
    store(pMemData->data.X2, float(300));
    store(pMemData->data.Y2, float(200));
    store(pMemData->data.X3, float(300));
    store(pMemData->data.Y3, float(100));
}


bool Freetrack20Enhanced::initialize(){

    if (!shm.success())
    {
        std::cout << "<Freetrack20Enhanced::initialize> Can't load freetrack memory mapping" << std::endl;
        return false;

    }else{
        std::cout << "<Freetrack20Enhanced::initialize> freetrack memory mapping successfully loaded" << std::endl;

        pMemData->data.DataID = 1;
        pMemData->data.CamWidth = 100;
        pMemData->data.CamHeight = 250;

        #if 1
            center_view();
        #endif

        store(pMemData->GameID2, 0);

        for (unsigned k = 0; k < 2; k++){
            store(pMemData->table_ints[k], 0);
        }
            
    }

    std::cout << "<Freetrack20Enhanced::initialize> success" << std::endl;
    return true;
}
