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

    // Console.WriteLine("input.body: ", input.Body);
    // Console.WriteLine("input.body", input.Body);

    // understand what the body shape is
    // must align the types (see attributes DataMember vs. JsonPropertyName)
    // var dbContext = new DynamoDBContext(new AmazonDynamoDBClient());

    // var submissionCollection = await dbContext.LoadAsync<User>(input);

    // Console.WriteLine("what is submissionCollection: ", submissionCollection);

    // return new APIGatewayProxyResponse
    // {
    //   Body = input.Body,
    //   StatusCode = (int)HttpStatusCode.OK,
    //   Headers = new Dictionary<string, string>
    //         {
    //             {"Content-Type", "application/json"},
    //             {"Access-Control-Allow-Origin", "https://vite-react-ts-dashinja.vercel.app" }
    //         },
    // };
  }
}

[DynamoDBTable("SubmissionCollection")]
public class SubmissionCollection
{

  public SubmissionCollection(string id, int newValue, List<int> collection)
  {
    Id = id;
    Collection = collection;
    // NewValue = newValue;
  }

  //   public SubmissionCollection(APIGatewayProxyResponse input) {
  //     var theBody = System.Text.Json.JsonSerializer.Deserialize<SubmissionCollection>(input);
  //   }

  [DynamoDBHashKey]
  public string Id { get; set; }

  public List<int>? Collection { get; set; }

  //   public int NewValue { get; set; }
}

public class User
{
    [DynamoDBHashKey]
  public Guid Id { get; set; }
  public string Name { get; set; }
}