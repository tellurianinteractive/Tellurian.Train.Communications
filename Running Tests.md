## Testing Strategy

Tests are organized by layer:
- `*.Tests` projects mirror the structure of their corresponding implementation projects
- Tests validate command byte generation, notification parsing, and protocol correctness
- DO NOT use [ExpectedException] or Assert.ThrowsException; use Assert.Throws instead 

### Running Tests

This project uses MSTest.Sdk with Microsoft Testing Platform (MTP), configured in `global.json`.

```bash
# Run all tests in the solution
dotnet test --solution Tellurian.Trains.Control.sln

# Run tests for a specific project
dotnet test --project Tellurian.Trains.Protocols.XpressNet.Tests
dotnet test --project Tellurian.Trains.Adapters.Z21.Tests
dotnet test --project Tellurian.Trains.Communications.Channels.Tests
dotnet test --project Tellurian.Trains.Protocols.LocoNet.Tests
dotnet test --project Tellurian.Trains.Interfaces.Tests

# Alternative: run from within a test project directory
cd Tellurian.Trains.Interfaces.Tests
dotnet run

# List all tests
dotnet test --project Tellurian.Trains.Interfaces.Tests --list-tests
```

### Assembly Settings
- Use `InternalsVisibleTo` for test projects to access internal members
- Parallelize tests where possible using [assembly.Parallelize]
- Write any assembly-level attributes in `_AssemblySettings.cs`
