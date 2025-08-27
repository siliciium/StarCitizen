#pragma once
#include "Libs/freetrack20enhanced.h"

using namespace System;

namespace freetrack20enhancedwrapper {
	public ref class Freetrack20EnhancedClass
	{
	public:
		Freetrack20EnhancedClass();
		~Freetrack20EnhancedClass();
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
	private : 
		Freetrack20Enhanced* freetrack20enhanced;
	};
}
