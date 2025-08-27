# Implement FreeTrack 2.0 Enhanced Protocol in your .NET project
C++/CLI code that wraps native C++ for .NET

_CRL : C++/CLI Runtime Library, C++/CLI is an extension of C++ to interact with the .NET Framework_

**NOTE** : This project use some parts of this related project [OpenTrack](https://github.com/opentrack/opentrack)

# Required for Star Citizen game
[NPClient64.dll](https://github.com/opentrack/opentrack/blob/master/bin/NPClient64.dll)

How it's work ? 
- When you run Star Citizen game , the game search this registry key : `HKEY_CURRENT_USER\Software\NaturalPoint\NATURALPOINT\NPClient Location`, this contain the directory of `NPClient64.dll`. If exists, Star Citizen load the library.
- Using this provided wrapper, you can communicate with the loaded library.

# Disclaimer
_This program is not affiliated with the Cloud Imperium group of companies. All content on this site not authored by its host or users are property of their respective owners. Star Citizen®, Roberts Space Industries® and Cloud Imperium® are registered trademarks of Cloud Imperium Rights LLC_
