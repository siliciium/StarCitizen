#include <windows.h>
#include <shlobj.h>
#include <string>
#include <tchar.h>
#include <iostream>
#include <filesystem>
#include <fstream>
#include <regex>
#include <sstream>
#include <chrono>
#include <ctime>
#include <vector>
#include <map>


// Global variable
HWND hwndButton;
HWND hwndEdit;
HWND hwndEdit2;
HWND hwndLabel;
HWND hwndProgress;
HFONT hfont1;
HFONT hfont2;
HFONT hfont3;
std::map<std::string, std::vector<int>> map_datas;

// Function declarations
void OpenDirectory(HWND hwnd);

bool endsWith(const std::wstring& str, const std::wstring& suffix) {
    return str.size() >= suffix.size() && str.compare(str.size() - suffix.size(), suffix.size(), suffix) == 0;
}

std::wstring stringToLPCWSTR(const std::string& s) {
    int len;
    int slength = (int)s.length() + 1;
    len = MultiByteToWideChar(CP_UTF8, 0, s.c_str(), slength, 0, 0); 
    wchar_t* buf = new wchar_t[len];
    MultiByteToWideChar(CP_UTF8, 0, s.c_str(), slength, buf, len);
    std::wstring r(buf);
    delete[] buf;
    return r;
}

void AddStringToEditControl(HWND hwnd, HWND hwndEdit, const std::wstring& str) {
    SendMessage(hwndEdit, EM_REPLACESEL, TRUE, (LPARAM)str.c_str());
    UpdateWindow(hwnd);
}

void Process_file_optimized(const std::filesystem::path& path, const std::regex& pattern_datetime, const std::regex& pattern_version) {
    std::ifstream file(path);
    std::string first_line;
    std::string last_line;
    std::string line;
    std::string version;

    std::vector<int> vec_secondes;
    bool have_error = false;

    while (std::getline(file, line)) {

        std::smatch matches_version;
        if (std::regex_search(line, matches_version, pattern_version)) {
            if(matches_version.size() == 3){
                version = matches_version[2];                 
            }
            break;            
        }
        
    }

    if(version.length() > 0){
        if (file.is_open()) {

            if (std::getline(file, first_line)) {
                //std::cout << "first_line: " << first_line << std::endl;
            }

            file.seekg(-5, std::ios_base::end);

            int i = -5;
            while (file.peek() != '\n') {
                file.seekg(i, std::ios_base::end);
                i--;
            }

            i+=2;
            file.seekg(i, std::ios_base::end);
            
            std::getline(file, last_line);
            //std::cout << "last_line: " << last_line << std::endl;            
        
            

            std::smatch matches_datetime;
            std::chrono::system_clock::time_point tp_first;
            std::chrono::system_clock::time_point tp_last;

            bool tp_first_found = false;
            bool tp_last_found = false;

            if (std::regex_search(first_line, matches_datetime, pattern_datetime)) {
                for (size_t i = 0; i < matches_datetime.size(); ++i) {
                    std::tm tm = {};
                    std::istringstream ss(matches_datetime[i]);
                    ss >> std::get_time(&tm, "%Y-%m-%dT%H:%M:%S");

                    if (ss.fail()) {
                        MessageBox(NULL, L"Parse datetime failed", L"SC Game Time Calc (by Silicium)", MB_OK);  
                        have_error = true;                     
                    }else{
                        tp_first = std::chrono::system_clock::from_time_t(std::mktime(&tm));  
                        tp_first_found = true;                                      
                    }                                      
                }
            }

            
            if (std::regex_search(last_line, matches_datetime, pattern_datetime)) {
                for (size_t i = 0; i < matches_datetime.size(); ++i) {
                    std::tm tm = {};
                    std::istringstream ss(matches_datetime[i]);
                    ss >> std::get_time(&tm, "%Y-%m-%dT%H:%M:%S");

                    if (ss.fail()) {
                        MessageBox(NULL, L"Parse datetime failed", L"SC Game Time Calc (by Silicium)", MB_OK);   
                        have_error = true;                     
                    }else{
                        tp_last = std::chrono::system_clock::from_time_t(std::mktime(&tm));
                        tp_last_found = true;                                        
                    }                                      
                }
            }


            if(!tp_last_found){                

                while(!tp_last_found && !have_error){

                    i-=3;
                    file.seekg(i, std::ios_base::end);

                    while (file.peek() != '\n') {
                        file.seekg(i, std::ios_base::end);
                        i--;
                    }
                    
                    i+=2;
                    file.seekg(i, std::ios_base::end);                   
                    std::getline(file, last_line);
                    
                    if (std::regex_search(last_line, matches_datetime, pattern_datetime)) {

                        for (size_t i = 0; i < matches_datetime.size(); ++i) {
                            //std::cout << "Match: " << matches[i] << '\n';            
                            std::tm tm = {};
                            std::istringstream ss(matches_datetime[i]);
                            ss >> std::get_time(&tm, "%Y-%m-%dT%H:%M:%S");

                            if (ss.fail()) { 
                                MessageBox(NULL, L"Parse datetime failed", L"SC Game Time Calc (by Silicium)", MB_OK);   
                                have_error = true;         
                            }else{
                                tp_last = std::chrono::system_clock::from_time_t(std::mktime(&tm));
                                tp_last_found = true;                                        
                            }                                      
                        }
                    }else{
                        tp_last_found = false;
                    }

                    if (file.tellg() == 0) {
                        break;
                    }

                }  

            }


            if(tp_first_found && tp_last_found){
              
                long long delta = std::chrono::system_clock::to_time_t(tp_last) - std::chrono::system_clock::to_time_t(tp_first);
                int secondes = static_cast<int>(delta);

                if(map_datas.count(version) == 0){
                    map_datas[version] = vec_secondes;
                }
                map_datas[version].push_back(secondes);  
              
            }
                        

            file.close();

        } else {
            MessageBox(NULL, L"Unable to open file", L"SC Game Time Calc (by Silicium)", MB_OK);
        }
    }

    
}

