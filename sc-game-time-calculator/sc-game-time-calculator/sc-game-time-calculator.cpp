#include <windows.h>
#include <iostream>
#include <fstream>
#include <string>
#include <regex>
#include <filesystem>
#include <sstream>
#include <chrono>
#include <ctime>
#include <vector>
#include <map>


std::map<std::string, std::vector<int>> map_datas;
std::vector<std::string> list_skip_versions;

class ArgumentParser {
    public:
        ArgumentParser(int &argc, char **argv){
            for (int i=1; i < argc; ++i)
                this->tokens.push_back(std::string(argv[i]));
        }

        bool argumentExists(const std::string &option) const{
            return std::find(this->tokens.begin(), this->tokens.end(), option)
                != this->tokens.end();
        }

        std::string getArgumentValue(const std::string &option) const{
            std::vector<std::string>::const_iterator itr;
            itr =  std::find(this->tokens.begin(), this->tokens.end(), option);
            if (itr != this->tokens.end() && ++itr != this->tokens.end()){
                return *itr;
            }
            return "";
        }

    private:
        std::vector <std::string> tokens;
};

std::string replace(std::string source, std::string from, std::string to){
    std::string copy_source = source;
    size_t start_pos = copy_source.find(from);
    if(start_pos != std::string::npos) {
        copy_source.replace(start_pos, from.length(), to);
    }
    return copy_source;
}

std::vector<std::string> splitStringToVector(const std::string& str) {
    std::vector<std::string> result;
    std::stringstream ss(str);
    std::string item;

    while (std::getline(ss, item, ',')) {
        result.push_back(item);
    }

    return result;
}

void process_file(const std::filesystem::path& path, const std::regex& pattern_datetime, const std::regex& pattern_version) {
    std::ifstream file(path);
    if (!file) {
        std::cerr << "Failed to open file: " << path << '\n';
        return;
    }


    std::vector<std::chrono::system_clock::time_point> time_points;
    std::vector<int> vec_secondes;
    std::string version;

    std::string line;

    while (std::getline(file, line)) {

        std::smatch matches_version;
        if (std::regex_search(line, matches_version, pattern_version)) {
            if(matches_version.size() == 3){
                version = matches_version[2];

                if (std::find(list_skip_versions.begin(), list_skip_versions.end(), version) != list_skip_versions.end()) {
                    break;
                } 
            }
            
        }

        std::smatch matches_datetime;
        if (std::regex_search(line, matches_datetime, pattern_datetime)) {
            for (size_t i = 0; i < matches_datetime.size(); ++i) {
                //std::cout << "Match: " << matches[i] << '\n';

                std::tm tm = {};
                std::istringstream ss(matches_datetime[i]);
                ss >> std::get_time(&tm, "%Y-%m-%dT%H:%M:%S");

                if (ss.fail()) {
                    std::cout << "Parse datetime failed\n";                    
                }else{
                    std::chrono::system_clock::time_point tp = std::chrono::system_clock::from_time_t(std::mktime(&tm));                
                    time_points.push_back(tp);
                }                                      
            }
        }
    }

    

    if(time_points.size() >= 2){
        std::chrono::system_clock::time_point start_pt = time_points.front();
        std::chrono::system_clock::time_point end_pt = time_points.back();
        long long delta = std::chrono::system_clock::to_time_t(end_pt) - std::chrono::system_clock::to_time_t(start_pt);
        
        //std::time_t start_tt = std::chrono::system_clock::to_time_t(start_pt);
        //std::cout << "start_tt:" << start_tt << std::endl;
        //std::time_t end_tt = std::chrono::system_clock::to_time_t(end_pt);
        //std::cout << "end_tt:" << end_tt << std::endl;
        
        int secondes = static_cast<int>(delta);
        vec_secondes.push_back(secondes);
         
    }

    if(version.length() > 0 && (std::find(list_skip_versions.begin(), list_skip_versions.end(), version) == list_skip_versions.end()) ){

        if(map_datas.count(version) == 0){
            map_datas[version] = vec_secondes;
        }else{
            for (int value : vec_secondes) {
                map_datas[version].push_back(value);
            }
        } 

    }

    time_points.clear();
    vec_secondes.clear();
}

