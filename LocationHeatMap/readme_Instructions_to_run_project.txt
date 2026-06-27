Instruction on running this location tracker code:

Make sure to open a simulator android device before running the command, i used android studio to simulate pikexl8 android device.

Copy the whole folder to the pc and open it through the visual studio code and give the 
Below command to run it  "dotnet build -t:Run -f net10.0-android" , before this add "googleAPIkey" in the last line to the 
"AndroidManifest.xml" in platforms ->android folder.


Prerequisites:

NET 10 SDK: dot net –version should show 10.0.x
JDK 21-Mandatory
Android Studio for simulation
MAUI Android workload: 
Commands: dotnet workload install maui-android
On macOS: sudo dotnet workload install maui-android

Google API key:
•	Go to https://console.cloud.google.com/→ create/select a project → enable "Maps SDK for Android"
•	Create an API key (APIs & Services → Credentials → Create Credentials → API Key)
•	Paste it into the manifest, replacing the placeholder


