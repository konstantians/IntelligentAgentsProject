using IntelligentAgents.AuthLibrary.Models;
using IntelligentAgents.AuthLibrary.Models.ResponseModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IntelligentAgents.AuthLibrary.AuthLogic;


//Logging Messages Start With 2 For Success/Information, 3 For Warning And 4 For Error(Almost like HTTP Status Codes). The range is 0-99, for example 1000. 
//The range of codes for this class is is 200-299, for example 2200 or 2299.
public class AuthenticationProcedures : IAuthenticationProcedures
{
    private readonly AppIdentityDbContext _identityDbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<AuthenticationProcedures> _logger;
    private readonly IConfiguration _config;
    private readonly IHelperMethods _helperMethods;
    public AuthenticationProcedures(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppIdentityDbContext appIdentityDbContext,
        IConfiguration config, IHelperMethods helperMethods, ILogger<AuthenticationProcedures> logger = null!)
    {
        _identityDbContext = appIdentityDbContext;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger ?? NullLogger<AuthenticationProcedures>.Instance;
        _helperMethods = helperMethods;
        _config = config;
    }

    public async Task<ReturnUserAndCodeResponseModel> GetCurrentUserByTokenAsync(string accessToken)
    {
        try
        {
            return await _helperMethods.StandardTokenAndUserValidationProcedures(accessToken, new EventId(3200, "GetCurrentUserByToken"));
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4200, "CurrentUserCouldNotBeRetrieved"), ex, "An error occurred while retrieving logged in user. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnTokenAndCodeResponseModel> SignInAsync(string email, string password, bool isPersistent)
    {
        try
        {
            AppUser? user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                _logger.LogWarning(new EventId(3211, "SignInFailureDueToNullUser"), "Tried to sign in null user= Email={Email}.", email);
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenEmail);
            }

            if (!_helperMethods.IsEmailConfirmed(user, new EventId(3212, "SignInFailureDueToUnconfirmedEmail"), "The sign in process could not continue, because the user account is not activated. Email= {Email}."))
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);


            if (await _helperMethods.IsAccountLockedOut(user, new EventId(3213, "SignInFailureDueToAccountBeingLocked"), "User with locked account tried to sign in. Email={Email}."))
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountLocked);

            var result = await _userManager.CheckPasswordAsync(user!, password)!;
            if (!result)
            {
                await _userManager.AccessFailedAsync(user);
                if (await _userManager.GetAccessFailedCountAsync(user) >= 10)
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(10)));

                _logger.LogWarning(new EventId(3214, "UserSignInFailureDueToInvalidCredentials"), "User could not be signed in, because of invalid credentials. Email={Email}.", email);
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.InvalidCredentials);
            }

            //Get all the roles of the user
            List<string> userRoles = new List<string>(await _userManager.GetRolesAsync(user));
            string accessToken = GenerateTokenAsync(user!, isPersistent);
            _logger.LogInformation(new EventId(2203, "UserSignInSuccess"), "Successfully signed in user. Username={Email}, IsPersistent={IsPersistent}.", email, isPersistent);

            return new ReturnTokenAndCodeResponseModel(accessToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4204, "UserSignInFailure"), ex, "An error occurred while trying to sign in the user. " +
                "Email={Email}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    private string GenerateTokenAsync(AppUser user, bool isPersistent = false)
    {
        // Create claims for the updatedAppUser
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
        };

        // Generate JWT accessToken
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = isPersistent ? DateTime.Now.AddDays(30) : DateTime.Now.AddDays(1);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
