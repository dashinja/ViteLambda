using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ViteLambda
{
    public class SubmissionCollection
    {
        public readonly Guid Id;

        [JsonPropertyName("list")]
        public List<int> List { get; set; }
    }
}
