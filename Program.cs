using Azure.AI.OpenAI;

// Define list of candidate classes
var areas = new [] {
    "area-Extensions-Caching",
    "area-System.Runtime.InteropServices",
    "area-System.Numerics",
    "area-System.Xml"
};

// Initialize Azure OpenAI client
var endpoint = new Uri(Environment.GetEnvironmentVariable("OPENAI_ENDPOINT"));
var key = new Azure.AzureKeyCredential(Environment.GetEnvironmentVariable("OPENAI_KEY")); 
var deploymentId = Environment.GetEnvironmentVariable("OPENAI_DEPLOYMENT");
var client = new OpenAIClient(endpoint, key);

// Create a list of a GitHub issues
var data = new [] 
{
    new Issue
    {
        Title = @"Refine the memory pressure expiration algorithm",
        Description = @"When the cache is removing items due to memory pressure it currently considers:  Expired Priority LRU  Also consider:  TTL - How long does this item have to live before time based expiration? Sliding vs Absolute. Estimated object (graph) size Others?",
        ActualLabel = "area-Extensions-Caching",
        PredictedLabel = ""
    },
    new Issue
    {
        Title = @"Quaternion operator overloads should be using the respective methods",
        Description = @"Quaternion declares a handful of methods to perform addition, subtraction and multiplication, and provides the respective overloads for these operations. However, instead of re-using the Add, Multiply etc. methods, the code is re-written in the operator overloads. The operators should be using their respective methods rather than re-declaring the same code. This is under the assumption that the JIT inlines the methods when they are used in the operator overloads.",
        ActualLabel = "area-System.Numerics",
        PredictedLabel = ""
    }
};

// Utility function that uses OpenAI models to get embeddings
var GetEmbedding = (string text) => {
    var embeddingOptions = new EmbeddingsOptions(text);

    Embeddings embeddingResult = client.GetEmbeddings(deploymentId, embeddingOptions);

    return embeddingResult.Data[0].Embedding.ToArray();
};

// Utility function to calculate cosine similarity
var CosineSimilarity = (float[] vectorA, float[] vectorB) =>  {
    float dotProduct = 0.0f;
    float magnitudeA = 0.0f;
    float magnitudeB = 0.0f;
 
    for (int i = 0; i < vectorA.Length; i++)
    {
        dotProduct += vectorA[i] * vectorB[i];
        magnitudeA += vectorA[i] * vectorA[i];
        magnitudeB += vectorB[i] * vectorB[i];
    }
 
    return dotProduct / (float)(Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));    
};

// Generate embedings for areas (candidate classes)
var areaEmbeddings = 
    areas.Select(area => GetEmbedding(area)).ToArray();

// Generate embeddings for each of the issues
var issueEmbedding = data.Select(issue => GetEmbedding(issue.GetPreEmbeddedString()));

// Generate an embedding vector for each of the candidate classes
var areaSize = areaEmbeddings.Count();
var issueEmbeddingBroadcast = 
    issueEmbedding.Select(issueVector => 
        Enumerable.Repeat(issueVector,areaSize));

// Create mappings of issue and area embeddings 
var issueAreaEmbeddings = 
    issueEmbeddingBroadcast
    .Select(issueEmbeddings => 
        issueEmbeddings
            .Zip(areaEmbeddings,(issue,area) 
                => new { IssueEmbedding=issue, AreaEmbedding=area}).ToArray())
    .ToArray();

// Map area labels to issue and area embeddings
var areaLabelEmbeddingMapping = 
    issueAreaEmbeddings
        .Select(embeddings => 
            areas
                .Zip(embeddings,(area,embeddings) => 
                    new {
                        Area=area,
                        IssueEmbedding=embeddings.IssueEmbedding,
                        AreaEmbedding=embeddings.AreaEmbedding}))
        .ToArray();

// Get the top area for each issue
var topAreas = 
    areaLabelEmbeddingMapping
        .Select(x => 
            x.MaxBy(e => CosineSimilarity(e.IssueEmbedding,e.AreaEmbedding)));


// Create a new issue containing the predicted top area it belongs to
var predictions = 
    data.Zip(topAreas,(issue,topArea) => 
        new Issue{
            Title=issue.Title,
            Description=issue.Description,
            ActualLabel=issue.ActualLabel,
            PredictedLabel=topArea.Area})
    .ToArray();

// Print out issues and their actual v predicted label
foreach(var prediction in predictions)
{
    Console.WriteLine($"Title: {prediction.Title} | Actual Area: {prediction.ActualLabel} | Predicted Area: {prediction.PredictedLabel} ");
}

public class Issue
{
    public string Title {get;set;}
    public string Description {get;set;}

    public string ActualLabel {get;set;}
    public string PredictedLabel {get;set;}
    public string GetPreEmbeddedString () => $"Title: {this.Title} | Description: {this.Description}";
}