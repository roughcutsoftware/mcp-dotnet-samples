# MCP Client Sample

This sample demonstrates how to build a Model Context Protocol (MCP) **client** in C# that can connect to and consume MCP servers.

## Overview

This console application shows how to:
- Connect to MCP servers over HTTP
- Discover available tools from servers
- Invoke tools with parameters
- Handle responses and errors
- Manage connections

## Prerequisites

- .NET 8.0 SDK or later
- One or more MCP servers running (from the samples in this repository)

## Running the Sample

### Step 1: Start MCP Servers

First, start one or more of the MCP servers from this repository. For example:

```bash
# Terminal 1: Start Todo List server
cd todo-list
dotnet run --project ./src/McpSamples.TodoList.HybridApp -- --http

# Terminal 2: Start Awesome Copilot server  
cd awesome-copilot
dotnet run --project ./src/McpSamples.AwesomeCopilot.HybridApp -- --http

# Terminal 3: Start Markdown to HTML server
cd markdown-to-html
dotnet run --project ./src/McpSamples.MarkdownToHtml.HybridApp -- --http
```

### Step 2: Run the Client

```bash
# In a new terminal
cd mcp-client-sample
dotnet run --project ./src/McpSamples.Client.ConsoleApp
```

## What the Sample Demonstrates

### 1. Todo List Server Demo
- Connects to http://localhost:5240/mcp
- Lists available tools
- Adds a new todo item
- Retrieves all todo items

### 2. Awesome Copilot Server Demo  
- Connects to http://localhost:5250/mcp
- Lists available tools
- Searches for Python-related instructions

### 3. Markdown to HTML Server Demo
- Connects to http://localhost:5280/mcp
- Lists available tools
- Converts markdown text to HTML

## Key Components

### Models (Models/McpModels.cs)
Defines the MCP protocol data structures:
- `McpMessage` - Core JSON-RPC message format
- `McpTool` - Tool definition structure
- `ToolCallRequest/Result` - Tool invocation structures
- `InitializeParams` - Connection initialization

### Service (Services/McpClientService.cs)
Implements the MCP client functionality:
- HTTP-based communication with servers
- Tool discovery and invocation
- Connection management
- Error handling

### Console App (Program.cs)
Demonstrates practical usage:
- Dependency injection setup
- Multiple server connections
- Tool discovery and usage patterns
- Error handling examples

## Expected Output

When you run the client with servers running, you should see output like:

```
MCP Client Demo Application
==========================

=== Demo: Todo List Server ===
info: McpSamples.Client.ConsoleApp.Services.McpClientService[0]
      Connecting to MCP server at http://localhost:5240/mcp
info: McpSamples.Client.ConsoleApp.Services.McpClientService[0]
      Successfully connected to MCP server
info: Program[0]
      Available tools:
info: Program[0]
        - add_todo_item: Adds a to-do item to database.
info: Program[0]
        - get_todo_items: Gets a list of to-do items from database.
...

Adding a todo item...
info: Program[0]
      Result: Todo item added: 'Learn about MCP clients' (ID: 1)

Getting all todo items...
info: Program[0]
      Todo items: ID: 1, Text: Learn about MCP clients, Completed: False
```

## Extending the Sample

This sample provides a foundation that you can extend:

1. **Add authentication support** for secured servers
2. **Implement prompt and resource discovery** (not just tools)
3. **Add a user interface** instead of console output
4. **Create connection pooling** for multiple servers
5. **Add retry logic** for failed requests
6. **Implement caching** for tool definitions

## Troubleshooting

### Server Connection Issues
- Ensure the MCP servers are running with the `--http` flag
- Check that the server ports match the URLs in the client code
- Verify no firewall is blocking the connections

### Tool Invocation Errors
- Use `ListToolsAsync()` to see available tools
- Check parameter names and types match the tool's requirements
- Review server logs for detailed error information

## Related Documentation

- [Main documentation](../../HOW_TO_BUILD_MCP_CLIENT.md) - Comprehensive guide to building MCP clients
- [Server samples](../../README.md) - Examples of MCP servers to test against
- [MCP Specification](https://modelcontextprotocol.io/) - Official protocol documentation