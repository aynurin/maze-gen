#!/bin/sh

set -e

msbuild

mono build/Debug/mazegen/maze-gen.exe run "$@"