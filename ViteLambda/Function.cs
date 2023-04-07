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
  public async Task<User> FunctionHandler(Guid input, ILambdaContext context)
  {

    var dbContext = new DynamoDBContext(new AmazonDynamoDBClient());
    var user = await dbContext.LoadAsync<User>(input);

    return user;
  }
}

[DynamoDBTable("User")]

public class User
{
    [DynamoDBHashKey]
  public Guid Id { get; set; }
  public string? Name { get; set; }
}