using Amazon.Lambda.Core;
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
    public PayloadResponse FunctionHandler(Payload input, ILambdaContext context)
    {

        if ()

        return new PayloadResponse(input)
        {
            Headers="what?"
        };
    }
}

public class Payload
{
    public int Data { get; set; }
}

public class PayloadResponse
{
    public PayloadResponse(Payload data)
    {
        this.Data = data.Data;
    }
    public int Data { get; set; }
    public string? Headers { get; set; }
}