using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Byron.Udemy.LambdaAnnotations;

public class Function
{
    private readonly DynamoDBContext _dbContext;
    public Function()
    {
        _dbContext = new DynamoDBContext(new AmazonDynamoDBClient());
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "users/{userId}")]
    public async Task<string> FunctionHandler(string userId, ILambdaContext context)
    {
        Guid.TryParse(userId, out var id);

        var user = await _dbContext.LoadAsync<User>(id);
        return System.Text.Json.JsonSerializer.Serialize(user);
    }
}

[DynamoDBTable("User")]
public class User
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }
    public string? Name { get; set; }
}