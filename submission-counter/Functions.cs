using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using System.Security.Cryptography.X509Certificates;

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
            return GoodResponse(list.List);
        }

        return BadRequest("List not found", HttpStatusCode.NotFound);

    }

    /// <summary>
    /// Adds new data to the submission list.
    /// </summary>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/submissions")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> PostListAsync([FromBody] RequestBody request, ILambdaContext context)
    {

        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(request));
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(request.Body));

        try
        {
            Console.WriteLine("Begin try block");
            var newUser = await GetListValue();

            Console.WriteLine("newUser Received succesfully");
            Console.WriteLine("newUser $$$$: ", newUser);


            if (newUser != null)
            {
                Console.WriteLine("newUser is not null");

                var newSubmission = new SubmissionCollection();

                if (newUser.List.Count > 0)
                {
                    Console.WriteLine("newUser.List.Count > 0 so...");

                    newUser.List.Add(request.Body);
                    Console.WriteLine("newUser.List.Add(Int32.Parse(req.Body) Completed");

                    newSubmission = newUser;

                    Console.WriteLine("Add req.Body to newUser");
                    Console.WriteLine("newSubmission = newUser");

                }
                else
                {
                    Console.WriteLine("newUser has no elements so...");
                    Console.WriteLine("newSubmission freshly assigned defaults");
                    Console.WriteLine("req: ", request);
                    Console.WriteLine("req.body: ", request.Body);
                    // Console.WriteLine("req.body.length: ", request.Body.Length);

                    int newValue = request.Body;
                    Console.WriteLine("Int32.Parse(req.Body.Trim())");

                    newSubmission.Id = _connectString;
                    Console.WriteLine("newSubmission.Id = _connectString");

                    newSubmission.List = new List<int>() { newValue };
                    Console.WriteLine("newSubmission.List = new List<int>() { newValue };");

                    Console.WriteLine("Modified newSubmission successfully");
                }

                try
                {
                    Console.WriteLine("begin PostListValue block");
                    PostListValue(newSubmission).Wait();

                    Console.WriteLine("PostListValue(newSubmission) successfully completed");

                    return GoodResponse(newSubmission.List);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"PostlistValue failed - Message: {e}");
                    return BadRequest("Failed to get Value from PostListValue", HttpStatusCode.InternalServerError);
                    throw new Exception("Post itself failed: BAH!");
                }

            }
        }
        catch (Exception e)
        {
            Console.WriteLine("GetListValue Failed");

            Console.WriteLine($"Error Message: {e.Message}");
            return BadRequest("Submission Failed to Complete", HttpStatusCode.BadRequest);
            throw new Exception("Submission Failed to Complete");
        }


        return BadRequest("Failed to save new submission");
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Delete, "/submissions")]

    public async Task<APIGatewayHttpApiV2ProxyResponse> DeleteListAsync (APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        Console.WriteLine("What is the method?$$$$$");
        Console.WriteLine(request.RequestContext.Http.Method);

        if (request.RequestContext.Http.Method == "DELETE")
        {
            try
            {
                await _dbContext.DeleteAsync<SubmissionCollection>(_connectString);

                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = "List deleted",
                    StatusCode = (int)HttpStatusCode.NoContent
                };
            }
            catch (Exception)
            {
                throw new Exception("Problem deleting Item");
            }
        }

        return BadRequest("Incorrect Delete Request", HttpStatusCode.BadRequest);

    }

    private static APIGatewayHttpApiV2ProxyResponse GoodResponse(List<int> list)
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = System.Text.Json.JsonSerializer.Serialize(list)
        };
    }

    private static APIGatewayHttpApiV2ProxyResponse BadRequest(String message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = (int)statusCode,
            Body = message
        };
    }

    /// <summary>
    /// Retrieves current value list store and returns it to the caller.
    /// </summary>
    /// <returns>SubmissionCollection</returns>
    private async Task<SubmissionCollection> GetListValue()
    {
        var result = await _dbContext.LoadAsync<SubmissionCollection>(_connectString);

        if (result != null)
        {
            return result;
        } else
        {
            var init_submission = new SubmissionCollection() { Id = _connectString, List = { } };
            
            await _dbContext.SaveAsync(init_submission);

            return await _dbContext.LoadAsync<SubmissionCollection>(_connectString);
        }
    }

    private async Task PostListValue(SubmissionCollection newSubmission)
    {
        await _dbContext.SaveAsync(newSubmission);
    }
}

public class SubmissionCollection
{
    public SubmissionCollection() { 
        Id = Guid.NewGuid();
        List = new List<int> { };
    }

    [DynamoDBHashKey]
    public Guid Id;

    [JsonPropertyName("list")]
    public List<int> List { get; set; }
}

public class RequestBody
{
    [JsonPropertyName("body")]
    public int Body { get; set; }
}
