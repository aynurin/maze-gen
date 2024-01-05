## Testing

The unit-test coverage goal is 98% (please see `coverage` task in
`./.vscode/tasks.json`).

### Smoke tests

```bash
msbuild && nunit3-console build/Debug/tests/tests.dll --where="cat!=Smoke"
```

### Run one test

```bash
msbuild && nunit3-console build/Debug/tests/tests.dll --test="PlayersWorlds.Maps.AreaDistributorTest.AreaDistributorTest_SingleMapForce(\"6x12: P1x2;S2x2\")"
```

## Nuget

### Add a package

```bash
nuget install PKG_NAME [-Version PKG_VERSION] -o packages
```

Then add reference to the .csproj with a HintPath manually.

### Restore

```bash
nuget restore
```

## DocFx

Install docfx package:

```bash
dotnet tool install -g docfx
```

Build and serve the docs:

```bash
docfx docfx/docfx.json --serve
```
