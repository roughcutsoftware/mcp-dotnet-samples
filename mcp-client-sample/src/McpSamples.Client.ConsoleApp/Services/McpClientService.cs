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

            if (response?.Result is JsonElement element)
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

            if (response?.Result is JsonElement element)
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