using RestSharp;
using Newtonsoft.Json.Linq;

namespace RestSharp_Automation;

public class APITests
{
    private readonly RestClient client;

    public APITests()
    {
        string filePath = Path.Combine("..", "..", "..", "Fixtures", "apiRoutesConfig.json");
        var jsonData = File.ReadAllText(filePath);
        var config = JObject.Parse(jsonData);

        client = new RestClient(config["baseUrl"].ToString());
    }

    // Test to get all objects
    [Fact]
    public async Task GetListOfObjects()
    {
        var request = new RestRequest("objects", Method.Get);
        var response = await client.ExecuteAsync(request);

        Console.WriteLine("Request URL: " + request.Resource);
        Console.WriteLine("Response Status Code: " + (int)response.StatusCode);
        Console.WriteLine("Response Content: " + response.Content);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.NotNull(response.Content);

        var jsonResponse = JArray.Parse(response.Content);

        string outputFilePath = Path.Combine("..", "..", "..", "Fixtures", "responseData.json");  // Relative path to Fixtures folder
        File.WriteAllText(outputFilePath, jsonResponse.ToString());
    }

    // Test to Add an Object
    [Fact]
    public async Task AddObject()
    {
        var body = @"{
        ""name"": ""ASUS TUF B760M Plus WIFI"",
        ""data"": {
            ""year"": 2024,
            ""price"": 1599.99,
            ""CPU model"": ""AMD RYZEN 7 5800X3D"",
            ""Hard disk size"": ""2 TB""
            }
        }";

        var request = new RestRequest("objects", Method.Post);

        request.AddHeader("Content-Type", "application/json");
        request.AddParameter("application/json", body, ParameterType.RequestBody);
        Console.WriteLine("Request Body: " + body);

        var response = await client.ExecuteAsync(request);

        Assert.Equal(200, (int)response.StatusCode);
        var content = JObject.Parse(response.Content);
        Assert.NotNull(content["id"]);

        string responseFilePath = Path.Combine("..", "..", "..", "Fixtures", "responseData.json");
        JArray existingData;
        if (File.Exists(responseFilePath))
        {
            var existingDataJson = File.ReadAllText(responseFilePath);
            existingData = JArray.Parse(existingDataJson);
        }
        else
        {
            existingData = new JArray();
        }
        existingData.Add(content);
        File.WriteAllText(responseFilePath, existingData.ToString());
    }

    // Test to get an existing Object
    [Fact]
    public async Task GetObjectById()
    {
        string responseFilePath = Path.Combine("..", "..", "..", "Fixtures", "responseData.json");
        JArray existingData;

        if (File.Exists(responseFilePath))
        {
            var existingDataJson = File.ReadAllText(responseFilePath);
            existingData = JArray.Parse(existingDataJson);

            var lastAddedObject = existingData.Last;
            string objectId = lastAddedObject["id"].ToString();

            var request = new RestRequest($"objects/{objectId}", Method.Get);
            var response = await client.ExecuteAsync(request);

            Assert.Equal(200, (int)response.StatusCode);
            Assert.NotNull(response.Content);
            var jsonResponse = JObject.Parse(response.Content);
            Assert.Equal(objectId, jsonResponse["id"].ToString());
            Assert.NotNull(jsonResponse["name"]);
        }
        else
        {
            Assert.True(false, "Response data file does not exist.");
        }
    }

    // Test to update an existing Object
    [Fact]
    public async Task UpdateObject_ShouldReturnUpdatedObject()
    {
        string responseFilePath = Path.Combine("..", "..", "..", "Fixtures", "responseData.json");
        JArray existingData;

        if (File.Exists(responseFilePath))
        {
            var existingDataJson = File.ReadAllText(responseFilePath);
            existingData = JArray.Parse(existingDataJson);

            var lastAddedObject = existingData.Last;
            string objectId = lastAddedObject["id"].ToString();

            var body = @"{
            ""name"": ""ASUS TUF B760M Plus WIFI"",
            ""data"": {
                ""year"": 2024,
                ""price"": 1899.99,
                ""CPU model"": ""AMD RYZEN 7 7800X3D"",
                ""Hard disk size"": ""2 TB""
                }
            }";

            var request = new RestRequest($"objects/{objectId}", Method.Put);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);

            Assert.Equal(200, (int)response.StatusCode);
            var content = JObject.Parse(response.Content);
            Assert.Equal("TUF Gaming B760", content["name"].ToString());
            Assert.Equal(1849.99, (double)content["data"]["price"]);
        }
        else
        {
            Assert.True(false, "Response data file does not exist.");
        }
    }

    // Test to delete an existing Object
    [Fact]
    public async Task DeleteObject_ShouldReturnNoContent()
    {
        string responseFilePath = Path.Combine("..", "..", "..", "Fixtures", "responseData.json");
        JArray existingData;

        if (File.Exists(responseFilePath))
        {

            var existingDataJson = File.ReadAllText(responseFilePath);
            existingData = JArray.Parse(existingDataJson);

            var lastAddedObject = existingData.Last;
            string objectId = lastAddedObject["id"].ToString();

            var request = new RestRequest($"objects/{objectId}", Method.Delete);
            var response = await client.ExecuteAsync(request);

            Assert.Equal(200, (int)response.StatusCode);

            var getRequest = new RestRequest($"objects/{objectId}", Method.Get);
            var getResponse = await client.ExecuteAsync(getRequest);
            Assert.Equal(404, (int)getResponse.StatusCode);
        }
        else
        {
            Assert.True(false, "Response data file does not exist.");
        }
    }
}