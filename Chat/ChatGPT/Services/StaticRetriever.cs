// public class StaticRetriever : IRetriever
// {
//     private readonly List<string> _documents = new()
//     {
//         "Singleton pattern ensures only one instance exists.",
//         "Decorator pattern allows behavior extension without modifying original class.",
//         "Strategy pattern enables selecting algorithm at runtime."
//     };

//     public Task<IReadOnlyList<RetrievedChunk>> RetrieveAsync(string query)
//     {
//         var matches = _documents
//             .Where(d => d.Contains(query, StringComparison.OrdinalIgnoreCase))
//             .ToList();

//         return Task.FromResult((IReadOnlyList<RetrievedChunk>)matches);
//     }
// }