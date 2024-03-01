#!/bin/sh

set -e

AltCover="$(realpath ./packages/altcover.8.6.68/tools/net472/AltCover.exe)"
ReportGenerator="$(realpath ./packages/ReportGenerator.5.1.23/tools/net47/ReportGenerator.exe)"
gendarme="$(realpath ./packages/altcode.gendarme.2023.12.27.19054/tools/gendarme.exe)"
nunit="$(realpath ./packages/NUnit.ConsoleRunner.3.17.0/tools/nunit3-console.exe)"


cd build/Debug/tests
mono $AltCover --assemblyFilter=Moq --typeFilter=PlayersWorlds.Maps.Areas.Evolving.VectorDistanceForceProducer --typeFilter=PlayersWorlds.Maps.Maze.MazeGenerationException --pathFilter=./src/renderers --outputDirectory=__Instrumented
mono $AltCover Runner --recorderDirectory __Instrumented --cobertura=../../coverage.cobertura.xml --executable "$nunit" -- __Instrumented/PlayersWorlds.Maps.Tests.dll --framework=mono-4.0 --where="Category!=Load AND Category!=Integration"
cd ../../..
mono $ReportGenerator -reports:build/coverage.cobertura.xml -targetdir:build/coverage -reporttypes:TextSummary -assemblyfilters:-AltCover.Monitor\;-PlayersWorlds.Maps.Tests
mono $nunit build/Debug/tests/PlayersWorlds.Maps.Tests.dll --framework=mono-4.0 --where="Category!=Load AND Category!=Integration"
for cs_file in $(find src/ -name '*.cs'); do test_file=$(echo $cs_file | sed -e 's/.cs$/Test.cs/' -e 's/^src/tests/'); [ ! -e "$test_file" ] && printf '%s\n' "MISSING TEST CLASS FOR $cs_file"; done
mono $gendarme build/Debug/PlayersWorlds.Maps/PlayersWorlds.Maps.dll --html build/gendarme.html || true
cat build/coverage/Summary.txt
