# EnhancedMap
A free Ultima Online map tool


[Site](http://razorenhanced.org/)

Download [Latest EnhancedMap](http://razorenhanced.org/download/Enhanced-updater.zip)


# How can i compile EnhancedMapServer for my server machine?
1. Install .NET Core 2.1:

   - Windows: https://www.microsoft.com/net/download/windows
   - Linux: https://www.microsoft.com/net/download/linux
   - macOS: https://www.microsoft.com/net/download/macos
   
 2. Open CMD and write ([here](https://github.com/dotnet/docs/blob/master/docs/core/rid-catalog.md) you can see all platforms available. Here im compiling for a Windows 10 x64 machine): 
   ```
   cd "{ENHANCEDMAPSERVERNETCORE_FOLDER}"
   dotnet publish -c Release -r win10-x64
   ```
   
 3. Navigate to "ENHANCEDMAPSERVERNETCORE_FOLDER/bin/Release/netcoreapp2.1/{YOUR_PLATFORM}/publish/"
 
 4. Run EnhancedMapServerNetCore.exe (if you are using windows, instead use terminal for linux/macos)
