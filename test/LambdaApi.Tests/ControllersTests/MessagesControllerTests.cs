using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Xunit;

namespace LambdaApi.Tests.ControllersTests;

public class MessagesControllerTests
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

    [Fact]
        public async Task TestCreateMessage()
        {
            var lambdaFunction = new LambdaEntryPoint();

            // Carregar request de exemplo do diretório SampleRequests
            var requestStr = File.ReadAllText("./SampleRequests/ControllersResquestTests/MessagesController-Create.json");
            var request = JsonSerializer.Deserialize<APIGatewayProxyRequest>(requestStr, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var context = new TestLambdaContext();

            var response = await lambdaFunction.FunctionHandlerAsync(request, context);

            Assert.Equal(200, response.StatusCode);
            Assert.Contains("\"status\":\"save\"", response.Body);
            Assert.True(response.MultiValueHeaders.ContainsKey("Content-Type"));
            Assert.Equal("application/json; charset=utf-8", response.MultiValueHeaders["Content-Type"][0]);
        }

        [Fact]
        public async Task TestListMessages()
        {
            var lambdaFunction = new LambdaEntryPoint();

            var requestStr = File.ReadAllText("./SampleRequests/ControllersResquestTests/MessagesController-List.json");
            var request = JsonSerializer.Deserialize<APIGatewayProxyRequest>(requestStr, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var context = new TestLambdaContext();

            var response = await lambdaFunction.FunctionHandlerAsync(request, context);

            Assert.Equal(200, response.StatusCode);
            Assert.Contains("\"status\":\"listed\"", response.Body);
            Assert.True(response.MultiValueHeaders.ContainsKey("Content-Type"));
            Assert.Equal("application/json; charset=utf-8", response.MultiValueHeaders["Content-Type"][0]);
        }
}