# Quick Start: Building MCP Clients in C#

This guide shows you how to build applications that **consume** MCP (Model Context Protocol) servers using C#.

## 🎯 What You'll Learn

By following this guide, you'll understand how to:
- Build MCP clients that connect to MCP servers
- Discover and invoke tools from servers programmatically  
- Handle authentication, errors, and multiple connections
- Create AI applications that leverage external capabilities

## 🚀 Quick Demo

### 1. Clone and explore this repository
```bash
git clone https://github.com/roughcutsoftware/mcp-dotnet-samples
cd mcp-dotnet-samples
```

### 2. Start an MCP server (example: todo-list)
```bash
cd todo-list
dotnet run --project ./src/McpSamples.TodoList.HybridApp -- --http
# Server will start at http://localhost:5240/mcp
```

### 3. Run the example MCP client
```bash
# In a new terminal
cd mcp-client-sample  
dotnet run --project ./src/McpSamples.Client.ConsoleApp
```

You'll see output like:
```
=== Demo: Todo List Server ===
Available tools:
  - add_todo_item: Adds a to-do item to database.
  - get_todo_items: Gets a list of to-do items from database.

Adding a todo item...
Result: Todo item added: 'Learn about MCP clients' (ID: 1)
```

## 📚 Complete Resources

- **[Full Documentation](./HOW_TO_BUILD_MCP_CLIENT.md)** - Comprehensive 25k+ character guide
- **[Working Sample](./mcp-client-sample/)** - Complete console application example
- **[Server Samples](./README.md)** - MCP servers to test your clients against

## 🔄 Key Concept: Servers vs Clients

```
┌─────────────────────┐    JSON-RPC     ┌─────────────────────┐
│   MCP Client        │◄─── over ─────► │   MCP Server        │
│ (Your Application)  │      HTTP       │ (Service Provider)  │
│                     │                 │                     │
│ - Discovers tools   │                 │ - Exposes tools     │
│ - Invokes functions │                 │ - Handles requests  │
│ - Processes results │                 │ - Returns responses │
└─────────────────────┘                 └─────────────────────┘
```

**This repository shows you how to build BOTH sides!**

## 🛠️ Next Steps

1. **Start with the sample**: Run the client demo to see MCP in action
2. **Read the full guide**: Understand the complete implementation
3. **Build your own**: Create clients for your specific use cases
4. **Explore servers**: Check out the server samples for inspiration

---

**Ready to get started?** Head to [HOW_TO_BUILD_MCP_CLIENT.md](./HOW_TO_BUILD_MCP_CLIENT.md) for the complete guide!