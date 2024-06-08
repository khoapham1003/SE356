using _1015bookstore.viewmodel.System.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using _1015bookstore.websiteadmin.Service;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace _1015bookstore.websiteadmin.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserAPIClient _userAPIClient;
        private readonly IConfiguration _config;

        public LoginController(IUserAPIClient userAPIClient, IConfiguration config)
        {
            _userAPIClient = userAPIClient;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var response = await _userAPIClient.Authenticate(request);

            if (!response.Status)
            {
                ViewBag.error = response.Message;
                return View();
            }
            // return RedirectToAction("Login", "User", response.data);

            var token = (string)response.Data["sUser_tokenL"];

            var userPrincipal = this.ValidateToken(token);
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3),
                IsPersistent = true
            };

            HttpContext.Session.SetString("token", token);

            await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        userPrincipal,
                        authProperties);

            // Ghi log sau khi đăng nhập thành công
            string logMessage = $"{DateTime.Now}: User {request.sUser_username} logged in successfully.";
            WriteLog(logMessage);

            return RedirectToAction("Index", "Home");
        }

        private ClaimsPrincipal ValidateToken(string jwtToken)
        {
            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken;

            TokenValidationParameters validationParameters = new TokenValidationParameters();

            validationParameters.RequireAudience = false;
            validationParameters.ValidateAudience = false;
            validationParameters.ValidateIssuer = false;

            validationParameters.ValidateLifetime = true;

            validationParameters.ValidateIssuerSigningKey = true;
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);

            return principal;
        }

        private void WriteLog(string message)
        {
            string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", "LoginLogs.txt");

            // Tạo thư mục nếu nó không tồn tại
            if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            }

            // Ghi đè nội dung vào file txt
            using (StreamWriter writer = new StreamWriter(logFilePath, true)) // Sử dụng `true` để thêm vào file
            {
                writer.WriteLine(message);
            }
        }
    }
}
