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
  private readonly DynamoDBContext _dbContext;

  public Function()
  {
    _dbContext = new DynamoDBContext(new AmazonDynamoDBClient());

  }
  /// <summary>
  /// A simple function that takes an input and returns it as a response.
  /// </summary>
  /// <param name="request"></param>
  /// <param name="context"></param>
  /// <returns>APIGatewayProxyResponse</returns>
  public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
  {

    return request.RequestContext.Http.Method.ToUpper() switch
    {
      "GET" => await HandleGetRequest(request),
      "POST" => await HandlePostRequest(request)
    };
  }

  private async Task<APIGatewayHttpApiV2ProxyResponse> HandlePostRequest(APIGatewayHttpApiV2ProxyRequest request)
  {
    var user = System.Text.Json.JsonSerializer.Deserialize<User>(request.Body);

    if (user == null)
    {
      return BadResponse("Invalid User details");
    }

    await _dbContext.SaveAsync(user);
    return new APIGatewayHttpApiV2ProxyResponse()
    {
      StatusCode = (int)HttpStatusCode.OK
    };
  }

  private static APIGatewayHttpApiV2ProxyResponse BadResponse(string message)
  {
    return new APIGatewayHttpApiV2ProxyResponse()
    {
      Body = message,
      StatusCode = (int)HttpStatusCode.NotFound
    };
  }

  private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetRequest(APIGatewayHttpApiV2ProxyRequest request)
  {
    request.PathParameters.TryGetValue("userId", out var userIdString);
    if (Guid.TryParse(userIdString, out var userId))
    {

      var dbContext = new DynamoDBContext(new AmazonDynamoDBClient());
      var user = await _dbContext.LoadAsync<User>(userId);

      if (user != null)
      {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
          Body = System.Text.Json.JsonSerializer.Serialize(user),
          StatusCode = (int)HttpStatusCode.OK
        };
      }
    }

    return BadResponse("Invalid userId in path");
  }
}

[DynamoDBTable("User")]

public class User
{
  [DynamoDBHashKey]
  public Guid Id { get; set; }
  public string? Name { get; set; }
}