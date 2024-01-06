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

1. Add package to `packages.config`
2. Run `nuget restore` or `nuget install PKG_NAME [-Version PKG_VERSION] -o packages`
3. Add reference to .csproj and specify the HintPath pointing to the .dll.

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
