#include "pch.h"
#include "freetrack20enhanced_wrapper.h"

namespace freetrack20enhancedwrapper {
	Freetrack20EnhancedClass::Freetrack20EnhancedClass() {
		freetrack20enhanced = new Freetrack20Enhanced();
	}

	Freetrack20EnhancedClass::~Freetrack20EnhancedClass() {
		delete freetrack20enhanced;
	}

	bool Freetrack20EnhancedClass::initialize() {
		return freetrack20enhanced->initialize();
	}

	void Freetrack20EnhancedClass::center_view() {
		freetrack20enhanced->center_view();
	}

	void Freetrack20EnhancedClass::pose(
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
	){
		freetrack20enhanced->pose(
			headpose_pitch,
			headpose_yaw,
			headpose_roll,
			headpose_X,
			headpose_Y,
			headpose_Z,
			raw_pitch,
			raw_yaw,
			raw_roll,
			raw_X,
			raw_Y,
			raw_Z
		);
	}
}

