## Building

```
> msbuild core.mono.csproj
```

## TODO

1. Add predefined areas with area types before generating the maze
    - e.g., rooms (walls, can enter)
    - lakes (no walls, cannot enter)
    - mounts (walls, cannot enter)
    - area tags to allow different types of areas
2. Add unit tests

## Various

Clean nuget cache:

```bash
rm -rf ~/.local/share/NuGet/plugins-cache
rm -rf /tmp/NuGetScratchshav/
rm -rf ~/.local/share/NuGet/v3-cache
rm -rf ~/.nuget/packages
rm -rf packages
```

### Notes

Project is migrated to mono, so:

**Build:**

```bash
msbuild
```

**Test:**

```bash
nunit3-console build/Debug/tests/tests.dll
```

**NuGet**

```bash
nuget restore
```