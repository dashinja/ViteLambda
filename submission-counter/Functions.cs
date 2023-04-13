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
            return GoodResponse(list.List);
        }

        return BadRequest("List is null");

    }

    /// <summary>
    /// Adds new data to the submission list.
    /// </summary>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "/submissions")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> PostListAsync([FromBody] APIGatewayHttpApiV2ProxyRequest req, ILambdaContext context)
    {
        try
        {
            var newUser = await GetListValue();

            if (newUser != null)
            {
                newUser.List.Add(Int32.Parse(req.Body));

                var newSubmission = new SubmissionCollection()
                {
                    Id = _connectString,
                    List = newUser.List,
                };

                await PostListValue(newSubmission);

                return GoodResponse(newSubmission.List);
            }
        }
        catch (Exception)
        {

            throw new Exception("Submission Failed to Complete");
        }


        return BadRequest("Failed to save new submission");
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Delete, "/submissions")]

    public async Task<APIGatewayHttpApiV2ProxyResponse> DeleteListAsync ()
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

    private static APIGatewayHttpApiV2ProxyResponse GoodResponse(List<int> list)
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

public class SubmissionCollectionRequest
{
    public SubmissionCollectionRequest (int newValue)
    {
        NewValue = newValue;
    }
    [JsonPropertyName("body")]
    public int NewValue { get; set; }
}
