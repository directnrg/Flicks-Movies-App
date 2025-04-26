using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using System.Diagnostics;
using System.Net;

namespace Flicks_App.AWS
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
            try
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

            } catch (NullReferenceException)
            {
                try
                {
                    SystemsManagementClient = new(caCentral);

                    dynamoClient = new(caCentral);

                    s3Client = new(caCentral);

                } catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to create Client using IAM Role. {ex.Message}");
                }

            }   
        }
    }
}
