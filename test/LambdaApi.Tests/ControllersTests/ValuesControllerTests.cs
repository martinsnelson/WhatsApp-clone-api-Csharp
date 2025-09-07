using System.Text.Json;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;



namespace LambdaApi.Tests.ControllersTests;

public class ValuesControllerTests
{


    [Fact]
    public async Task TestGet()
    {
        var lambdaFunction = new LambdaEntryPoint();

        var requestStr = File.ReadAllText("./SampleRequests/ControllersResquestTests/ValuesController-Get.json");
        var request = JsonSerializer.Deserialize<APIGatewayProxyRequest>(requestStr, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        var context = new TestLambdaContext();
        var response = await lambdaFunction.FunctionHandlerAsync(request, context);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("[\"Martins\",\"Nelson\"]", response.Body);
        Assert.True(response.MultiValueHeaders.ContainsKey("Content-Type"));
        Assert.Equal("application/json; charset=utf-8", response.MultiValueHeaders["Content-Type"][0]);
    }


}