#!/bin/sh

set -e

mono --profile=log:calls,alloc,output=output.mlpd,maxframes=4,calldepth=5 build/Debug/mazegen/maze-gen.exe perfrun
mprof-report --out=build/perf.txt output.mlpd