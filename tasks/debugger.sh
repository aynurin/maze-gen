#!/bin/sh

set -e

msbuild

mono --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555 ./build/Debug/mazegen/maze-gen.exe run -p "$@"