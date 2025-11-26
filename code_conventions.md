## Code Conventions and Best Practices
This project follows strict C# coding standards documented by Microsoft.


### Querying Microsoft Documentation

You have access to MCP tools called `microsoft_docs_search`, `microsoft_docs_fetch`, and `microsoft_code_sample_search` - these 
tools allow you to search through and fetch Microsoft's latest official documentation and code samples, 
and that information might be more detailed or newer than what's in your training data set.

When handling questions around how to work with native Microsoft technologies, 
such as C#, F#, ASP.NET Core, Microsoft.Extensions, NuGet, Entity Framework, the `dotnet` runtime - please use 
these tools for research purposes when dealing with specific / narrowly defined questions that may occur.

### Key Design Patterns

1. **Observer Pattern**: Used throughout for event notifications (communication results, state changes, notifications)
2. **Strategy Pattern**: Multiple protocol implementations behind common interfaces
3. **Command Pattern**: Protocol commands encapsulate operations as objects
4. **Factory Pattern**: `NotificationFactory` creates appropriate notification types from byte data
5. **Result Type**: `CommunicationResult` represents operation outcomes without exceptions

### Language Features
- Target framework: **net10.0**
- **Modern C# features**: Use C# 14 and later features as documented by Microsoft.
