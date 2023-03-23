using System.Security.Claims;
using System.Text;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Exceptions.AccountBlockingService;
using Innowise.Clinic.Auth.Exceptions.CrossServiceCommunication;
using Innowise.Clinic.Auth.Exceptions.UserManagement;
using Innowise.Clinic.Auth.Persistence.Constants;
using Innowise.Clinic.Auth.Services.AccountBlockingService.Interfaces;
using Innowise.Clinic.Auth.Services.Constants;
using Innowise.Clinic.Auth.Services.Constants.Jwt;
using Innowise.Clinic.Auth.Services.Extensions;
using Innowise.Clinic.Auth.Services.JwtService.Interfaces;
using Innowise.Clinic.Auth.Services.UserManagementService.Data;
using Innowise.Clinic.Auth.Services.UserManagementService.Interfaces;
using Innowise.Clinic.Shared.Dto;
using Innowise.Clinic.Shared.MassTransit.MessageTypes.Events;
using Innowise.Clinic.Shared.MassTransit.MessageTypes.Requests;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.Services.UserManagementService.Implementations;

public class UserManagementService : IUserManagementService
{
    private readonly IAccountBlockingService _accountBlockingService;
    private readonly AuthenticationRequirementsSettings _authenticationRequirementsSettings;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly ITokenService _tokenGenerator;
    private readonly ITokenRevoker _tokenRevoker;
    private readonly ITokenValidator _tokenValidator;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly IRequestClient<UserProfileLinkingRequest> _userProfileLinkingClient;
    private readonly IBus _bus;

