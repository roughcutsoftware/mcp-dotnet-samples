# How to Build a Copilot Client using C# and MCP

This guide demonstrates how to build Model Context Protocol (MCP) **clients** using C# that can connect to and consume MCP servers.

## Table of Contents

- [Understanding MCP: Client vs Server](#understanding-mcp-client-vs-server)
- [Prerequisites](#prerequisites)
- [MCP Client Architecture](#mcp-client-architecture)
- [Building a Basic MCP Client](#building-a-basic-mcp-client)
- [Connecting to MCP Servers](#connecting-to-mcp-servers)
- [Discovering and Invoking Tools](#discovering-and-invoking-tools)
- [Handling Authentication](#handling-authentication)
- [Advanced Client Features](#advanced-client-features)
- [Testing Your Client](#testing-your-client)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## Understanding MCP: Client vs Server

The Model Context Protocol (MCP) defines a standardized way for AI applications to connect to external data sources and tools. It's important to understand the distinction:

### MCP Servers (What this repository demonstrates)
- **Purpose**: Provide tools, prompts, and resources that can be consumed by clients
- **Examples**: The samples in this repository (awesome-copilot, todo-list, markdown-to-html, outlook-email)
- **Role**: Expose functionality through standardized MCP protocol
- **Think of it as**: The "service provider" that offers specific capabilities

### MCP Clients (What you want to build)
- **Purpose**: Connect to and consume MCP servers to leverage their capabilities
- **Examples**: VS Code with MCP extension, custom AI applications, chatbots
- **Role**: Discover, connect to, and invoke tools from MCP servers
- **Think of it as**: The "service consumer" that uses capabilities from servers

## Prerequisites

- **.NET 9.0 SDK** (Required for compatibility with MCP libraries)
- **Visual Studio Code** with C# Dev Kit extension
- **Basic understanding** of:
  - C# and .NET development
  - HTTP/JSON communication
  - Async/await patterns

## MCP Client Architecture

An MCP client typically consists of these components:

```
┌─────────────────────────────────────────────────┐
│                 MCP Client                      │
├─────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌────────────────────────┐ │
│  │ Connection      │  │ Tool Discovery &       │ │
│  │ Manager         │  │ Invocation             │ │
│  └─────────────────┘  └────────────────────────┘ │
│  ┌─────────────────┐  ┌────────────────────────┐ │
│  │ Authentication  │  │ Response Processing    │ │
│  │ Handler         │  │ & Error Handling       │ │
│  └─────────────────┘  └────────────────────────┘ │
└─────────────────────────────────────────────────┘
                         │
                         ▼ (HTTP/STDIO)
┌─────────────────────────────────────────────────┐
│                 MCP Server                      │
│  (todo-list, awesome-copilot, etc.)            │
└─────────────────────────────────────────────────┘
```

## Building a Basic MCP Client

Let's start by creating a simple MCP client that can connect to the servers in this repository.

### Step 1: Create Client Project Structure

```bash
# Create the client project
mkdir mcp-client-sample
cd mcp-client-sample
dotnet new console -n McpSamples.Client.ConsoleApp
cd McpSamples.Client.ConsoleApp

# Add required packages
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.Http  
dotnet add package System.Text.Json
dotnet add package Microsoft.Extensions.Logging.Console
```

### Step 2: Define MCP Protocol Models

First, let's define the basic MCP protocol data structures:

```csharp
// Models/McpModels.cs
using System.Text.Json.Serialization;

namespace McpSamples.Client.ConsoleApp.Models;

// Core MCP message structure
public class McpMessage
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("method")]
    public string? Method { get; set; }
    
    [JsonPropertyName("params")]
    public object? Params { get; set; }
    
    [JsonPropertyName("result")]
    public object? Result { get; set; }
    
    [JsonPropertyName("error")]
    public McpError? Error { get; set; }
}

public class McpError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

// Tool-related models
public class McpTool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("inputSchema")]
    public object? InputSchema { get; set; }
}

public class ToolCallRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("arguments")]
    public Dictionary<string, object>? Arguments { get; set; }
}

public class ToolCallResult
{
    [JsonPropertyName("content")]
    public List<ContentItem>? Content { get; set; }
    
    [JsonPropertyName("isError")]
    public bool IsError { get; set; }
}

public class ContentItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";
    
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

// Server info models
public class ServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
    
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = string.Empty;
}

public class ServerCapabilities
{
    [JsonPropertyName("tools")]
    public object? Tools { get; set; }
    
    [JsonPropertyName("prompts")]
    public object? Prompts { get; set; }
    
    [JsonPropertyName("resources")]
    public object? Resources { get; set; }
}

public class InitializeParams
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";
    
    [JsonPropertyName("capabilities")]
    public ClientCapabilities Capabilities { get; set; } = new();
    
    [JsonPropertyName("clientInfo")]
    public ClientInfo ClientInfo { get; set; } = new();
}

public class ClientCapabilities
{
    [JsonPropertyName("roots")]
    public object? Roots { get; set; }
    
    [JsonPropertyName("sampling")]
    public object? Sampling { get; set; }
}

public class ClientInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "McpSamples.Client";
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
}
```

### Step 3: Create the MCP Client Service

Now let's create the main client service that handles communication with MCP servers:

```csharp
// Services/McpClientService.cs
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using McpSamples.Client.ConsoleApp.Models;

namespace McpSamples.Client.ConsoleApp.Services;

public interface IMcpClientService
{
    Task<bool> ConnectAsync(string serverUrl);
    Task<List<McpTool>> ListToolsAsync();
    Task<ToolCallResult> CallToolAsync(string toolName, Dictionary<string, object>? arguments = null);
    Task DisconnectAsync();
    bool IsConnected { get; }
}

public class McpClientService : IMcpClientService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<McpClientService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _serverUrl;
    private int _requestId = 1;

    public bool IsConnected => !string.IsNullOrEmpty(_serverUrl);

    public McpClientService(HttpClient httpClient, ILogger<McpClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public async Task<bool> ConnectAsync(string serverUrl)
    {
        try
        {
            _serverUrl = serverUrl.TrimEnd('/');
            _logger.LogInformation("Connecting to MCP server at {ServerUrl}", _serverUrl);

            // Initialize the connection
            var initializeRequest = new McpMessage
            {
                Id = GetNextRequestId(),
                Method = "initialize",
                Params = new InitializeParams()
            };

            var response = await SendRequestAsync(initializeRequest);
            if (response?.Error != null)
            {
                _logger.LogError("Failed to initialize: {Error}", response.Error.Message);
                return false;
            }

            _logger.LogInformation("Successfully connected to MCP server");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MCP server");
            return false;
        }
    }

    public async Task<List<McpTool>> ListToolsAsync()
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected to a server");

        try
        {
            var request = new McpMessage
            {
                Id = GetNextRequestId(),
                Method = "tools/list",
                Params = new { }
            };

            var response = await SendRequestAsync(request);
            if (response?.Error != null)
            {
                _logger.LogError("Failed to list tools: {Error}", response.Error.Message);
                return new List<McpTool>();
            }

            if (response.Result is JsonElement element)
            {
                var toolsProperty = element.GetProperty("tools");
                var tools = JsonSerializer.Deserialize<List<McpTool>>(toolsProperty.GetRawText(), _jsonOptions);
                return tools ?? new List<McpTool>();
            }

            return new List<McpTool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list tools");
            return new List<McpTool>();
        }
    }

    public async Task<ToolCallResult> CallToolAsync(string toolName, Dictionary<string, object>? arguments = null)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Not connected to a server");

        try
        {
            var request = new McpMessage
            {
                Id = GetNextRequestId(),
                Method = "tools/call",
                Params = new ToolCallRequest
                {
                    Name = toolName,
                    Arguments = arguments ?? new Dictionary<string, object>()
                }
            };

            var response = await SendRequestAsync(request);
            if (response?.Error != null)
            {
                _logger.LogError("Failed to call tool {ToolName}: {Error}", toolName, response.Error.Message);
                return new ToolCallResult { IsError = true };
            }

            if (response.Result is JsonElement element)
            {
                var result = JsonSerializer.Deserialize<ToolCallResult>(element.GetRawText(), _jsonOptions);
                return result ?? new ToolCallResult { IsError = true };
            }

            return new ToolCallResult { IsError = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call tool {ToolName}", toolName);
            return new ToolCallResult { IsError = true };
        }
    }

    public async Task DisconnectAsync()
    {
        if (IsConnected)
        {
            _logger.LogInformation("Disconnecting from MCP server");
            _serverUrl = null;
        }
        await Task.CompletedTask;
    }

    private async Task<McpMessage?> SendRequestAsync(McpMessage request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogDebug("Sending request: {Json}", json);

        var response = await _httpClient.PostAsync(_serverUrl, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Received response: {Json}", responseJson);

        return JsonSerializer.Deserialize<McpMessage>(responseJson, _jsonOptions);
    }

    private string GetNextRequestId() => (_requestId++).ToString();

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
```

### Step 4: Create the Console Application

Now let's create a console application that demonstrates using the MCP client:

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using McpSamples.Client.ConsoleApp.Services;

// Create host with dependency injection
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHttpClient<IMcpClientService, McpClientService>();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
    })
    .Build();

var client = host.Services.GetRequiredService<IMcpClientService>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("MCP Client Demo Application");
logger.LogInformation("==========================");

try
{
    // Example 1: Connect to Todo List server
    await DemoTodoListServerAsync(client, logger);
    
    // Example 2: Connect to Awesome Copilot server
    await DemoAwesomeCopilotServerAsync(client, logger);
}
catch (Exception ex)
{
    logger.LogError(ex, "Application error occurred");
}

static async Task DemoTodoListServerAsync(IMcpClientService client, ILogger logger)
{
    logger.LogInformation("\n=== Demo: Todo List Server ===");
    
    // Connect to the todo-list server (assumes it's running on localhost:5240)
    var todoServerUrl = "http://localhost:5240/mcp";
    
    if (!await client.ConnectAsync(todoServerUrl))
    {
        logger.LogWarning("Could not connect to todo server at {Url}. Make sure it's running with: dotnet run --project ./todo-list/src/McpSamples.TodoList.HybridApp -- --http", todoServerUrl);
        return;
    }

    // List available tools
    var tools = await client.ListToolsAsync();
    logger.LogInformation("Available tools:");
    foreach (var tool in tools)
    {
        logger.LogInformation("  - {Name}: {Description}", tool.Name, tool.Description);
    }

    // Demo tool usage
    if (tools.Any(t => t.Name == "add_todo_item"))
    {
        logger.LogInformation("\nAdding a todo item...");
        var result = await client.CallToolAsync("add_todo_item", new Dictionary<string, object>
        {
            ["todoItemText"] = "Learn about MCP clients"
        });
        
        if (!result.IsError && result.Content != null)
        {
            foreach (var content in result.Content)
            {
                logger.LogInformation("Result: {Text}", content.Text);
            }
        }
    }

    if (tools.Any(t => t.Name == "get_todo_items"))
    {
        logger.LogInformation("\nGetting all todo items...");
        var result = await client.CallToolAsync("get_todo_items");
        
        if (!result.IsError && result.Content != null)
        {
            foreach (var content in result.Content)
            {
                logger.LogInformation("Todo items: {Text}", content.Text);
            }
        }
    }

    await client.DisconnectAsync();
}

static async Task DemoAwesomeCopilotServerAsync(IMcpClientService client, ILogger logger)
{
    logger.LogInformation("\n=== Demo: Awesome Copilot Server ===");
    
    // Connect to the awesome-copilot server (assumes it's running on localhost:5250)
    var copilotServerUrl = "http://localhost:5250/mcp";
    
    if (!await client.ConnectAsync(copilotServerUrl))
    {
        logger.LogWarning("Could not connect to copilot server at {Url}. Make sure it's running with: dotnet run --project ./awesome-copilot/src/McpSamples.AwesomeCopilot.HybridApp -- --http", copilotServerUrl);
        return;
    }

    // List available tools
    var tools = await client.ListToolsAsync();
    logger.LogInformation("Available tools:");
    foreach (var tool in tools)
    {
        logger.LogInformation("  - {Name}: {Description}", tool.Name, tool.Description);
    }

    // Demo search functionality
    if (tools.Any(t => t.Name == "search_instructions"))
    {
        logger.LogInformation("\nSearching for Python instructions...");
        var result = await client.CallToolAsync("search_instructions", new Dictionary<string, object>
        {
            ["keywords"] = "python"
        });
        
        if (!result.IsError && result.Content != null)
        {
            foreach (var content in result.Content)
            {
                logger.LogInformation("Search results: {Text}", content.Text);
            }
        }
    }

    await client.DisconnectAsync();
}
```

## Connecting to MCP Servers

### HTTP-based Connection

For HTTP-based MCP servers (like when running with `--http` flag):

```csharp
var client = serviceProvider.GetRequiredService<IMcpClientService>();
await client.ConnectAsync("http://localhost:5242/mcp");
```

### STDIO-based Connection (Advanced)

For STDIO-based connections, you'll need a different implementation:

```csharp
public class StdioMcpClientService : IMcpClientService
{
    private Process? _serverProcess;
    
    public async Task<bool> ConnectAsync(string command, string[] args)
    {
        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        
        _serverProcess.Start();
        
        // Implement JSON-RPC communication over STDIO
        // This is more complex and requires handling line-by-line communication
        
        return true;
    }
    
    // ... implement other methods for STDIO communication
}
```

## Discovering and Invoking Tools

### Tool Discovery Pattern

```csharp
public async Task<Dictionary<string, McpTool>> DiscoverAllCapabilitiesAsync()
{
    var capabilities = new Dictionary<string, McpTool>();
    
    // Discover tools
    var tools = await ListToolsAsync();
    foreach (var tool in tools)
    {
        capabilities[tool.Name] = tool;
    }
    
    // You can also discover prompts and resources
    // var prompts = await ListPromptsAsync();
    // var resources = await ListResourcesAsync();
    
    return capabilities;
}
```

### Dynamic Tool Invocation

```csharp
public async Task<object?> InvokeToolDynamicallyAsync(string toolName, object parameters)
{
    // Convert parameters to dictionary
    var arguments = new Dictionary<string, object>();
    
    if (parameters != null)
    {
        var json = JsonSerializer.Serialize(parameters);
        arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(json) 
                   ?? new Dictionary<string, object>();
    }
    
    var result = await client.CallToolAsync(toolName, arguments);
    return result;
}
```

## Handling Authentication

### API Key Authentication

```csharp
public class AuthenticatedMcpClientService : McpClientService
{
    private readonly string _apiKey;
    
    public AuthenticatedMcpClientService(HttpClient httpClient, ILogger<McpClientService> logger, string apiKey) 
        : base(httpClient, logger)
    {
        _apiKey = apiKey;
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }
}
```

### Custom Authentication Headers

```csharp
// For Azure Functions or API Management scenarios
httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
httpClient.DefaultRequestHeaders.Add("X-Functions-Key", functionKey);
```

## Advanced Client Features

### Connection Pooling and Management

```csharp
public class McpConnectionManager
{
    private readonly Dictionary<string, IMcpClientService> _connections = new();
    private readonly IServiceProvider _serviceProvider;
    
    public async Task<IMcpClientService> GetOrCreateConnectionAsync(string serverUrl)
    {
        if (_connections.TryGetValue(serverUrl, out var existingClient))
        {
            return existingClient;
        }
        
        var client = _serviceProvider.GetRequiredService<IMcpClientService>();
        if (await client.ConnectAsync(serverUrl))
        {
            _connections[serverUrl] = client;
            return client;
        }
        
        throw new InvalidOperationException($"Failed to connect to {serverUrl}");
    }
}
```

### Retry and Error Handling

```csharp
public async Task<T> WithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException) when (i < maxRetries - 1)
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
        }
    }
    
    return await operation(); // Final attempt without catching
}
```

## Testing Your Client

### Unit Testing

```csharp
[Test]
public async Task CallTool_ValidTool_ReturnsResult()
{
    // Arrange
    var mockHttpClient = new Mock<HttpClient>();
    var logger = Mock.Of<ILogger<McpClientService>>();
    var client = new McpClientService(mockHttpClient.Object, logger);
    
    // Act
    var result = await client.CallToolAsync("test_tool", new Dictionary<string, object>
    {
        ["param1"] = "value1"
    });
    
    // Assert
    Assert.IsFalse(result.IsError);
    Assert.IsNotNull(result.Content);
}
```

### Integration Testing

```csharp
[Test]
public async Task IntegrationTest_TodoServer()
{
    // This test requires the todo server to be running
    var services = new ServiceCollection()
        .AddHttpClient<IMcpClientService, McpClientService>()
        .AddLogging()
        .BuildServiceProvider();
        
    var client = services.GetRequiredService<IMcpClientService>();
    
    // Test connection
    var connected = await client.ConnectAsync("http://localhost:5240/mcp");
    Assert.IsTrue(connected);
    
    // Test tool discovery
    var tools = await client.ListToolsAsync();
    Assert.IsTrue(tools.Any());
    
    // Test tool invocation
    var result = await client.CallToolAsync("get_todo_items");
    Assert.IsFalse(result.IsError);
}
```

## Best Practices

### 1. Connection Management
- **Always dispose clients properly** to free resources
- **Use connection pooling** for multiple server connections
- **Implement health checks** to verify server availability

### 2. Error Handling
- **Handle network failures gracefully** with retry logic
- **Validate tool parameters** before making calls
- **Log errors appropriately** for debugging

### 3. Performance
- **Cache tool discovery results** to avoid repeated calls
- **Use async/await properly** to avoid blocking
- **Consider connection timeouts** for responsive applications

### 4. Security
- **Never hardcode credentials** in source code
- **Use HTTPS** for production environments
- **Validate server certificates** in production

## Troubleshooting

### Common Issues

1. **Connection Refused**
   ```
   Solution: Ensure the MCP server is running and accessible
   Check: Server URL, port, and network connectivity
   ```

2. **Tool Not Found**
   ```
   Solution: Verify tool name spelling and availability
   Check: Use ListToolsAsync() to see available tools
   ```

3. **Invalid Parameters**
   ```
   Solution: Check tool schema and parameter requirements
   Check: Use proper data types and required parameters
   ```

4. **Authentication Failures**
   ```
   Solution: Verify API keys, tokens, or credentials
   Check: Authentication headers and server requirements
   ```

### Debugging Tips

1. **Enable detailed logging** to see request/response data
2. **Use network debugging tools** (Fiddler, Wireshark) to inspect traffic
3. **Test with simple tools first** before complex ones
4. **Verify server health** with basic HTTP requests

## Next Steps

1. **Extend the client** to support prompts and resources
2. **Add more robust error handling** and retry mechanisms
3. **Implement caching** for frequently used tools
4. **Create a UI application** that uses the MCP client
5. **Add support for real-time notifications** from servers

## Additional Resources

- [MCP Official Specification](https://modelcontextprotocol.io/)
- [This repository's server samples](./README.md) for testing your client
- [.NET HttpClient documentation](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
- [JSON-RPC 2.0 Specification](https://www.jsonrpc.org/specification)

---

This guide provides a comprehensive foundation for building MCP clients in C#. The example client can connect to any of the server samples in this repository, discover their capabilities, and invoke their tools programmatically.