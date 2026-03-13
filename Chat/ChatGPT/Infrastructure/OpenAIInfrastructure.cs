using OpenAI;

public class OpenAIInfrastructure
{
    public OpenAIClient Client { get; }

    public OpenAIInfrastructure(IConfiguration config)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        Client = new OpenAIClient(apiKey);
    }
}