using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection.Emit;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ViteLambda;

public class Function
{

    /// <summary>
    /// A simple function that takes an input and returns it as a response.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns>APIGatewayProxyResponse</returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyResponse input, ILambdaContext context)
    {

        // var dbContext = new DynamoDBContext(new AmazonDynamoDBClient());

        // var submissionCollection = await dbContext.LoadAsync("submission_list");

        // Console.WriteLine("what is submissionCollection: ", submissionCollection);

        return new APIGatewayProxyResponse
        {
            Body = input.Body,
            StatusCode = (int)HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Access-Control-Allow-Origin", "https://vite-react-ts-dashinja.vercel.app" }
            },
        };
    }
}

[DynamoDBTable("SubmissionCollection")]
public class SubmissionCollection {
    [DynamoDBHashKey]
    public string? Id {get; set;}
    
    [DynamoDBProperty("list_access")]
    public List<int>? Collection {get; set;}
}