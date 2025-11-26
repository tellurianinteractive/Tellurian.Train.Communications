## Testing Strategy

Tests are organized by layer:
- `*.Tests` projects mirror the structure of their corresponding implementation projects
- Tests validate command byte generation, notification parsing, and protocol correctness
- DO NOT use [ExpectedException] or Assert.ThrowsException; use Assert.Throws instead 

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test Tellurian.Trains.Protocols.XpressNet.Tests
dotnet test Tellurian.Trains.Adapters.Z21.Tests
dotnet test Tellurian.Communications.Channels.Tests
dotnet test Tellurian.Protocols.LocoNet.Tests
dotnet test Tellurian.Trains.Interfaces.Tests

# List all tests
dotnet test --list-tests
```

### Assembly Settings
- Use `InternalsVisibleTo` for test projects to access internal members
- Parallelize tests where possible using [assembly.Parallelize]
- Write any assembly-level attributes in `_AssemblySettings.cs`
