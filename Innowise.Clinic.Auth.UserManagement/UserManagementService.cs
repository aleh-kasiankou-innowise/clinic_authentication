using System.Text;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Extensions;
using Innowise.Clinic.Auth.Jwt.Interfaces;
using Innowise.Clinic.Auth.Mail.Interfaces;
using Innowise.Clinic.Auth.Persistence.Constants;
using Innowise.Clinic.Auth.UserManagement.Exceptions;
using Innowise.Clinic.Auth.UserManagement.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth.UserManagement;

public class UserManagementService : IUserManagementService
{
    private readonly IEmailHandler _emailHandler;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly ITokenService _tokenGenerator;
    private readonly ITokenRevoker _tokenRevoker;
    private readonly ITokenValidator _tokenValidator;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public UserManagementService(UserManager<IdentityUser<Guid>> userManager, IEmailHandler emailHandler,
        ITokenService tokenGenerator, SignInManager<IdentityUser<Guid>> signInManager, ITokenRevoker tokenRevoker,
        ITokenValidator tokenValidator)
    {
        _userManager = userManager;
        _emailHandler = emailHandler;
        _tokenGenerator = tokenGenerator;
        _signInManager = signInManager;
        _tokenRevoker = tokenRevoker;
        _tokenValidator = tokenValidator;
    }

    public async Task<AuthTokenPairDto> RegisterPatientAsync(PatientCredentialsDto patientCredentials)
    {
        var registeredUser = await RegisterNewPatientAsync(patientCredentials);

        var emailConfirmationLink = await PrepareEmailConfirmationLink(registeredUser);

        await _emailHandler.SendEmailConfirmationLinkAsync(registeredUser.Email, emailConfirmationLink);

        var authTokens =
            await _tokenGenerator.GenerateJwtAndRefreshTokenAsync(registeredUser);

        return authTokens;
    }

    public async Task<AuthTokenPairDto> SignInUserAsync(PatientCredentialsDto patientCredentials)
    {
        var user = await _userManager.FindByEmailAsync(patientCredentials.Email);
        if (user == null) throw new UserNotFoundException();

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
            return _tokenGenerator.GenerateJwtToken(principal);
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

        var user = await _userManager.FindByIdAsync(userId);

        var confirmation = await _userManager.ConfirmEmailAsync(user, emailConfirmationToken);

        if (!confirmation.Succeeded) throw new EmailConfirmationFailedException(confirmation.Errors);
    }

    private async Task<string> PrepareEmailConfirmationLink(IdentityUser<Guid> user)
    {
        var emailConfirmationToken =
            await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var tokenGeneratedBytes = Encoding.UTF8.GetBytes(emailConfirmationToken);
        var tokenEncoded = WebEncoders.Base64UrlEncode(tokenGeneratedBytes);
        var tokenConfirmationLink = Environment.GetEnvironmentVariable("AuthApiUrl") + "auth/email/confirm/" +
                                    tokenEncoded + $"?userid={user.Id}";

        return tokenConfirmationLink;
    }

    private async Task<IdentityUser<Guid>> RegisterNewPatientAsync(PatientCredentialsDto patientCredentials)
    {
        var registeredUser = await _userManager.FindByEmailAsync(patientCredentials.Email);

        if (registeredUser != null) throw new UserAlreadyRegisteredException();

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
}