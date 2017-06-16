## Overview
This is a command line progam that will query the local symcli database and generate a text file with all of the cleanup commands for VMAX2 meta devices. The application utilizes the Solutions Enabler offline mode to generate the script on your local machine without affecting your Unisphere/Solutions Enabler server. The current version of the script performs the following:

1. Warns if a device is not a meta devices.
2. Warns if a device is mapped or masked. At this time you will have to clean this up manually.
3. Warns if a device is part of an RDF session. At this time you will have to clean this up manually.
4. Warns if a device is part of a TimeFinder session. At this time you will have to clean this up manually.
5. Generates the pool unbind, meta dissolve, and device delete commands.
6. Provides a text file with preview commands, and a seperate text file with commit commands.


## Setup

1. Make sure your operating system is Windows 8 or higher.
2. Make sure you have the Microsoft .NET Framework 4.5.2 or higher installed.
3. Install the same version of Solutions Enabler as your Unisphere/Solutions Enabler server. If your Unisphere server is running 7.6.2 then install 7.6.2 on your local machine. Leave all the defaults during the installation.
4. Copy the symapi_db.bin from your Unisphere/Solutions Enabler server to your local machine. Copy it from C:\Program Files\EMC\SYMAPI\db and store it on your local machine in the same location.
5. Download GenerateMetaDestroyScript.exe to your local machine.


## Usage
The GenerateMetaDestroyScript.exe program is run from command line, and expects two arguements. Arguement 1 is the Serial Number of the VMAX, the last for digits of the S/N is enough. Arguement 2 is a comma seperated list of the metadevices you want commands created for.

1. Open a command line window and navigate to the location of the  GenerateMetaDestroyScript.exe.

2. Run the command with the two required arugements: ```GenerateMetaDestroyScript.exe "SN here" "Meta List Here"```

3. For example if you wanted to delete 3 meta devices from VMAX5021 you would run the following: ```GenerateMetaDestroyScript.exe "5021" "0A7C,0A7F,0A8C"```

3. GenerateMetaDestroyScript.exe will generate two text files on your desktop. One will contain the symconfigure preview commands, the other the symconfigure commit commands.


## Licensing
Copyright (c) 2017 Ryan Murphy

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.