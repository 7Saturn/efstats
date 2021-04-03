all : efstats.exe
ifeq ($(OS),Windows_NT)
efstats.exe : efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Newtonsoft.Json.dll Incident.cs
	C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe -out:efstats.exe -r:Newtonsoft.Json.dll efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Incident.cs
clean:
	if exist efstats.exe del efstats.exe
else
efstats.exe : efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Newtonsoft.Json.dll Incident.cs
	mcs -out:efstats.exe -r:Newtonsoft.Json.dll efstats.cs Player.cs PlayerList.cs PlayerMapping.cs Elo.cs Weapons.cs Encounter.cs Incident.cs
clean:
	rm -f efstats.exe
endif
