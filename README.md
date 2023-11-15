# Summary

- core - the library, a .NET 4.7 assembly compatible with Unity
- maze-gen - a console client for debugging
- tests - ...

## TODO

1. Add predefined areas with area types before generating the maze
   - e.g., rooms (walls, can enter)
   - lakes (no walls, cannot enter)
   - mounts (walls, cannot enter)
   - area tags to allow different types of areas
2. Add unit tests

## Building

See `./.vscode/tasks.json`.

The coverage report is available in `build/coverage/summary.txt`.

Running the maze-gen on Linux:

```bash
mono --debug build/Debug/mazegen/maze-gen.exe
```

## Testing

### Smoke tests

```
msbuild && pushd build/Debug/tests; nunit3-console tests.dll --where="class=Nour.Play.AreaDistributorTest and cat=Smoke"; popd
```

### One method

```
msbuild && pushd build/Debug/tests; nunit3-console tests.dll --where="class=Nour.Play.AreaDistributorTest and method=AreaDistributorTest_SingleMapForce"; popd
```

### One test (w/ generated parameters)

```
msbuild && pushd build/Debug/tests; nunit3-console tests.dll --test="Nour.Play.AreaDistributorTest.AreaDistributorTest_SingleMapForce(\"6x12: P1x2;S2x2\")"; popd
```

## Nuget

### Add a package

```bash
nuget install PKG_NAME [-Version PKG_VERSION] -o packages
```

Then add reference with a HintPath manually.

### Restore

```bash
nuget restore
```

### Clean nuget cache

```bash
rm -rf ~/.local/share/NuGet/plugins-cache
rm -rf /tmp/NuGetScratchshav/
rm -rf ~/.local/share/NuGet/v3-cache
rm -rf ~/.nuget/packages
rm -rf packages
```
