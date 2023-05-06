using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using DotNetEnv;

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
    Env.Load();
    _dbContext = new DynamoDBContext(new AmazonDynamoDBClient());
    _connectString = Guid.Parse(Environment.GetEnvironmentVariable("CONNECT_STRING"));
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
    try
    {
      Console.WriteLine("starting getListValue for POST");
      var user = await GetListValue();
      Console.WriteLine("user: ");
      Console.WriteLine(user);
      if (user != null && request.Body != "")
      {
        user.List.Add(request.Body);

        try
        {
          PostListValue(user).Wait();

          return GoodResponse(user.List);
        }
        catch (Exception e)
        {
          return BadRequest($"Failed to get Value from PostListValue - Message: {e.Message}", HttpStatusCode.InternalServerError);
        }

      }
    }
    catch (Exception e)
    {
      return BadRequest($"Submission Failed to Complete - Error: {e.Message}", HttpStatusCode.BadRequest);
    }
    return BadRequest("Failed to save new submission");
  }

  [LambdaFunction]
  [HttpApi(LambdaHttpMethod.Delete, "/submissions")]

  public async Task<APIGatewayHttpApiV2ProxyResponse> DeleteListAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
  {
    if (request.RequestContext.Http.Method == "DELETE")
    {
      try
      {
        await _dbContext.DeleteAsync<SubmissionBody>(_connectString);

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

  private static APIGatewayHttpApiV2ProxyResponse GoodResponse(List<string> list)
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
    var result = await _dbContext.LoadAsync<SubmissionBody>(_connectString);

    if (result != null)
    {
      // Values transformed from string to Submission Collection Type
      SubmissionCollection finalResult = new SubmissionCollection
      {
        Id = _connectString,
        List = new List<string>() { result.List.Trim() }
      };

      // Remove all empty string
      finalResult.List.RemoveAll(x => x == "");

      return finalResult;
    }
    else
    {
      var init_submission = new SubmissionCollection() { Id = _connectString, List = { } };

      return init_submission;
    }
  }

  private async Task PostListValue(SubmissionCollection newSubmission)
  {
    try
    {
      if (newSubmission.List != null)
      {
        // Values saved to DB as String
        var saveToDb = string.Join(", ", newSubmission.List);

        if (saveToDb != "")
        {
          var postList = new SubmissionBody()
          {
            Id = _connectString,
            List = saveToDb
          };
          await _dbContext.SaveAsync(postList);
        }
        else
        {
          Console.WriteLine("saveToDb is actually an empty string");
        }
      }
      else
      {
        throw new Exception("newSubmission.List is null...terminating");
      }


    }
    catch (Exception e)
    {
      Console.WriteLine($"Error in PostListValue: {e}");
      throw;
    }

  }
}

public class SubmissionCollection
{
  public SubmissionCollection()
  {
    Id = Guid.NewGuid();
    List = new List<string> { };
  }

  [DynamoDBHashKey]
  public Guid Id;

  [JsonPropertyName("list")]
  public List<string> List { get; set; }
}

public class SubmissionBody
{
  public SubmissionBody()
  {
    Id = Guid.NewGuid();
    List = "";
  }

  [DynamoDBHashKey]
  public Guid Id;

  [DynamoDBProperty("List")]
  public string List { get; set; }
}

public class RequestBody
{
  [JsonPropertyName("body")]
  public string? Body { get; set; }
}
