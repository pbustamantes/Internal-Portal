using Xunit;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using InternalPortal.Application.Common.Interfaces;
using InternalPortal.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InternalPortal.Integration.Tests;

public class ApiIntegrationTests : IClassFixture<ApiIntegrationTests.CustomFactory>
{
    private readonly HttpClient _client;

    public class CustomFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = "TestDb_Api_" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove all DbContext-related registrations
                var descriptorsToRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)).ToList();
                foreach (var d in descriptorsToRemove) services.Remove(d);

                // Add in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));

                // Replace real email service with no-op stub for tests
                var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
                if (emailDescriptor != null) services.Remove(emailDescriptor);
                services.AddScoped<IEmailService, StubEmailService>();
            });
        }
    }

    public ApiIntegrationTests(CustomFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEvents_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/events");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn200()
    {
        var content = new StringContent(
            """{"email":"test@test.com","password":"Password123!","firstName":"Test","lastName":"User"}""",
            System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Swagger_ShouldBeAccessible()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UploadProfilePicture_WithValidImage_ShouldReturn200()
    {
        // Register a user and get the token
        var registerContent = new StringContent(
            """{"email":"pic@test.com","password":"Password123!","firstName":"Pic","lastName":"User"}""",
            System.Text.Encoding.UTF8, "application/json");
        var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var registerJson = await registerResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(registerJson);
        var token = doc.RootElement.GetProperty("accessToken").GetString()!;

        // Create a small 1x1 PNG image
        var pngBytes = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE, 0x00, 0x00, 0x00,
            0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
            0x00, 0x00, 0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, 0x00, 0x00, 0x00,
            0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        };

        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        formContent.Add(fileContent, "file", "test.png");

        // Verify the user was created by calling GET /users/me
        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResponse = await _client.SendAsync(meRequest);
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "/api/users/me/profile-picture");
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        uploadRequest.Content = formContent;
        var uploadResponse = await _client.SendAsync(uploadRequest);
        var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK, uploadJson);

        var uploadDoc = JsonDocument.Parse(uploadJson);
        uploadDoc.RootElement.GetProperty("profilePictureUrl").GetString().Should().Contain("/uploads/profile-pictures/");
    }

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ShouldReturn200()
    {
        var content = new StringContent(
            """{"email":"test@test.com"}""",
            System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/forgot-password", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WithoutToken_ShouldReturn400()
    {
        var content = new StringContent(
            """{"email":"test@test.com","token":"","newPassword":"NewPassword123"}""",
            System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/reset-password", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

public class StubEmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
