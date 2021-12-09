all : build/efstats.exe build/efstats.zip readme/
ifeq ($(OS),Windows_NT)
CP=src\\ConsoleParameters\\src\\
build/efstats.exe : src\\efstats.cs src\\Player.cs src\\PlayerList.cs src\\PlayerMapping.cs src\\Elo.cs src\\Weapons.cs src\\Encounter.cs src\\Csv.cs src\\Newtonsoft.Json.dll src\\Incident.cs $(CP)ConsoleParameters.cs $(CP)Parameter.cs $(CP)ParameterDefinition.cs src\\SaveFile.cs
	if not exist build mkdir build
	C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe -out:build\\efstats.exe -r:src\\Newtonsoft.Json.dll $(CP)ConsoleParameters.cs $(CP)Parameter.cs $(CP)ParameterDefinition.cs src\\efstats.cs src\\Player.cs src\\PlayerList.cs src\\PlayerMapping.cs src\\Elo.cs src\\Weapons.cs src\\Encounter.cs src\\Incident.cs src\\Csv.cs src\\SaveFile.cs -win32icon:graphics\\ef_logo_256.ico
clean:
	if exist build\efstats.exe del build\efstats.exe
	if exist build\Newtonsoft.Json.dll del build\Newtonsoft.Json.dll
	if exist build\efstats.zip del build\efstats.zip
efstats.zip : build/efstats.exe src/Newtonsoft.Json.dll
	copy src\\Newtonsoft.Json.dll build\\
	tar -cf build\\efstats.zip build\\efstats.exe build\\Newtonsoft.Json.dll readme\\
else
CP=src/ConsoleParameters/src/
build/efstats.exe : src/efstats.cs\
  src/Player.cs\
  src/PlayerList.cs\
  src/PlayerMapping.cs\
  src/Elo.cs\
  src/Weapons.cs\
  src/Encounter.cs\
  src/Newtonsoft.Json.dll\
  src/Incident.cs\
  src/Csv.cs\
  src/SaveFile.cs\
  $(CP)ConsoleParameters.cs\
  $(CP)Parameter.cs\
  $(CP)ParameterDefinition.cs
	mkdir -p build
	mcs -out:build/efstats.exe -r:src/Newtonsoft.Json.dll\
      $(CP)ConsoleParameters.cs\
      $(CP)Parameter.cs\
      $(CP)ParameterDefinition.cs\
      src/efstats.cs\
      src/Player.cs\
      src/PlayerList.cs\
      src/PlayerMapping.cs\
      src/Elo.cs\
      src/Weapons.cs\
      src/Encounter.cs\
      src/Incident.cs\
      src/Csv.cs\
      src/SaveFile.cs\
    -win32icon:graphics/ef_logo_256.ico
clean:
	rm -f build/efstats.exe
	rm -f build/Newtonsoft.Json.dll
	rm -f build/efstats.zip
build/efstats.zip : build/efstats.exe src/Newtonsoft.Json.dll readme/
	cp src/Newtonsoft.Json.dll build/
	cd build && rm -f efstats.zip && zip -9 -r efstats.zip efstats.exe Newtonsoft.Json.dll
	zip -9 -r build/efstats.zip readme/
endif
pull:
	cd src/ConsoleParameters; git submodule update --remote --recursive; cd ..
gitinit:
	cd src/ConsoleParameters; git submodule update --init; cd ..
