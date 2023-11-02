using Amazon.SimpleSystemsManagement.Model;
using System.Net;

namespace _301153142_301137955_Soto_Ko_Lab3.AWS
{
    public class ParameterStore
    {
        public async static Task<GetParameterResponse> GetConnectionStringFromParameterStore() {
            

            var request = new GetParameterRequest()
            {
                Name = "/lab3/connection-string-rds",
                WithDecryption = true,

            };
            var result = await AWSClients.SystemsManagementClient.GetParameterAsync(request);

            return result;
        }
        
    }
}
