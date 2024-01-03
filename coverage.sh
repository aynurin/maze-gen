#!/bin/sh

set -e

cd build/Debug/tests
mono ~/.nuget/packages/altcover/8.6.68/tools/net472/AltCover.exe --assemblyFilter=Moq --typeFilter=Log --typeFilter=PlayersWorlds.Maps.Areas.Evolving.VectorDistanceForceProducer --pathFilter=./src/renderers --outputDirectory=__Instrumented
mono ~/.nuget/packages/altcover/8.6.68/tools/net472/AltCover.exe Runner --recorderDirectory __Instrumented --cobertura=../../coverage.cobertura.xml --executable nunit3-console -- __Instrumented/tests.dll --where="Category!=Load"
cd ../../..
mono packages/ReportGenerator.5.1.23/tools/net47/ReportGenerator.exe -reports:build/coverage.cobertura.xml -targetdir:build/coverage -reporttypes:TextSummary -assemblyfilters:-AltCover.Monitor\;-tests
nunit3-console build/Debug/tests/tests.dll --where="Category!=Load"
packages/altcode.gendarme.2023.12.27.19054/tools/gendarme.exe build/Debug/PlayersWorlds.Maps/PlayersWorlds.Maps.dll --html build/gendarme.html || cat build/coverage/Summary.txt
