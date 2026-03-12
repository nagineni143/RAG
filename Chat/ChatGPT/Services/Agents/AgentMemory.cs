public class AgentMemory
{
    private readonly List<string> _observations = new();
    public void AddObservation(string observation)
    {
        _observations.Add(observation);
    }
    public string GetMemory()
    {
        if (_observations.Count == 0)
            return "";

        return string.Join("\n", _observations.Select((o, i) =>
            $"Observation {i + 1}: {o}"));
    }
    public void Clear()
    {
        _observations.Clear();
    }
}