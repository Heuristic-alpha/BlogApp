using BlogApp.Infrastructures;
using BlogApp.Models.DataModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorAdminPanel.Services
{
    public class AuthServices : AuthenticationStateProvider
    {
        public const string RoleClaimType = "role";
        public const string Bearer = "Bearer";
        public const string Authorization = "Authorization";

        public const string ApiLoginPath = "/api/Auth/Login";
        public const string ApiTokenValidationPath = "/api/Auth/ValidateToken";

        public const string LoginPagePath = "auth/login";
        public const string LogoutPagePath = "auth/logout";

        public ClaimsPrincipal User { get; private set; } = new ClaimsPrincipal(new ClaimsIdentity());

        private HttpClient _httpClient;
        private IJSRuntime _jSRuntime;
        private JsonSerializerOptions _webJsonSerializerOption;
        private ILogger<AuthServices> _logger;

        public AuthServices(HttpClient httpClient, IJSRuntime jSRuntime, ILogger<AuthServices> logger, JsonSerializerOptions webJsonSerializerOption)
        {
            _httpClient = httpClient;     
            _jSRuntime = jSRuntime;
            _webJsonSerializerOption = webJsonSerializerOption;
            _logger = logger;

            AuthenticationStateChanged += OnAuthStateChanged;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? jWTToken = GetJWTTokenFromHeader();
            jWTToken ??= await GetJWTTokenFromCookieAsync();

            if (jWTToken == null)
            {
                Task<AuthenticationState> task = Task.FromResult(CreateEmptyAuthState());
                NotifyAuthenticationStateChanged(task);
                return task.Result;
            }
            else
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                try
                {
                    JwtSecurityToken security = handler.ReadJwtToken(jWTToken);
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(security.Claims, "JWT", nameType: JwtRegisteredClaimNames.UniqueName, roleType: RoleClaimType);
                    ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentity);

                    Task<AuthenticationState> task = Task.FromResult(new AuthenticationState(principal));
                    NotifyAuthenticationStateChanged(task);
                    return task.Result;
                }
                catch
                {
                    Task<AuthenticationState> task = Task.FromResult(CreateEmptyAuthState());
                    NotifyAuthenticationStateChanged(task);
                    return task.Result;
                }
            }
        }
        public async Task<bool> IsJWTTokenValid()
        {
            string? token = GetJWTTokenFromHeader();
            token ??= await GetJWTTokenFromCookieAsync();
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            else
            {
                var response = await _httpClient.PostAsJsonAsync<string>(ApiTokenValidationPath, token);
                return JsonSerializer.Deserialize<bool>(await response.Content.ReadAsStringAsync(), _webJsonSerializerOption);
            }
        }
        public async Task<LoginResult> LoginAsync(LoginDetails loginDetails)
        {
            var response = await _httpClient.PostAsJsonAsync<LoginDetails>(ApiLoginPath, loginDetails);
            string? json = await response.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResult>(json!, _webJsonSerializerOption)!;
            if (loginResult.Success)
            {
                SetJWTBearerHeader(loginResult.Token);
                await SetCookie(Constants.JWTAuthentication.JWTAuthToken, loginResult.Token, Constants.JWTAuthentication.TokenValidationLifeTimeInHours);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
            return loginResult;
        }
        public async Task LogoutAsync()
        {
            ClearJWTBearerHeader();
            // Clear token in cookie storage
            await SetCookie(Constants.JWTAuthentication.JWTAuthToken, string.Empty, -1);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private async Task SetCookie(string name, string value, int hours)
        {
            await _jSRuntime.InvokeVoidAsync("BlazorExtensions.SetCookie", name, value, hours);
        }
        private ValueTask<string> GetCookie(string name)
        {
            return _jSRuntime.InvokeAsync<string>("BlazorExtensions.GetCookie", name);
        }
        private void ClearJWTBearerHeader()
        {
            if (_httpClient.DefaultRequestHeaders.TryGetValues(Authorization, out var values))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
        private void SetJWTBearerHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Add(Authorization, $"{Bearer} {token}");
        }
        private string? GetJWTTokenFromHeader()
        {
            string? token = null;
            if (_httpClient.DefaultRequestHeaders.TryGetValues(Authorization, out var values))
            {
                // Parse token form header [Bearer eysdhvsrshtbbvsrytryfadganrnt...]
                try
                {
                    token = values.ToArray()[0].Split(" ")[1];
                }
                catch (Exception e)
                {
                    throw new FormatException($"Cant parse JWT Token from {Authorization} request header.", e);
                }
            }
            return token;
        }
        private async Task<string?> GetJWTTokenFromCookieAsync()
        {
            string result = await GetCookie(Constants.JWTAuthentication.JWTAuthToken);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            else
            {
                return result;
            }
        }
        private AuthenticationState CreateEmptyAuthState()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(principal);
        }
        private void OnAuthStateChanged(Task<AuthenticationState> authenticationState)
        {
            User = authenticationState.Result.User;
        }
    }
}
