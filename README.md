# Azure OpenAI Service Zero Shot Classification in .NET

This sample shows how to use the [Azure OpenAI Service .NET SDK](https://www.nuget.org/packages/Azure.AI.OpenAI/) to generate embeddings and classify GitHub issues from the .NET Runtime repo by area label.

## Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Azure Subscription](aka.ms/free)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource?pivots=web-portal)
    - Open AI Model Deployed (**text-similarity-curie-001**).

## Instructions

1. Set the following environment variables using information from your Azure Open AI service resource:
    - **OPENAI_ENDPOINT** - Azure OpenAI service endpoint.
    - **OPENAI_KEY** - Azure OpenAI service access key.
    - **OPENAI_DEPLOYMENT** - The name of the model deployment.
1. Build the project

    ```dotnetcli
    dotnet build
    ```

1. Run the project

    ```dotnetcli
    dotnet run
    ```

If successful, you should see output similar to the following:

```text
Title: Refine the memory pressure expiration algorithm | Actual Area: area-Extensions-Caching | Predicted Area: area-Extensions-Caching
Title: Quaternion operator overloads should be using the respective methods | Actual Area: area-System.Numerics | Predicted Area: area-System.Runtime.InteropServices
```