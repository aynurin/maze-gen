## Building

```
> dotnet build core.csproj
```

or

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
