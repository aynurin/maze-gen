#!/bin/sh

set -e

reset
msbuild

until mono packages/NUnit.ConsoleRunner.3.17.0/tools/nunit3-console.exe build/Debug/tests/PlayersWorlds.Maps.Tests.dll --framework=mono-4.0 "$@"; do :; done