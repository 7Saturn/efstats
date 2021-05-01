all : efstats.exe
ifeq ($(OS),Windows_NT)
efstats.exe : efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Csv.cs Newtonsoft.Json.dll Incident.cs ConsoleParameters\\ConsoleParameters.cs ConsoleParameters\\Parameter.cs ConsoleParameters\\ParameterDefinition.cs
	C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe -out:efstats.exe -r:Newtonsoft.Json.dll ConsoleParameters\\ConsoleParameters.cs ConsoleParameters\\Parameter.cs ConsoleParameters\\ParameterDefinition.cs efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Incident.cs Csv.cs
clean:
	if exist efstats.exe del efstats.exe
else
efstats.exe : efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Newtonsoft.Json.dll Incident.cs Csv.cs ConsoleParameters/ConsoleParameters.cs ConsoleParameters/Parameter.cs ConsoleParameters/ParameterDefinition.cs
	mcs -out:efstats.exe -r:Newtonsoft.Json.dll ConsoleParameters/ConsoleParameters.cs ConsoleParameters/Parameter.cs ConsoleParameters/ParameterDefinition.cs efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Incident.cs Csv.cs
clean:
	rm -f efstats.exe
endif
pull:
	cd ConsoleParameters; git submodule update --remote --recursive; cd ..
gitinit:
	cd ConsoleParameters; git submodule update --init; cd ..
