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