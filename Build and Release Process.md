### Building the Project
```bash
dotnet build
```
After building, apply the code analysis fixes:
```bash
dotnet format analyzers --severity info
```

### Package Creation
The build automatically creates NuGet packages for distributable projects.