void Print_format_datetime(HWND hwnd, HWND hwndEdit, int totalSeconds, bool addsign, bool is_negative) {
    int days = totalSeconds / (24 * 3600);
    totalSeconds %= 24 * 3600;
    int hours = totalSeconds / 3600;
    totalSeconds %= 3600;
    int minutes = totalSeconds / 60;
    int seconds = totalSeconds % 60;

    std::wstring sign = L"";
    if(addsign){
        if(is_negative){
            sign = L"+";
        }else{
            sign = L"-";
        }
    }

    std::wstringstream wss;
    wss << sign;
    if(days > 0){
        wss << std::setfill(L'0') << std::setw(2) << days << L" days ";
    }
     
    wss << std::setfill(L'0') << std::setw(2) << hours << L":"
        << std::setfill(L'0') << std::setw(2) << minutes << L":"
        << std::setfill(L'0') << std::setw(2) << seconds;

    // Append the string to the edit control.
    SendMessage(hwndEdit, EM_REPLACESEL, TRUE, (LPARAM)wss.str().c_str());
    UpdateWindow(hwnd);
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
        case WM_CREATE: {
            // Create "Open" button
            hwndButton = CreateWindow(TEXT("button"), TEXT("Select"),
                WS_VISIBLE | WS_CHILD,
                20, 20, 80, 25,
                hwnd, (HMENU) 1, NULL, NULL);

            // Create edit control
            hwndEdit = CreateWindow(TEXT("edit"), NULL,
                WS_VISIBLE | WS_CHILD | WS_BORDER | ES_AUTOHSCROLL,
                120, 20, 420, 25,
                hwnd, (HMENU) 2, NULL, NULL);
            
            // Create edit control        
            hwndEdit2 = CreateWindow(TEXT("edit"), NULL,
                WS_CHILD | WS_VISIBLE | WS_VSCROLL | ES_MULTILINE | WS_BORDER,
                20, 55, 540, 250,
                hwnd, (HMENU) 3, NULL, NULL);        

            // Create a progress bar.
            hwndProgress = CreateWindowEx(
                0, 
                PROGRESS_CLASS, NULL,
                WS_CHILD | WS_VISIBLE | PBS_SMOOTH, 
                20, 305, 525, 25, 
                hwnd, NULL, NULL, NULL);


            // Create a new font.
            hfont1 = CreateFont(20, 0, 0, 0, FW_DONTCARE, FALSE, FALSE, FALSE, DEFAULT_CHARSET,
            OUT_OUTLINE_PRECIS, CLIP_DEFAULT_PRECIS, CLEARTYPE_QUALITY, VARIABLE_PITCH, TEXT("Segoe UI"));
            // Set the new font for the window.
            SendMessage(hwndButton, WM_SETFONT, (WPARAM)hfont1, TRUE);        
            SendMessage(hwndEdit, WM_SETFONT, (WPARAM)hfont1, TRUE);
            

            // Create a new font.
            hfont2 = CreateFont(22, 0, 0, 0, FW_DONTCARE, FALSE, FALSE, FALSE, DEFAULT_CHARSET,
            OUT_OUTLINE_PRECIS, CLIP_DEFAULT_PRECIS, CLEARTYPE_QUALITY, VARIABLE_PITCH, TEXT("Consolas"));        
            SendMessage(hwndEdit2, WM_SETFONT, (WPARAM)hfont2, TRUE);
            break;
        }
        case WM_GETMINMAXINFO:{     
            MINMAXINFO* minMaxInfo = (MINMAXINFO*)lParam;
            minMaxInfo->ptMinTrackSize.x = 595;  // Minimum width
            minMaxInfo->ptMinTrackSize.y = 400;  // Minimum height
            minMaxInfo->ptMaxTrackSize.x = 595;  // Maximum width
            minMaxInfo->ptMaxTrackSize.y = 400;  // Maximum height
            return 0;    
        }
        case WM_COMMAND:{     
            if (LOWORD(wParam) == 1)
            {            
                OpenDirectory(hwnd);
            }
            break;
        }
        case WM_CLOSE:{
            DestroyWindow(hwnd);
            break;
        }
        case WM_DESTROY:{            
            PostQuitMessage(0);
            break;
        }
        case WM_CTLCOLORSTATIC:{   
            HDC hdcStatic = (HDC)wParam; 
            SetBkMode(hdcStatic, TRANSPARENT);
            return (LRESULT)GetStockObject(NULL_BRUSH);
        }
    }

    

    return DefWindowProc(hwnd, msg, wParam, lParam);
}

