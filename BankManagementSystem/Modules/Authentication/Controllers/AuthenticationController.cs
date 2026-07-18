using BankManagementSystem.Common.Constants;
using BankManagementSystem.Modules.Authentication.Requests;
using BankManagementSystem.Modules.Authentication.Responses;
using BankManagementSystem.Modules.Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankManagementSystem.Modules.Authentication.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;


        public AuthenticationController(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            LoginResponse response = _authenticationService.Login(request);

            if (!response.Success)
            {
                if (response.Message == "Invalid password." ||
                    response.Message == "Customer not found.")
                {
                    return Unauthorized(response);
                }

                return BadRequest(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("change-password")]
        public ActionResult<ChangePasswordResponse> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            ChangePasswordResponse response = _authenticationService.ChangePassword(request);

            if (!response.Success)
            {
                if (response.Message == "Customer not found." ||
                    response.Message == "Current password is incorrect.")
                {
                    return Unauthorized(response);
                }

                return BadRequest(response);
            }

            return Ok(response);
        }
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                Success = true,
                Message = "Logout successful. Please remove the token from the client."
            });
        }
    }
}