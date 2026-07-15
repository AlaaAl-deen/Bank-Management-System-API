using BankManagementSystem.Modules.Users.Requests;
using BankManagementSystem.Modules.Users.Responses;
using BankManagementSystem.Modules.Users.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankManagementSystem.Modules.Users.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController()
        {
            _userService = new UserService();
        }

        [HttpPost]
        public ActionResult<CreateUserResponse> CreateUser(CreateUserRequest request)
        {
            CreateUserResponse response = _userService.CreateUser(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}