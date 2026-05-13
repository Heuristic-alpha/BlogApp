using BlogApp.Models.DataModels;
using BlogApp.Models;
using BlogApp.Infrastructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {       
        private UserManager<IdentityAppUser> _userManager;
        private SignInManager<IdentityAppUser> _signInManager;
        private IConfiguration _configuration;
        private TokenValidationParameters _tokenValidationParameters;
        private byte[] _jwtSecret = Array.Empty<byte>();
        private ILogger<AuthController> _logger;

        public AuthController(UserManager<IdentityAppUser> userManager, SignInManager<IdentityAppUser> signInManager, IConfiguration configuration, ILogger<AuthController> logger, TokenValidationParameters tokenValidationParameters)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _tokenValidationParameters = tokenValidationParameters;
            _logger = logger;

            string? strVal = _configuration[Constants.JWTAuthentication.JWTSecretName];
            if (strVal != null)
            {
                _jwtSecret = Encoding.ASCII.GetBytes(strVal);
            }
            else
                throw new KeyNotFoundException($"Not Found any {Constants.JWTAuthentication.JWTSecretName} entry on configuration file");
        }

        [HttpPost]
        public async Task<ActionResult<LoginResult>> Login([FromBody] LoginDetails loginDetails)
        {
            LoginResult loginResult = new();

            IdentityAppUser? identityUser = await _userManager.FindByNameAsync(loginDetails.UserName);
            bool passwordIsCorrect = false;
            if (identityUser != null)
            {
                var signInResult = await _signInManager.CheckPasswordSignInAsync(identityUser, loginDetails.Password, true);
                if (signInResult.Succeeded)
                {
                    passwordIsCorrect = true;
                }
            }
            if (passwordIsCorrect && identityUser != null)
            {
                // Create claims for user
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, identityUser.UserName!));
                claims.Add(new Claim(ClaimTypes.Email, identityUser.Email!));
                var roles = await _userManager.GetRolesAsync(identityUser);
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

                SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(Constants.JWTAuthentication.TokenValidationLifeTimeInHours),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_jwtSecret), SecurityAlgorithms.HmacSha256Signature)
                };

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                SecurityToken token = handler.CreateToken(descriptor);

                loginResult.Success = true;
                loginResult.Token = handler.WriteToken(token);
                return Ok(loginResult);
            }
            else
            {
                loginResult.Success = false;
                loginResult.Error = $"UserName or Password is incorrect.";
                return Unauthorized(loginResult);
            }
        }

        [HttpPost]
        public ActionResult<bool> ValidateToken([FromBody] string token)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            try
            {
                var p = handler.ValidateToken(token, _tokenValidationParameters, out _);
                if (p != null) { return true; }
                else { return false; }
            }
            catch
            {
                return false;
            }
        }
    }
}
