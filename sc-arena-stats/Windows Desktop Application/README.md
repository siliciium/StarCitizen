![image](https://github.com/user-attachments/assets/93ec314b-09c4-43f4-822c-bfbdafe0d77d)

*This program is not affiliated with the Cloud Imperium group of companies. All content on this site not authored by its host or users are property of their respective owners. Star Citizen®, Roberts Space Industries® and Cloud Imperium® are registered trademarks of Cloud Imperium Rights LLC*

# Presentation
This program opens a Star Citizen log file and retrieves data for `CActor::Kill` ​​and maintains a list of `Kills,Deaths,Suicides` and `Crashes`. When you press `Open` a websocket server is started on port `8118` (can be changed in MainWindow.cs). The server can be queried at any time via a websocket client. The program continuously reads the file to retrieve new entries, you must press `Stop` to stop reading the file and the websocket server.

# Required Tools and Font

- [Visual Studio Community](https://visualstudio.microsoft.com/fr/vs/) (Install)
- [Electrolize Font](https://fonts.google.com/specimen/Electrolize) (Install this font on your Windows system)

# Visual Studio

**1-** Open Visual Studio and create new `Application WPF (.NET Framework)`

![image](https://github.com/user-attachments/assets/31d27fbe-4188-4424-a31f-d306085d12a1)



**2-** Configure the project with `.NET framework 4.7.2`

![image](https://github.com/user-attachments/assets/30deb65b-b794-4b1a-b0ea-0771a2b96e30)



**3-** Add a reference for `System.Text.Json`

![image](https://github.com/user-attachments/assets/e981cee0-07f6-4c93-bae5-4f320e822c0c)

(You can use the search box to find it)
![image](https://github.com/user-attachments/assets/6a0de0dd-2995-4c3a-b817-6e2a2ce31e23)



**4-** Add a new class and enter the name : `WebSocketServer.cs`

![image](https://github.com/user-attachments/assets/83319071-4435-4ff7-b1b9-d8c313752a93)

At this step you need to have :

![image](https://github.com/user-attachments/assets/ddbf0b08-4fb6-4524-94a3-b68adbb31378)


**5-** Copy the class content or replace the class file with : [WebSocketServer.cs](https://github.com/siliciium/StarCitizen/blob/main/sc-arena-stats/Windows%20Desktop%20Application/WebSocketServer.cs)


**6-** Copy the class content or replace the class file with : [MainWindow.xaml.cs](https://github.com/siliciium/StarCitizen/blob/main/sc-arena-stats/Windows%20Desktop%20Application/MainWindow.xaml.cs)


**7-** Copy the XAML content or replace the XAML file with : [MainWindow.xaml.cs](https://github.com/siliciium/StarCitizen/blob/main/sc-arena-stats/Windows%20Desktop%20Application/MainWindow.xaml)

**Optionnal** : add the app [icon](https://github.com/siliciium/StarCitizen/blob/main/sc-arena-stats/Windows%20Desktop%20Application/icon.ico) 
![image](https://github.com/user-attachments/assets/3800ae5a-d3d2-4ac7-8d07-22dc2c4e6c90)


**8-** Now you can run the programm by clicking on green arrow (Start/Démarrer)

![image](https://github.com/user-attachments/assets/42f38d6e-9ecf-4e1e-a93b-cd2b259a3d3c)