    public UserManagementService(
        UserManager<IdentityUser<Guid>> userManager,
        ITokenService tokenGenerator,
        SignInManager<IdentityUser<Guid>> signInManager,
        ITokenRevoker tokenRevoker,
        ITokenValidator tokenValidator,
        IOptions<AuthenticationRequirementsSettings> authenticationRequirementsSettings,
        IAccountBlockingService accountBlockingService,
        IRequestClient<UserProfileLinkingRequest> userProfileLinkingClient, IBus bus)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _signInManager = signInManager;
        _tokenRevoker = tokenRevoker;
        _tokenValidator = tokenValidator;
        _accountBlockingService = accountBlockingService;
        _userProfileLinkingClient = userProfileLinkingClient;
        _bus = bus;
        _authenticationRequirementsSettings = authenticationRequirementsSettings.Value;
    }

    public async Task RegisterPatientAsync(UserCredentialsDto patientCredentials)
    {
        var registeredUser = await RegisterNewPatientAsync(patientCredentials);
        var emailConfirmationLink = await PrepareEmailConfirmationLink(registeredUser);
        await _bus.Publish<PatientAccountCreatedEvent>(new(patientCredentials.Email, emailConfirmationLink));
    }

    public async Task RegisterConfirmedUserAsync(UserCredentialsDto userCredentials,
        AccountGenerationDto userCreationRequest)
    {
        var user = await RegisterNewConfirmedUserAsync(userCredentials, userCreationRequest);
        var profileLinkingResult =
            await _userProfileLinkingClient.GetResponse<UserProfileLinkingResponse>(new(
                userCreationRequest.EntityId, user.Id));

        if (!profileLinkingResult.Message.IsSuccessful)
        {
            throw new ProfileNotLinkedException();
        }
        
        await _bus.Publish<EmployeeAccountGeneratedEvent>(new (userCredentials.Email, userCredentials.Password, userCreationRequest.Role));
    }

    public async Task<AuthTokenPairDto> SignInUserAsync(UserCredentialsDto patientCredentials)
    {
        var user = await _userManager.FindByEmailAsync(patientCredentials.Email) ?? throw new UserNotFoundException();
        if (_authenticationRequirementsSettings.ValidateUserEmailConfirmedOnLogin && !user.EmailConfirmed)
            throw new EmailNotConfirmedException(
                "Please confirm your email. The confirmation link has been sent to your e-mail.");

        if (!await _accountBlockingService.IsAccountActive(user.Id))
        {
            throw new AccountBlockedException();
        }

        var isSignInSucceeded =
            await _signInManager.UserManager.CheckPasswordAsync(user, patientCredentials.Password);
        if (!isSignInSucceeded)
        {
            await _tokenRevoker.RevokeAllUserTokensAsync(user.Id);
            throw new InvalidCredentialsProvidedException();
        }

        var authTokens = await _tokenGenerator.GenerateJwtAndRefreshTokenAsync(user);
        return authTokens;
    }

    public async Task LogOutUserAsync(AuthTokenPairDto userTokens)
    {
        await _tokenValidator.ValidateTokenPairAndExtractPrincipal(userTokens, false);
        var tokenId = userTokens.GetRefreshTokenId();
        await _tokenRevoker.RevokeTokenAsync(tokenId);
    }

    public async Task<string> RefreshTokenAsync(AuthTokenPairDto userTokens)
    {
        try
        {
            var principal = await _tokenValidator.ValidateTokenPairAndExtractPrincipal(userTokens);
            return _tokenGenerator.GenerateSecurityToken(principal);
        }
        catch (SecurityTokenValidationException)
        {
            var userId = userTokens.GetUserId();
            await _tokenRevoker.RevokeAllUserTokensAsync(userId);
            throw;
        }
    }

    public async Task ConfirmUserEmailAsync(string userId, string emailConfirmationToken)
    {
        var emailConfirmationTokenBytes = WebEncoders.Base64UrlDecode(emailConfirmationToken);
        emailConfirmationToken = Encoding.UTF8.GetString(emailConfirmationTokenBytes);
        var user = await _userManager.FindByIdAsync(userId) ?? throw new UserNotFoundException();
        var confirmation = await _userManager.ConfirmEmailAsync(user, emailConfirmationToken);
        if (!confirmation.Succeeded)
        {
            throw new EmailConfirmationFailedException(confirmation.Errors);
        }
    }

    public async Task LinkAccountToProfile(PatientCreatedProfileMessage profileLinkingDto)
    {
        var user = await _userManager.FindByIdAsync(profileLinkingDto.UserId.ToString()) ??
                   throw new UserNotFoundException();
        await _userManager.AddClaimAsync(user,
            new Claim(JwtClaimTypes.CanInteractWithProfileClaim, profileLinkingDto.ProfileId.ToString()));

        await _tokenRevoker.RevokeAllUserTokensAsync(profileLinkingDto.UserId);
    }

    private async Task<string> PrepareEmailConfirmationLink(IdentityUser<Guid> user)
    {
        var emailConfirmationToken =
            await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var tokenGeneratedBytes = Encoding.UTF8.GetBytes(emailConfirmationToken);
        var tokenEncoded = WebEncoders.Base64UrlEncode(tokenGeneratedBytes);
        var tokenConfirmationLink = ServicesRoutes.BuildEmailConfirmationLink(tokenEncoded, user.Id.ToString());
        return tokenConfirmationLink;
    }

    private async Task<IdentityUser<Guid>> RegisterNewPatientAsync(UserCredentialsDto patientCredentials)
    {
        await EnsureUserNotRegisteredAsync(patientCredentials.Email);
        IdentityUser<Guid> user = new()
        {
            Email = patientCredentials.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = patientCredentials.Email
        };

        var signUpResult = await _userManager.CreateAsync(user, patientCredentials.Password);
        if (signUpResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, UserRoles.Patient);
            return user;
        }

        throw new CredentialValidationFailedException(signUpResult.Errors);
    }

    private async Task<IdentityUser<Guid>> RegisterNewConfirmedUserAsync(UserCredentialsDto userCredentials,
        AccountGenerationDto userCreationRequest)
    {
        await EnsureUserNotRegisteredAsync(userCredentials.Email);
        IdentityUser<Guid> user = new()
        {
            Email = userCredentials.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = userCredentials.Email,
            EmailConfirmed = true
        };

        var signUpResult = await _userManager.CreateAsync(user, userCredentials.Password);
        if (signUpResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, userCreationRequest.Role);
            var claims = PrepareUserClaims(userCreationRequest.EntityId);
            await _userManager.AddClaimsAsync(user, claims);
            return user;
        }

        throw new CredentialValidationFailedException(signUpResult.Errors);
    }

    private async Task EnsureUserNotRegisteredAsync(string email)
    {
        var registeredUser = await _userManager.FindByEmailAsync(email);
        if (registeredUser != null)
        {
            throw new UserAlreadyRegisteredException();
        }
    }

    private IEnumerable<Claim> PrepareUserClaims(Guid profileId)
    {
        var claims = new List<Claim> { new(JwtClaimTypes.CanInteractWithProfileClaim, profileId.ToString()) };
        return claims;
    }
}