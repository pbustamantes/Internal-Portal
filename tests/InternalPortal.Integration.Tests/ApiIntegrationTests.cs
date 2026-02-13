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
    private readonly CustomFactory _factory;
    private readonly HttpClient _client;

    public class CustomFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = "TestDb_Api_" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Secret", "TestSecretKeyThatIsAtLeast32CharactersLong!");
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
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Creates an HttpClient that preserves cookies across requests (for refresh token cookie flow).
    /// </summary>
    private HttpClient CreateCookieClient()
    {
        var cookieContainer = new CookieContainer();
        var handler = new CookieContainerHandler(_factory.Server.CreateHandler(), cookieContainer);
        var client = new HttpClient(handler) { BaseAddress = _factory.Server.BaseAddress };
        return client;
    }

    [Fact]
    public async Task GetEvents_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/events");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn200AndSetRefreshCookie()
    {
        var content = new StringContent(
            """{"email":"test@test.com","password":"Password123!","firstName":"Test","lastName":"User"}""",
            System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify refresh token is in Set-Cookie, not in response body
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        var setCookie = string.Join("; ", cookies!);
        setCookie.Should().Contain("refreshToken=");
        setCookie.Should().Contain("httponly");
        setCookie.Should().Contain("path=/api/auth");

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("refreshToken", out _).Should().BeFalse();
        doc.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
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

    private async Task<string> RegisterAndGetAccessToken(string email)
    {
        var content = new StringContent(
            $$"""{"email":"{{email}}","password":"Password123!","firstName":"Test","lastName":"User"}""",
            System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("accessToken").GetString()!;
    }

    /// <summary>
    /// Register via a cookie-aware client so the refresh token cookie is automatically stored.
    /// Returns (cookieClient, accessToken).
    /// </summary>
    private async Task<(HttpClient cookieClient, string accessToken)> RegisterWithCookieClient(string email)
    {
        var cookieClient = CreateCookieClient();
        var content = new StringContent(
            $$"""{"email":"{{email}}","password":"Password123!","firstName":"Test","lastName":"User"}""",
            System.Text.Encoding.UTF8, "application/json");
        var response = await cookieClient.PostAsync("/api/auth/register", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return (cookieClient, doc.RootElement.GetProperty("accessToken").GetString()!);
    }

    [Fact]
    public async Task RevokeToken_WithCookie_ShouldReturn204()
    {
        var (cookieClient, accessToken) = await RegisterWithCookieClient("revoke-self@test.com");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/revoke");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await cookieClient.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RevokeToken_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.PostAsync("/api/auth/revoke", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithCookie_ShouldReturnNewAccessToken()
    {
        var (cookieClient, _) = await RegisterWithCookieClient("refresh-test@test.com");

        // Call refresh — cookie is sent automatically
        var refreshResponse = await cookieClient.PostAsync("/api/auth/refresh", null);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await refreshResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();

        // Verify a new cookie was issued (rotation)
        refreshResponse.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        string.Join("; ", cookies!).Should().Contain("refreshToken=");
    }

    [Fact]
    public async Task RefreshToken_WithoutCookie_ShouldReturn401()
    {
        var response = await _client.PostAsync("/api/auth/refresh", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithCookie_ShouldClearCookieAndReturn204()
    {
        var (cookieClient, accessToken) = await RegisterWithCookieClient("logout-test@test.com");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await cookieClient.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // After logout, refresh should fail
        var refreshResponse = await cookieClient.PostAsync("/api/auth/refresh", null);
        refreshResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    // ── Profile endpoints ──

    [Fact]
    public async Task GetCurrentUser_WithAuth_ShouldReturn200WithUserData()
    {
        var accessToken = await RegisterAndGetAccessToken("getme@test.com");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("email").GetString().Should().Be("getme@test.com");
        doc.RootElement.GetProperty("firstName").GetString().Should().Be("Test");
        doc.RootElement.GetProperty("lastName").GetString().Should().Be("User");
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/users/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_ShouldReturn200WithUpdatedFields()
    {
        var accessToken = await RegisterAndGetAccessToken("update-profile@test.com");

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(
            """{"firstName":"Updated","lastName":"Name","department":"Finance"}""",
            System.Text.Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("firstName").GetString().Should().Be("Updated");
        doc.RootElement.GetProperty("lastName").GetString().Should().Be("Name");
        doc.RootElement.GetProperty("department").GetString().Should().Be("Finance");
    }

    [Fact]
    public async Task UpdateProfile_ShouldPersistChanges()
    {
        var accessToken = await RegisterAndGetAccessToken("update-persist@test.com");

        // Update
        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, "/api/users/me");
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        updateRequest.Content = new StringContent(
            """{"firstName":"Persisted","lastName":"Change","department":"Legal"}""",
            System.Text.Encoding.UTF8, "application/json");
        var updateResponse = await _client.SendAsync(updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify via GET
        using var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var getResponse = await _client.SendAsync(getRequest);
        var json = await getResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("firstName").GetString().Should().Be("Persisted");
        doc.RootElement.GetProperty("lastName").GetString().Should().Be("Change");
        doc.RootElement.GetProperty("department").GetString().Should().Be("Legal");
    }

    [Fact]
    public async Task UpdateProfile_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.PutAsync("/api/users/me",
            new StringContent("""{"firstName":"A","lastName":"B"}""",
                System.Text.Encoding.UTF8, "application/json"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadProfilePicture_WithInvalidExtension_ShouldReturn400()
    {
        var accessToken = await RegisterAndGetAccessToken("bad-ext@test.com");

        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        formContent.Add(fileContent, "file", "malware.exe");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users/me/profile-picture");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = formContent;

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadProfilePicture_WithoutAuth_ShouldReturn401()
    {
        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        formContent.Add(fileContent, "file", "test.png");

        var response = await _client.PostAsync("/api/users/me/profile-picture", formContent);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteProfilePicture_AfterUpload_ShouldClearUrl()
    {
        var accessToken = await RegisterAndGetAccessToken("delete-pic@test.com");

        // Upload a picture first
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

        using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, "/api/users/me/profile-picture");
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        uploadRequest.Content = formContent;
        var uploadResponse = await _client.SendAsync(uploadRequest);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify picture URL was set
        var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
        var uploadDoc = JsonDocument.Parse(uploadJson);
        uploadDoc.RootElement.GetProperty("profilePictureUrl").GetString().Should().NotBeNullOrEmpty();

        // Delete the picture
        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/users/me/profile-picture");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteJson = await deleteResponse.Content.ReadAsStringAsync();
        var deleteDoc = JsonDocument.Parse(deleteJson);
        deleteDoc.RootElement.GetProperty("profilePictureUrl").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task DeleteProfilePicture_WithNoPicture_ShouldReturn200WithNullUrl()
    {
        var accessToken = await RegisterAndGetAccessToken("no-pic-delete@test.com");

        using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/users/me/profile-picture");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("profilePictureUrl").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task DeleteProfilePicture_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.DeleteAsync("/api/users/me/profile-picture");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyEvents_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/users/me/events");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyEvents_WithAuth_ShouldReturn200()
    {
        var accessToken = await RegisterAndGetAccessToken("my-events@test.com");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me/events");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Auth endpoints ──

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

/// <summary>
/// Delegating handler that adds cookie support on top of the test server's handler.
/// </summary>
internal class CookieContainerHandler : DelegatingHandler
{
    private readonly CookieContainer _cookieContainer;

    public CookieContainerHandler(HttpMessageHandler inner, CookieContainer cookieContainer)
        : base(inner)
    {
        _cookieContainer = cookieContainer;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Add stored cookies to the request
        var cookieHeader = _cookieContainer.GetCookieHeader(request.RequestUri!);
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Store response cookies
        if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            foreach (var cookie in setCookieHeaders)
            {
                _cookieContainer.SetCookies(request.RequestUri!, cookie);
            }
        }

        return response;
    }
}

public class StubEmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
