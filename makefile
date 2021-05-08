all : efstats.exe efstats.zip
ifeq ($(OS),Windows_NT)
efstats.exe : efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Csv.cs Newtonsoft.Json.dll Incident.cs ConsoleParameters\\ConsoleParameters.cs ConsoleParameters\\Parameter.cs ConsoleParameters\\ParameterDefinition.cs SaveFile.cs
	C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe -out:efstats.exe -r:Newtonsoft.Json.dll ConsoleParameters\\ConsoleParameters.cs ConsoleParameters\\Parameter.cs ConsoleParameters\\ParameterDefinition.cs efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Incident.cs Csv.cs SaveFile.cs
clean:
	if exist efstats.exe del efstats.exe
	if exist efstats.zip del efstats.zip
efstats.zip : efstats.exe Newtonsoft.Json.dll
	tar -cf efstats.zip efstats.exe Newtonsoft.Json.dll
else
efstats.exe : efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Newtonsoft.Json.dll Incident.cs Csv.cs ConsoleParameters/ConsoleParameters.cs ConsoleParameters/Parameter.cs ConsoleParameters/ParameterDefinition.cs SaveFile.cs
	mcs -out:efstats.exe -r:Newtonsoft.Json.dll ConsoleParameters/ConsoleParameters.cs ConsoleParameters/Parameter.cs ConsoleParameters/ParameterDefinition.cs efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Incident.cs Csv.cs SaveFile.cs
clean:
	rm -f efstats.exe
	rm -f efstats.zip
efstats.zip : efstats.exe Newtonsoft.Json.dll
	zip -9 efstats.zip efstats.exe Newtonsoft.Json.dll
endif
pull:
	cd ConsoleParameters; git submodule update --remote --recursive; cd ..
gitinit:
	cd ConsoleParameters; git submodule update --init; cd ..
