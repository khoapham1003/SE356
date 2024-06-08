using _1015bookstore.application.System.Users;
using _1015bookstore.viewmodel.Catalog.Products;
using _1015bookstore.viewmodel.System.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace _1015bookstore.webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly string _logFilePath;

        public UserController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            var logFileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
            if (!Directory.Exists(logFileDirectory))
            {
                Directory.CreateDirectory(logFileDirectory);
            }
            _logFilePath = Path.Combine(logFileDirectory, "UserActivityLogs.txt");

        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.Authencate(request);
                if (!result.Status)
                {
                    string logMessage1 = $"{DateTime.Now}: Failed login attempt for user {request.sUser_username}.";
                    WriteLog(logMessage1);

                    return StatusCode(result.CodeStatus, result.Message);
                }

                // Ghi log sau khi đăng nhập thành công
                string logMessage = $"{DateTime.Now}: User {request.sUser_username} logged in successfully.";
                WriteLog(logMessage);

                return StatusCode(result.CodeStatus, result.Data);
            }
            catch (Exception ex)
            {
                string errorLogMessage = $"{DateTime.Now}: Exception during login for user {request.sUser_username}. Exception: {ex.Message}";
                WriteLog(errorLogMessage);
                return StatusCode(500, ex.Message);
            }
        }

        private void WriteLog(string logMessage)
        {
            string logDirectory = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            string combinedContent;
            using (StreamReader reader = new StreamReader(_logFilePath))
            {
                string currentContent = reader.ReadToEnd();
                combinedContent = logMessage + Environment.NewLine + currentContent;
            }
            using (StreamWriter writer = new StreamWriter(_logFilePath, false))
            {

                writer.WriteLine(combinedContent);

            }
        }
       

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.Register(request);
                string logMessage = $"{DateTime.Now}: User {request.sUser_username} registered successfully.";
                WriteLog(logMessage);

                return StatusCode(result.CodeStatus, result.Message);
            }
            catch (Exception ex)
            {
                string logMessage = $"{DateTime.Now}: Exception occurred while registering user {request.sUser_username}. Exception: {ex.Message}";
                WriteLog(logMessage);

                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("forgotpassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody][Required(ErrorMessage = "Please enter your Email!")][EmailAddress(ErrorMessage = "The E-mail is wrong format!")] string email)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.ForgotPassword(email);
                string logMessage = $"{DateTime.Now}: User with email {email} requested password reset.";
                WriteLog(logMessage);

                if (!result.Status)
                {
                    return StatusCode(result.CodeStatus, result.Message);
                }
                return StatusCode(result.CodeStatus, result.Data);
            }
            catch (Exception ex)
            {
                string logMessage = $"{DateTime.Now}: Exception occurred while processing password reset request for user with email {email}. Exception: {ex.Message}";
                WriteLog(logMessage);

                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("confirmCodeforgotpassword")]
        [AllowAnonymous]
        public async Task<IActionResult> CofirmCodeForgotPassword(ConfirmCodeFPRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.CofirmCodeForgotPassword(request);
                if (!result.Status)
                    return StatusCode(result.CodeStatus, result.Message);
                else
                    return StatusCode(result.CodeStatus, result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPost("ChangePasswordForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePasswordForgotPassword(ChangePasswordFPRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.ChangePasswordForgotPassword(request);
                if (!result.Status)
                {
                    string logMessage = $"{DateTime.Now}: Failed to change password for user with email. Error: {result.Message}";
                    WriteLog(logMessage);
                    return StatusCode(result.CodeStatus, result.Message);
                }
                else
                {
                    string logMessage = $"{DateTime.Now}: Password changed successfully for user with email .";
                    WriteLog(logMessage);
                    return StatusCode(result.CodeStatus, result.Message);
                }
            }
            catch (Exception ex)
            {
                string logMessage = $"{DateTime.Now}: Exception occurred while changing password for user with email. Exception: {ex.Message}";
                WriteLog(logMessage);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.ChangePassword(request);
                string logMessage = $"{DateTime.Now}: User {User.Identity.Name} changed their password successfully.";
                WriteLog(logMessage);

                if (!result.Status)
                    return StatusCode(result.CodeStatus, result.Message);
                else
                {
                    string logMessage1 = $"{DateTime.Now}: User {User.Identity.Name} changed their password Failed.";
                    WriteLog(logMessage1);
                    return StatusCode(result.CodeStatus, result.Message);

                }
            }
            catch (Exception ex)
            {
                string logMessage = $"{DateTime.Now}: Exception occurred while changing password for user {User.Identity.Name}. Exception: {ex.Message}";
                WriteLog(logMessage);

                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{gUser_id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(Guid gUser_id)
        {
            try
            {
                var result = await _userService.User_GetById(gUser_id);
                if (!result.Status)
                {
                    return StatusCode(result.CodeStatus, result.Message);
                }
                return StatusCode(result.CodeStatus, result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("updateinfor")]
        [Authorize]
        public async Task<IActionResult> User_UpdateInfor([FromBody] UserUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.User_UpdateInfor(request);
                if (!result.Status)
                {
                    string logMessage = $"{DateTime.Now}: Failed to update user information for user {User.Identity.Name}. Error: {result.Message}";
                    WriteLog(logMessage);
                    return StatusCode(result.CodeStatus, result.Message);
                }
                else
                {
                    string logMessage = $"{DateTime.Now}: User information updated successfully for user {User.Identity.Name}.";
                    WriteLog(logMessage);
                    return StatusCode(result.CodeStatus, result.Message);
                }
            }
            catch (Exception ex)
            {
                string logMessage = $"{DateTime.Now}: Exception occurred while updating user information for user {User.Identity.Name}. Exception: {ex.Message}";
                WriteLog(logMessage);
                return StatusCode(500, ex.Message);
            }
        }


        //http://localhost:port/api/user/public-paging
        [HttpGet("admin-paging-keyword")]
        [Authorize]
        public async Task<IActionResult> User_GetUserByKeyWordPagingAdmin([FromQuery] GetUserByKeyWordPagingRequest request)
        {
            try
            {
                var pageResult = await _userService.User_GetUserByKeyWordPagingAdmin(request);
                return Ok(pageResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //http://localhost:port/api/user/createadmin
        [HttpPost("createadmin")]
        [Authorize]
        public async Task<IActionResult> User_CreateAdmin([FromBody] RegisterAdminRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _userService.User_CreateAdmin(request);
                return StatusCode(result.CodeStatus, result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("admin-getall")]
        [Authorize]
        public async Task<IActionResult> User_GetAllAdmin()
        {
            try
            {
                var result = await _userService.User_GetAllAdmin();
                return StatusCode(result.CodeStatus, result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
