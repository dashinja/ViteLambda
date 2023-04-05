using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection.Emit;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ViteLambda;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyResponse input, ILambdaContext context)
    {
        Console.WriteLine("what is input: ", input);
        return new APIGatewayProxyResponse
        {
            Body = input.Body,
            StatusCode = (int)HttpStatusCode.OK,
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Access-Control-Allow-Origin", "*" }
            }
        };
    }
}

public class Payload
{
    public int Data { get; set; }
}

public class PayloadResponse
{
    public PayloadResponse(int data)
    {
        this.Data = data;
    }
    public int Data { get; set; }
    public string? Headers { get; set; }
}