using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace submission_counter;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
public class Functions
{
    private readonly DynamoDBContext _dbContext;
    private readonly Guid _connectString;
    /// <summary>
    /// Default constructor.
    /// </summary>
    public Functions()
    {
        _dbContext = new DynamoDBContext(new AmazonDynamoDBClient());
        
        //TODO: Learn to put this in environment variable.
        _connectString = Guid.Parse("0405ee69-2efd-43ec-925a-086e25bf6459");
    }

    /// <summary>
    /// Endpoint that returns the submission list to the caller.
    /// </summary>
    /// <returns>APIGatewayHttpApiV2ProxyResponse</returns>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/submissions")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> GetListAsync()
    {
        SubmissionCollection list = await GetListValue();

        if (list != null)
        {
            return GoodResponse(list);
        }

        return BadRequest("List is null");

    }

    /// <summary>
    /// Adds new data to the submission list.
    /// </summary>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/submissions")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> PostListAsync([FromBody] SubmissionCollection req, ILambdaContext context)
    {
        await PostListValue(req);

        var newUser = await GetListValue();

        if (newUser != null)
        {
            return GoodResponse(newUser);
        }

        return BadRequest("Failed to save new submission");
    }

    private static APIGatewayHttpApiV2ProxyResponse GoodResponse(SubmissionCollection list)
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = System.Text.Json.JsonSerializer.Serialize(list)
        };
    }

    private static APIGatewayHttpApiV2ProxyResponse BadRequest(String message)
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.BadGateway,
            Body = message
        };
    }

    /// <summary>
    /// Retrieves current value list store and returns it to the caller.
    /// </summary>
    /// <returns>SubmissionCollection</returns>
    private async Task<SubmissionCollection> GetListValue()
    {
        return await _dbContext.LoadAsync<SubmissionCollection>(_connectString);
    }

    private async Task PostListValue(SubmissionCollection newSubmission)
    {
        await _dbContext.SaveAsync(newSubmission);
    }


//     /// <summary>
//     /// Root route that provides information about the other requests that can be made.
//     /// </summary>
//     /// <returns>API descriptions.</returns>
//     [LambdaFunction()]
//     [HttpApi(LambdaHttpMethod.Get, "/")]
//     public string Default()
//     {
//         var docs = @"Lambda Calculator Home:
// You can make the following requests to invoke other Lambda functions perform calculator operations:
// /add/{x}/{y}
// /subtract/{x}/{y}
// /multiply/{x}/{y}
// /divide/{x}/{y}
// ";
//         return docs;
//     }

//     /// <summary>
//     /// Perform x + y
//     /// </summary>
//     /// <param name="x"></param>
//     /// <param name="y"></param>
//     /// <returns>Sum of x and y.</returns>
//     [LambdaFunction()]
//     [HttpApi(LambdaHttpMethod.Get, "/add/{x}/{y}")]
//     public int Add(int x, int y, ILambdaContext context)
//     {
//         context.Logger.LogInformation($"{x} plus {y} is {x + y}");
//         return x + y;
//     }

//     /// <summary>
//     /// Perform x - y.
//     /// </summary>
//     /// <param name="x"></param>
//     /// <param name="y"></param>
//     /// <returns>x subtract y</returns>
//     [LambdaFunction()]
//     [HttpApi(LambdaHttpMethod.Get, "/subtract/{x}/{y}")]
//     public int Subtract(int x, int y, ILambdaContext context)
//     {
//         context.Logger.LogInformation($"{x} subtract {y} is {x - y}");
//         return x - y;
//     }

//     /// <summary>
//     /// Perform x * y.
//     /// </summary>
//     /// <param name="x"></param>
//     /// <param name="y"></param>
//     /// <returns>x multiply y</returns>
//     [LambdaFunction()]
//     [HttpApi(LambdaHttpMethod.Get, "/multiply/{x}/{y}")]
//     public int Multiply(int x, int y, ILambdaContext context)
//     {
//         context.Logger.LogInformation($"{x} multiply {y} is {x * y}");
//         return x * y;
//     }

//     /// <summary>
//     /// Perform x / y.
//     /// </summary>
//     /// <param name="x"></param>
//     /// <param name="y"></param>
//     /// <returns>x divide y</returns>
//     [LambdaFunction()]
//     [HttpApi(LambdaHttpMethod.Get, "/divide/{x}/{y}")]
//     public int Divide(int x, int y, ILambdaContext context)
//     {
//         context.Logger.LogInformation($"{x} divide {y} is {x / y}");
//         return x / y;
//     }
}

    public class SubmissionCollection
{
    [DynamoDBHashKey]
    public Guid Id;

        [JsonPropertyName("list")]
        public List<int>? List { get; set; }
}
