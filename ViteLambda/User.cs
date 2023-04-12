using Amazon.DynamoDBv2.DataModel;

namespace ViteLambda;

[DynamoDBTable("User")]

public class User
{
  [DynamoDBHashKey]
  public Guid Id { get; set; }
  public string? Name { get; set; }
}