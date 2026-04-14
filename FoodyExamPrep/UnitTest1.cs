using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;



namespace FoodyExamPrep
{
    [TestFixture]
    public class FoodyTests
    {
        private RestClient client;
        private static string createdFoodId;
        private const string baseUrl = "http://144.91.123.158:81";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("desi123", "12341234");//to add my creds

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);

        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl); 
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

            
        }
        [Test, Order(1)]

        public void CreateFood_ShouldReturnCreated()
        {
            var food = new
            {
                Name = "Test Food",
                Description = "A delicious test food",
                Url = ""
            }; 
            
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            createdFoodId = json.GetProperty("foodId").GetString() ?? string.Empty;

            Assert.That(createdFoodId, Is.Not.Null.And.Not.Empty, "Food ID should not be null or empty.");

        }

        [Test, Order(2)]

        public void EditFoodTitle_ShouldReturnOk()
        {
            var changes = new[]
            {
              new {path = "/name", op = "replace", value = "Updated Test Food"}
            };
           
            var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch);
           
            request.AddJsonBody(changes);
           
            var response = client.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"), "Response should indicate successful update.");
        }

        [Test, Order(3)]

        public void GetAllFoods_ShouldReturnList()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);
            
            var response = client.Execute(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            var foods = JsonSerializer.Deserialize<List<object>>(response.Content);

            Assert.That(foods, Is.Not.Empty);
        }

        [Test, Order(4)]

        public void DeleteFood_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Food/Delete/{createdFoodId}", Method.Delete);
            
            var response = client.Execute(request);
            
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully"), "Response should indicate successful deletion.");
        }

        [Test, Order(5)]

        public void CreateFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var food = new
            {
                Description = "Missing name field",
                Url = ""
            };
            
            var request = new RestRequest("/api/Food/Create", Method.Post);
            
            request.AddJsonBody(food);
           
            var response = client.Execute(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]

        public void EditNonExistingFood_ShouldReturnNotFound()
        {
            string fakeID = "123";
            var changes = new[]
            {
              new {path = "/name", op = "replace", value = "Updated Test Food"}
            };
            
            var request = new RestRequest("/api/Food/Edit/{fakeID}", Method.Patch);
            
            request.AddJsonBody(changes);
            
            var response = client.Execute(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No food revues..."));
        }

        [Test, Order(7)]

        public void DeleteNonExistingFood_ShouldReturnNotFound()
        {
            string fakeID = "123";
            var request = new RestRequest("/api/Food/Delete/{fakeID}", Method.Delete);
            
            var response = client.Execute(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this food revue!"));
        }   




        [OneTimeTearDown]

        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}