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
    
    // Example 3: Connect to Markdown to HTML server
    await DemoMarkdownToHtmlServerAsync(client, logger);
}
catch (Exception ex)
{
    logger.LogError(ex, "Application error occurred");
}

logger.LogInformation("\nDemo completed. Press any key to exit...");

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

static async Task DemoMarkdownToHtmlServerAsync(IMcpClientService client, ILogger logger)
{
    logger.LogInformation("\n=== Demo: Markdown to HTML Server ===");
    
    // Connect to the markdown-to-html server (assumes it's running on localhost:5280)
    var markdownServerUrl = "http://localhost:5280/mcp";
    
    if (!await client.ConnectAsync(markdownServerUrl))
    {
        logger.LogWarning("Could not connect to markdown server at {Url}. Make sure it's running with: dotnet run --project ./markdown-to-html/src/McpSamples.MarkdownToHtml.HybridApp -- --http", markdownServerUrl);
        return;
    }

    // List available tools
    var tools = await client.ListToolsAsync();
    logger.LogInformation("Available tools:");
    foreach (var tool in tools)
    {
        logger.LogInformation("  - {Name}: {Description}", tool.Name, tool.Description);
    }

    // Demo markdown conversion
    if (tools.Any(t => t.Name == "convert_markdown_to_html"))
    {
        logger.LogInformation("\nConverting markdown to HTML...");
        var result = await client.CallToolAsync("convert_markdown_to_html", new Dictionary<string, object>
        {
            ["markdown"] = "# Hello MCP\n\nThis is a **test** of markdown conversion."
        });
        
        if (!result.IsError && result.Content != null)
        {
            foreach (var content in result.Content)
            {
                logger.LogInformation("HTML result: {Text}", content.Text);
            }
        }
    }

    await client.DisconnectAsync();
}