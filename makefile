all : build/efstats.exe build/efstats.zip readme/
ifeq ($(OS),Windows_NT)
build/efstats.exe : efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Csv.cs Newtonsoft.Json.dll Incident.cs ConsoleParameters\\src\\ConsoleParameters.cs ConsoleParameters\\src\\Parameter.cs ConsoleParameters\\src\\ParameterDefinition.cs SaveFile.cs
	if not exist build mkdir build
	C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe -out:build\\efstats.exe -r:Newtonsoft.Json.dll ConsoleParameters\\src\\ConsoleParameters.cs ConsoleParameters\\src\\Parameter.cs ConsoleParameters\\src\\ParameterDefinition.cs efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Incident.cs Csv.cs SaveFile.cs
clean:
	if exist build\efstats.exe del build\efstats.exe
	if exist build\Newtonsoft.Json.dll del build\Newtonsoft.Json.dll
	if exist build\efstats.zip del build\efstats.zip
efstats.zip : build/efstats.exe Newtonsoft.Json.dll
	copy Newtonsoft.Json.dll build\\
	tar -cf build\\efstats.zip build\\efstats.exe build\\Newtonsoft.Json.dll readme\\
else
build/efstats.exe : efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Newtonsoft.Json.dll Incident.cs Csv.cs ConsoleParameters/src/ConsoleParameters.cs ConsoleParameters/src/Parameter.cs ConsoleParameters/src/ParameterDefinition.cs SaveFile.cs
	mkdir -p build
	mcs -out:build/efstats.exe -r:Newtonsoft.Json.dll ConsoleParameters/src/ConsoleParameters.cs ConsoleParameters/src/Parameter.cs ConsoleParameters/src/ParameterDefinition.cs efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Incident.cs Csv.cs SaveFile.cs
clean:
	rm -f build/efstats.exe
	rm -f build/Newtonsoft.Json.dll
	rm -f build/efstats.zip
build/efstats.zip : build/efstats.exe Newtonsoft.Json.dll readme/
	cp Newtonsoft.Json.dll build/
	cd build && rm -f efstats.zip && zip -9 -r efstats.zip efstats.exe Newtonsoft.Json.dll ../readme/
endif
pull:
	cd ConsoleParameters; git submodule update --remote --recursive; cd ..
gitinit:
	cd ConsoleParameters; git submodule update --init; cd ..
