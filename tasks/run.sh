#!/bin/sh

set -e

msbuild

mono --debug build/Debug/mazegen/maze-gen.exe run "$@"