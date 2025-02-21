# SC Arena Stats (HUD) example

![image](https://github.com/user-attachments/assets/5b8b1a94-ce90-4661-8665-05352f420c94)

*Config available :*
```js
const ws_addr = "ws://localhost:8118/";

const leaderboard_display        = true;
const leaderboard_filter_players = [];
const leaderboard_updateTimeMs   = 0;  // 0 = no refresh
const leaderboard_limit_entries  = 22; // 0 = no limit , 22 = 3440x1440

const killfeed_display           = true;
const killfeed_filter_players    = [];        
const killfeed_updateTimeMs      = 0;  // 0 = no refresh
const killfeed_limit_entries     = 28; // 0 = no limit , 28 = 3440x1440
const killfeed_include_suicide   = false;
const killfeed_include_crash     = false;
```

# SC Arena Stats (OBS overlay) example

![image](https://github.com/user-attachments/assets/a2060ad5-3ba4-4343-982d-8cb44afa6bba)

*Config available :*
```js
const ws_addr = "ws://localhost:8118/";

const leaderboard_filter_players = ["Player1"];        
const leaderboard_display        = true;
const leaderboard_updateTimeMs   = 10000;

const killfeed_display           = true;
const killfeed_filter_players    = ["Player1"];
const killfeed_updateTimeMs      = 10000;    
const killfeed_include_suicide   = false;
const killfeed_include_crash     = false;
const killfeed_limit_entries     = 25;     // 3440x1440
```

# SC Arena Stats (Simple example) example

![image](https://github.com/user-attachments/assets/3e65752d-797f-4aa8-9d83-2952be062d3d)

*Config available :*
```js
const ws_addr = "ws://localhost:8118/";

const leaderboard_display        = true;
const leaderboard_filter_players = [];
const leaderboard_updateTimeMs   = 10000;

const killfeed_display           = true;
const killfeed_filter_players    = [];        
const killfeed_updateTimeMs      = 10000;
const killfeed_include_suicide   = false;
const killfeed_include_crash     = false;
```
