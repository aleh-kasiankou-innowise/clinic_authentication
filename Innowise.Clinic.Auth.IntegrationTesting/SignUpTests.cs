using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Persistence.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Innowise.Clinic.Auth.IntegrationTesting;

public class SignUpTests : IClassFixture<IntegrationTestingWebApplicationFactory>
{
    private readonly IntegrationTestingWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly Guid _patientRoleId;

    public SignUpTests(IntegrationTestingWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _patientRoleId = _factory.UseDbContext(x => x.Roles.Single(r => r.Name == UserRoles.Patient).Id);
    }

    [Fact]
    public async Task TestPatientWithValidEmailAndPasswordRegistered_OK()
    {
        // Arrange

        var validUserRegistrationData = new PatientCredentialsDto
        {
            Email = $"test{TestHelper.UniqueNumber}@test.com",
            Password = "12345678"
        };

        // Act

        var response = await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, validUserRegistrationData);

        // Assert

        Assert.True(response.IsSuccessStatusCode);
        Assert.True(_factory.UseDbContext(x => x.Users.Any(u => u.Email == validUserRegistrationData.Email)));

        var generatedTokens = await response.Content.ReadFromJsonAsync<AuthTokenPairDto>();
        var userId = TestHelper.ExtractUserIdFromJwtToken(generatedTokens.SecurityToken);

        Assert.True(_factory.UseDbContext(x => x.UserRoles
            .Any(ur => ur.RoleId == _patientRoleId && ur.UserId == userId)));
    }

    [Fact]
    public async Task TestPatientWithInvalidEmailAndValidPasswordRegistered_Fail()
    {
        // Arrange

        var userRegistrationDataWithInvalidMail = new PatientCredentialsDto
        {
            Email = "testInvalidEmail",
            Password = "12345678"
        };

        // Act

        var response =
            await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithInvalidMail);

        // Assert

        Assert.False(response.IsSuccessStatusCode);
        Assert.False(_factory.UseDbContext(x => x.Users
            .Any(u => u.Email == userRegistrationDataWithInvalidMail.Email)));
    }

    [Fact]
    public async Task TestPatientWithValidEmailAndTooShortPasswordRegistered_Fail()
    {
        // Arrange

        var userRegistrationDataWithShortPassword = new PatientCredentialsDto
        {
            Email = $"test{TestHelper.UniqueNumber}@test.gmail.com",
            Password = "12345"
        };

        // Act

        var response =
            await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithShortPassword);

        // Assert

        Assert.False(response.IsSuccessStatusCode);
        Assert.False(_factory.UseDbContext(x => x.Users
            .Any(u => u.Email == userRegistrationDataWithShortPassword.Email)));
    }

    [Fact]
    public async Task TestPatientWithValidEmailAndTooLongPasswordRegistered_Fail()
    {
        // Arrange

        var userRegistrationDataWithLongPassword = new PatientCredentialsDto
        {
            Email = $"test{TestHelper.UniqueNumber}@test.gmail.com",
            Password = "12345678911131517"
        };

        // Act

        var response =
            await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithLongPassword);

        // Assert

        Assert.False(response.IsSuccessStatusCode);
        Assert.False(_factory.UseDbContext(x => x.Users
            .Any(u => u.Email == userRegistrationDataWithLongPassword.Email)));
    }


    [Fact]
    public async Task TestPatientWithNonUniqueEmailAndValidPasswordRegistered_Fail()
    {
        // Arrange

        var validEmail = $"test{TestHelper.UniqueNumber}@test.gmail.com";

        var userRegistrationDataWithRegisteredEmail = new PatientCredentialsDto
        {
            Email = validEmail,
            Password = "12345678"
        };

        // Act

        await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithRegisteredEmail);
        var response =
            await _httpClient.PostAsJsonAsync(TestHelper.SignUpEndpointUri, userRegistrationDataWithRegisteredEmail);

        // Assert

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(1, _factory.UseDbContext(x => x.Users
            .Count(u => u.Email == userRegistrationDataWithRegisteredEmail.Email)));
    }
}