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
    Console.WriteLine("request: ");

    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(request));
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(request.Body));

    try
    {
      Console.WriteLine("starting getListValue for POST");
      var user = await GetListValue();
      Console.WriteLine("user: ");
      Console.WriteLine(user);
      if (user != null && request.Body != "")
      {
        Console.WriteLine("passes user not null && .body not empty string for POST");

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
      Console.WriteLine($"Error Message: {e}");
      return BadRequest($"Submission Failed to Complete - Error: {e.Message}", HttpStatusCode.BadRequest);
    }
    return BadRequest("Failed to save new submission");
  }

  [LambdaFunction]
  [HttpApi(LambdaHttpMethod.Delete, "/submissions")]

  public async Task<APIGatewayHttpApiV2ProxyResponse> DeleteListAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
  {
    Console.WriteLine("What is the method?$$$$$");
    Console.WriteLine(request.RequestContext.Http.Method);

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
    Console.WriteLine("begin GetListValue()");
    var result = await _dbContext.LoadAsync<SubmissionBody>(_connectString);
    Console.WriteLine("GetListValue() Successful");
    Console.WriteLine("result value: ");
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result));


    if (result != null)
    {
      Console.WriteLine("pass result.Id != _connectString ");

      // Values transformed from string to Submission Collection Type
      SubmissionCollection finalResult = new SubmissionCollection
      {
        Id = _connectString,
        List = new List<string>() { result.List.Trim() }
      };

            // Remove all empty string
            finalResult.List.RemoveAll(x => x == "");

            Console.WriteLine("finalResult success");

      return finalResult;
    }
    else
    {
      Console.WriteLine("begin else statement as result IS '' in GetListValue()");

      var init_submission = new SubmissionCollection() { Id = _connectString, List = { } };

      Console.WriteLine("successfully make init_submission in GetListValue()");

      // await PostListValue(init_submission);

      // Console.WriteLine("successful PostListValue using init_submission in  GetListValue()");

      Console.WriteLine("return simply 'init_submission'");

      return init_submission;
    }
  }

  private async Task PostListValue(SubmissionCollection newSubmission)
  {
    try
    {
      Console.WriteLine("begin PostListValue()");

      if (newSubmission.List != null)
      {
        // Values saved to DB as String
        var saveToDb = string.Join(", ", newSubmission.List);
        Console.WriteLine("successful saveToDb in PostListValue()");

        Console.WriteLine("saveToDb value");
        Console.WriteLine(saveToDb);

        Console.WriteLine("START save of savetodb INTO DB");
        if (saveToDb != "")
        {
          var postList = new SubmissionBody()
          {
            Id = _connectString,
            List = saveToDb
          };
          await _dbContext.SaveAsync(postList);
          Console.WriteLine("FINISH successful save of savetodb INTO DB");
        }
        else
        {
          Console.WriteLine("saveToDb is actually an empty string");
        }
      }
      else
      {
        Console.WriteLine("Somehow newSubmission.List IS null... cannot go on");

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
  public string Body { get; set; }
}