void process_file_optimized(const std::filesystem::path& path, const std::regex& pattern_datetime, const std::regex& pattern_version) {
    std::ifstream file(path);
    std::string first_line;
    std::string last_line;
    std::string line;
    std::string version;

    std::vector<int> vec_secondes;

    while (std::getline(file, line)) {

        std::smatch matches_version;
        if (std::regex_search(line, matches_version, pattern_version)) {
            if(matches_version.size() == 3){
                version = matches_version[2];
                if (std::find(list_skip_versions.begin(), list_skip_versions.end(), version) != list_skip_versions.end()) {
                    version="";
                    break;
                } 
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
                        std::cout << "Parse datetime failed\n";                    
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
                        std::cout << "Parse datetime failed\n";                    
                    }else{
                        tp_last = std::chrono::system_clock::from_time_t(std::mktime(&tm));
                        tp_last_found = true;                                        
                    }                                      
                }
            }


            if(!tp_last_found){

                while(!tp_last_found){

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

                        //std::cout << "last_line: " << last_line.length() << ": " << last_line << std::endl;

                        for (size_t i = 0; i < matches_datetime.size(); ++i) {
                            //std::cout << "Match: " << matches[i] << '\n';            
                            std::tm tm = {};
                            std::istringstream ss(matches_datetime[i]);
                            ss >> std::get_time(&tm, "%Y-%m-%dT%H:%M:%S");

                            if (ss.fail()) {
                                std::cout << "Parse datetime failed\n";                    
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
                //std::cout << delta << std::endl;

                if(map_datas.count(version) == 0){
                    map_datas[version] = vec_secondes;
                }
                map_datas[version].push_back(secondes);                                     
            }
                        

            file.close();

        } else {
            std::cout << "Unable to open file" << std::endl;
        }
    }

    
}

void print_ban(HANDLE hConsole){
    SetConsoleTextAttribute(hConsole, 8);
    std::cout <<  "" << std::endl;  
    std::cout <<  "  ______ _______" << std::endl;           
    std::cout <<  " / _____|_______)                                _  (_)" << std::endl;               
    std::cout <<  "( (____  _           ____ _____ ____  _____    _| |_ _ ____  _____" << std::endl;    
    std::cout <<  " \\____ \\| |         / _  (____ |    \\| ___ |  (_   _) |    \\| ___ |" << std::endl;   
    std::cout <<  " _____) ) |_____   ( (_| / ___ | | | | ____|    | |_| | | | | ____|" << std::endl;   
    std::cout <<  "(______/ \\______)   \\___ \\_____|_|_|_|_____)     \\__)_|_|_|_|_____)" << std::endl;   
    std::cout <<  "                   (_____| by Silicium" << std::endl << std::endl;    
    SetConsoleTextAttribute(hConsole, 15);
}

void print_error(HANDLE hConsole, std::string desc, std::string detail){
    SetConsoleTextAttribute(hConsole, 12);
    std::cout << desc << detail <<  std::endl;   
    SetConsoleTextAttribute(hConsole, 15);
}

void print_format_datetime(int totalSeconds, bool addsign, bool is_negative){
    int days = totalSeconds / (24 * 3600);
    totalSeconds %= 24 * 3600;
    int hours = totalSeconds / 3600;
    totalSeconds %= 3600;
    int minutes = totalSeconds / 60;
    int seconds = totalSeconds % 60;


    std::string sign = "";
    if(addsign){
        if(is_negative){
            sign = "+";
        }else{
            sign = "-";
        }
    }
    

    std::cout << sign;
    if(days > 0){
        std::cout << std::setfill('0') << std::setw(2) << days << " days ";
    }
     
    std::cout << std::setfill('0') << std::setw(2) << hours << ":"
        << std::setfill('0') << std::setw(2) << minutes << ":"
        << std::setfill('0') << std::setw(2) << seconds;
}

int main(int argc, char **argv) {    
    ArgumentParser input(argc, argv);
    SetConsoleOutputCP(CP_UTF8);
    HANDLE hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    system("cls");
    
    print_ban(hConsole);

    std::filesystem::path dir_logpath;

    if(input.argumentExists("--logpath")){
        std::string logpath = input.getArgumentValue("--logpath");

        dir_logpath = logpath;
        if (std::filesystem::exists(dir_logpath)) {

            SetConsoleTextAttribute(hConsole, 8);
            std::cout << "Star Citizen log path ➔ " << replace(dir_logpath.string(), "A:/StarCitizen", "*********") << std::endl;
            SetConsoleTextAttribute(hConsole, 15);

        }else{
            std::cout << "ERROR : Invalid path " << dir_logpath <<  std::endl;
            print_error(hConsole, "ERROR : Invalid path ", dir_logpath.string());
            CloseHandle(hConsole);
            return 1;
        }
        
    }else{        
        print_error(hConsole, "ERROR : Missing --logpath ", "YOUR_PATH");
        CloseHandle(hConsole);
        return 1;
    }


    if(input.argumentExists("--skip")){
        std::string versions = input.getArgumentValue("--skip");
        list_skip_versions = splitStringToVector(versions);
    }

    
   
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

    int n = 0;
    for (const auto& entry : std::filesystem::directory_iterator(dir_logpath)) {

        
        if (entry.is_regular_file()) {
            SetConsoleTextAttribute(hConsole, 3);
            std::cout << "\rAnalysis " << (n+1) << "/" << count << " file(s)";
            SetConsoleTextAttribute(hConsole, 15);

            process_file_optimized(entry.path(), pattern_datetime, pattern_version);            
            n++;
        }
        
        
    }

    std::cout << "\r\n" <<  std::endl;

    std::string last_version;
    int last_total_sec = 0;
    int all_versions_total_sec = 0;

    int first = 0;
    int last = 0;


    int v = 0;
    for (const auto& pair : map_datas) {        
        SetConsoleTextAttribute(hConsole, 1);
        std::cout << pair.first << " ➔ ";
        SetConsoleTextAttribute(hConsole, 3);

        int total_sec = 0;
        for (int value : pair.second) {
            total_sec += value;
        }

        all_versions_total_sec += total_sec;
                
        print_format_datetime(total_sec, false, false);
        
        if(v > 0){
            SetConsoleTextAttribute(hConsole, 1);
            int diff_sec = last_total_sec - total_sec;            
            //std::cout << "\t" << last_version << " ➔ ";
            std::cout << "\t " ;
            SetConsoleTextAttribute(hConsole, 3);            

            bool is_negative = false;
            if(last_total_sec > total_sec){                
                diff_sec = last_total_sec - total_sec;  
            }else{
                diff_sec = total_sec - last_total_sec; 
                is_negative = true; 
            }
            print_format_datetime(diff_sec, true, is_negative);
        }
        
        std::cout << std::endl;

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

    SetConsoleTextAttribute(hConsole, 1);    
    std::cout << std::endl << std::endl << "Total playing time ➔ ";
    SetConsoleTextAttribute(hConsole, 3);
    print_format_datetime(all_versions_total_sec, false, false);
    SetConsoleTextAttribute(hConsole, 1);
    std::cout << std::endl;

    if(map_datas.size() >= 2){
        std::cout << "Previous two major patch ➔ ";
        SetConsoleTextAttribute(hConsole, 3);
        std::cout << (first+last)/60/60 ;
        SetConsoleTextAttribute(hConsole, 1);
        std::cout << " hour(s)" << std::endl;
    }

    std::cout << std::endl << std::endl << "*NOTE : some versions can be on same branch" << std::endl;
    
    SetConsoleTextAttribute(hConsole, 15);


    std::cout << std::endl << std::endl;
    
    map_datas.clear(); 
    CloseHandle(hConsole);
 

    return EXIT_SUCCESS;
}


