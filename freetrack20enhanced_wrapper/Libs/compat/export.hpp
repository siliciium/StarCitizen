// generates export.hpp for each module from compat/linkage.hpp

#pragma once

#include "linkage-macros.hpp"

#define BUILD_COMPAT 1

#ifdef BUILD_COMPAT
#   define OTR_COMPAT_EXPORT OTR_GENERIC_EXPORT
#else
#   define OTR_COMPAT_EXPORT OTR_GENERIC_IMPORT
#endif
