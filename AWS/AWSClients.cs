using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;

namespace _301153142_301137955_Soto_Ko_Lab3.AWS
{
    public static class AWSClients
    {
        public static AmazonDynamoDBClient dynamoClient;
        public static AmazonS3Client s3Client;
        const string AccessIdPropertyName = "ACCESS_KEY";
        const string AccessKeyPropertyName = "SECRET_ACCESS_KEY";

        private static string AwsAccessId { get; set; }
        private static string AwsAccessKey { get; set; }
        private static readonly RegionEndpoint caCentral = RegionEndpoint.CACentral1;

        static AWSClients()
        {

            AwsAccessId = Environment.GetEnvironmentVariable(AccessIdPropertyName)!.Trim();
            AwsAccessKey = Environment.GetEnvironmentVariable(AccessKeyPropertyName)!.Trim();
            BasicAWSCredentials dynamoCredentials = new(
                AwsAccessId,
                AwsAccessKey
            );

            dynamoClient = new(dynamoCredentials, caCentral);

            s3Client = new(AwsAccessId, AwsAccessKey, caCentral);
        }
    }
}
