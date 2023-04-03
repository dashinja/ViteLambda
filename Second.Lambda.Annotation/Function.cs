using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Second.Lambda.Annotation;

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
    [HttpApi(LambdaHttpMethod.Get, "/users/{userId}")]
    public async Task<User> FunctionHandler(string userId, ILambdaContext context)
    {
        Guid.TryParse(userId, out var id);
        return await _dbContext.LoadAsync<User>(id);
    }
}

public class User
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }
    public string? Name { get; set; }
}