void Process_Path(HWND hwnd,TCHAR *dir_logpath){

    std::string regex_datetime = "(\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2})";
    std::regex pattern_datetime(regex_datetime);

    std::string regex_version = "(ProductVersion: (\\d{1}\\.\\d{2}))";
    std::regex pattern_version(regex_version);

    int count = 0;
    for (const auto& entry : std::filesystem::directory_iterator(dir_logpath)) {
        if (entry.is_regular_file()) {
            count++;            
        }
    }

    if(count > 0){
        // Set the range and increment of the progress bar.
        SendMessage(hwndProgress, PBM_SETRANGE, 0, MAKELPARAM(0, count));
        SendMessage(hwndProgress, PBM_SETSTEP, (WPARAM)1, 0);

        int n = 0;
        for (const auto& entry : std::filesystem::directory_iterator(dir_logpath)) {
            
            if (entry.is_regular_file()) {

                Process_file_optimized(entry.path(), pattern_datetime, pattern_version);
                SendMessage(hwndProgress, PBM_STEPIT, 0, 0);              

                n++;
            }
                    
        }
        SendMessage(hwndProgress, PBM_SETPOS, 0, 0);


        // Process map_datas ...
        if(map_datas.size() > 0){
            AddStringToEditControl(hwnd ,hwndEdit2, L"\r\n\r\n");
        }

        std::wstring arrow = stringToLPCWSTR(" ➔ ");
        std::string last_version;
        int last_total_sec = 0;
        int all_versions_total_sec = 0;

        int first = 0;
        int last = 0;

        int v = 0;
        for (const auto& pair : map_datas) {        
            //std::cout << pair.first << " ➔ ";
            std::wstring wstr = stringToLPCWSTR(pair.first);
            AddStringToEditControl(hwnd ,hwndEdit2, wstr);            
            AddStringToEditControl(hwnd ,hwndEdit2, arrow.c_str());

            int total_sec = 0;
            for (int value : pair.second) {
                total_sec += value;                
            }

            all_versions_total_sec += total_sec;
            
            Print_format_datetime(hwnd, hwndEdit2, total_sec, false, false);
            
            if(v > 0){
                int diff_sec = last_total_sec - total_sec;            
                //std::cout << "\t" << last_version << " ➔ ";
                AddStringToEditControl(hwnd ,hwndEdit2, L"\t\t");

                bool is_negative = false;
                if(last_total_sec > total_sec){                
                    diff_sec = last_total_sec - total_sec;  
                }else{
                    diff_sec = total_sec - last_total_sec; 
                    is_negative = true; 
                }
                Print_format_datetime(hwnd, hwndEdit2, diff_sec, true, is_negative);
            }
            
            AddStringToEditControl(hwnd ,hwndEdit2, L"\r\n");

            last_version = pair.first;
            last_total_sec = total_sec;

            if(map_datas.size() >= 2){
                if(v == map_datas.size() - 2){
                    first = total_sec;
                }

                if(v == map_datas.size() - 1){
                    last = total_sec;
                }
            } 
                   
            v++;
        }

        AddStringToEditControl(hwnd ,hwndEdit2, L"\r\n");
        AddStringToEditControl(hwnd ,hwndEdit2, L"Total playing time");
        AddStringToEditControl(hwnd ,hwndEdit2, arrow.c_str());
        Print_format_datetime(hwnd, hwndEdit2, all_versions_total_sec, false, false);
        AddStringToEditControl(hwnd ,hwndEdit2, L"\r\n");

        if(map_datas.size() >= 2){            
            AddStringToEditControl(hwnd ,hwndEdit2, L"Previous two major patch");
            AddStringToEditControl(hwnd ,hwndEdit2, arrow.c_str());
            AddStringToEditControl(hwnd ,hwndEdit2, std::to_wstring((first+last)/60/60));
            AddStringToEditControl(hwnd ,hwndEdit2, L" hour(s)");
        }

    }else{
        MessageBox(NULL, L"No log file found in this directory", L"SC Game Time Calc (by Silicium)", MB_OK);
    }
    

    map_datas.clear();
}

