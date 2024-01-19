#!/bin/sh

set -e

alias AltCover="mono $(realpath ./packages/altcover.8.6.68/tools/net472/AltCover.exe)"
alias ReportGenerator="mono $(realpath ./packages/ReportGenerator.5.1.23/tools/net47/ReportGenerator.exe)"
alias gendarme="mono $(realpath ./packages/altcode.gendarme.2023.12.27.19054/tools/gendarme.exe)"

cd build/Debug/tests
AltCover --assemblyFilter=Moq --typeFilter=PlayersWorlds.Maps.Areas.Evolving.VectorDistanceForceProducer --typeFilter=PlayersWorlds.Maps.Maze.MazeGenerationException --pathFilter=./src/renderers --outputDirectory=__Instrumented
AltCover Runner --recorderDirectory __Instrumented --cobertura=../../coverage.cobertura.xml --executable nunit3-console -- __Instrumented/PlayersWorlds.Maps.Tests.dll --where="Category!=Load"
cd ../../..
ReportGenerator -reports:build/coverage.cobertura.xml -targetdir:build/coverage -reporttypes:TextSummary -assemblyfilters:-AltCover.Monitor\;-PlayersWorlds.Maps.Tests
nunit3-console build/Debug/tests/PlayersWorlds.Maps.Tests.dll --where="Category!=Load"
for cs_file in $(find src/ -name '*.cs'); do test_file=$(echo $cs_file | sed -e 's/.cs$/Test.cs/' -e 's/^src/tests/'); [ ! -e "$test_file" ] && printf '%s\n' "MISSING TEST CLASS FOR $cs_file"; done
gendarme build/Debug/PlayersWorlds.Maps/PlayersWorlds.Maps.dll --html build/gendarme.html || true
cat build/coverage/Summary.txt
