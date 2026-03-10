public class ToolRegistry
{
    private readonly IEnumerable<ITool> _tools;
    public ToolRegistry(IEnumerable<ITool> tools)
    {
        _tools = tools;
    }
    public IEnumerable<ITool> GetAll()
    {
        return _tools;
    }
    public ITool Get(string name)
    {
        return _tools.FirstOrDefault(t => t.Name == name);
    }
}