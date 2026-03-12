
public class McpServer
{
    private readonly ToolRegistry _toolRegistry;

    public McpServer(ToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry;
    }

    public IEnumerable<object> GetTools()
    {
        var tools = _toolRegistry.GetAll();

        return tools.Select(t => new
        {
            name = t.Name,
            description = t.Description,
            input_schema = t.GetSchema()
        });
    }
}