void OpenDirectory(HWND hwnd)
{

    SetWindowText(hwndEdit, L"");
    SetWindowText(hwndEdit2, L"");
    UpdateWindow(hwnd);


    BROWSEINFO bi = { 0 };
    bi.lpszTitle = L"Browse for folder...";
    LPITEMIDLIST pidl = SHBrowseForFolder(&bi);

    if (pidl != 0)
    {
        // Get the name of the folder
        TCHAR dir_logpath[MAX_PATH];
        if (SHGetPathFromIDList(pidl, dir_logpath))
        {          
            if(endsWith(dir_logpath, L"logbackups")){
                SetWindowText(hwndEdit, dir_logpath) ;   
                UpdateWindow(hwnd);

                Process_Path(hwnd, dir_logpath);
            }else{
                MessageBox(NULL, L"The selected folder does not seem to be the star citizen logbackups folder.", L"SC Game Time Calc (by Silicium)", MB_OK);
            }
                        
        }

        // Free memory used
        IMalloc *imalloc = 0;
        if (SUCCEEDED(SHGetMalloc(&imalloc)))
        {
            imalloc->Free(pidl);
            imalloc->Release();
        }
    }
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PWSTR pCmdLine, int nCmdShow)
{
    const wchar_t CLASS_NAME[] = L"Sample Window Class";
    WNDCLASS wc = { };

    wc.lpfnWndProc = WndProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = CLASS_NAME;

    RegisterClass(&wc);

    HWND hwnd = CreateWindowEx(0, CLASS_NAME, L"SC Game Time Calc (by Silicium)",
        WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT, 595, 400,
        NULL, NULL, hInstance, NULL);

    if (hwnd == NULL)
    {
        return 0;
    }

    // Create a label.
    hwndLabel = CreateWindowEx(0, TEXT("STATIC"), L"*NOTE : some versions can be on same branch", 
        WS_CHILD | WS_VISIBLE,
        20, 335, 540, 25, 
        hwnd, NULL, hInstance, NULL); 
    
    hfont3 = CreateFont(15, 0, 0, 0, FW_DONTCARE, FALSE, FALSE, FALSE, DEFAULT_CHARSET,
        OUT_OUTLINE_PRECIS, CLIP_DEFAULT_PRECIS, CLEARTYPE_QUALITY, VARIABLE_PITCH, TEXT("Segoe UI"));
        SendMessage(hwndLabel, WM_SETFONT, (WPARAM)hfont3, TRUE);


    // Get the screen dimensions.
    int screenWidth = GetSystemMetrics(SM_CXSCREEN);
    int screenHeight = GetSystemMetrics(SM_CYSCREEN);

    // Get the window dimensions.
    RECT windowRect;
    GetWindowRect(hwnd, &windowRect);
    int windowWidth = windowRect.right - windowRect.left;
    int windowHeight = windowRect.bottom - windowRect.top;

    // Calculate the centered position.
    int posX = (screenWidth - windowWidth) / 2;
    int posY = (screenHeight - windowHeight) / 2;

    // Set the window position.
    SetWindowPos(hwnd, NULL, posX, posY, 0, 0, SWP_NOZORDER | SWP_NOSIZE);

    ShowWindow(hwnd, nCmdShow);

    MSG msg = { };
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    return 0;
}
