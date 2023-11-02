using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;

namespace _301153142_301137955_Soto_Ko_Lab3.AWS
{
    public static class AWSClients
    {
        public static AmazonDynamoDBClient dynamoClient;
        public static AmazonS3Client s3Client;
        private static AmazonSimpleSystemsManagementClient systemsManagementClient;
        const string AccessIdPropertyName = "ACCESS_KEY";
        const string AccessKeyPropertyName = "SECRET_ACCESS_KEY";

        private static string AwsAccessId { get; set; }
        private static string AwsAccessKey { get; set; }
        public static AmazonSimpleSystemsManagementClient SystemsManagementClient { get => systemsManagementClient; set => systemsManagementClient = value; }

        private static readonly RegionEndpoint caCentral = RegionEndpoint.CACentral1;

        static AWSClients()
        {

            AwsAccessId = Environment.GetEnvironmentVariable(AccessIdPropertyName)!.Trim();
            AwsAccessKey = Environment.GetEnvironmentVariable(AccessKeyPropertyName)!.Trim();
            
            BasicAWSCredentials credentials = new(
                AwsAccessId,
                AwsAccessKey
            );

            SystemsManagementClient = new(credentials, caCentral);

            dynamoClient = new(credentials, caCentral);

            s3Client = new(AwsAccessId, AwsAccessKey, caCentral);
        }
    }
}
