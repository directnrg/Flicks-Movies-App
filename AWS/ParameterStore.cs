using Amazon.SimpleSystemsManagement.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net;

namespace Flicks_App.AWS
{
    public class ParameterStore

    { 
        public async static Task<Parameter> GetConnectionStringFromParameterStore()
        {


            var request = new GetParameterRequest()
            {
                Name = "/lab3/connection-string-rds",

                WithDecryption = true,

            };
            GetParameterResponse result = await AWSClients.SystemsManagementClient.GetParameterAsync(request);

            return result.Parameter;
        }

    }
}
