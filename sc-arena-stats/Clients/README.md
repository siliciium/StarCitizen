# SC Arena Stats (HUD)

![image](https://github.com/user-attachments/assets/b833faa9-2fca-473b-a5aa-a3fcbed463d6)

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

# SC Arena Stats (OBS overlay)

![image](https://github.com/user-attachments/assets/a20d7ee3-9023-4cfc-a482-e5e56864a0be)

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

# SC Arena Stats (Simple example)

![image](https://github.com/user-attachments/assets/fb27c21f-4127-4b83-9aa7-b3ac25e30a74)


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
