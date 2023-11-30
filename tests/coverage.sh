#!/bin/sh

set -e

cd build/Debug/tests
mono ~/.nuget/packages/altcover/8.6.68/tools/net472/AltCover.exe --assemblyFilter=Moq --typeFilter=Log --typeFilter=Nour.Play.Areas.Evolving.VectorDistanceForceProducer --pathFilter=./core/renderers --outputDirectory=__Instrumented
mono ~/.nuget/packages/altcover/8.6.68/tools/net472/AltCover.exe Runner --recorderDirectory __Instrumented --cobertura=../../coverage.cobertura.xml --executable nunit3-console -- __Instrumented/tests.dll --where="Category!=Load"
cd ../../..
mono packages/ReportGenerator.5.1.23/tools/net47/ReportGenerator.exe -reports:build/coverage.cobertura.xml -targetdir:build/coverage -reporttypes:TextSummary -assemblyfilters:-AltCover.Monitor\;-tests
nunit3-console build/Debug/tests/tests.dll --where="Category!=Load"
cat build/coverage/Summary.txt
