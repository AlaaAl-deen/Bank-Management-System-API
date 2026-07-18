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

        [HttpGet("{customerNumber}")]
        public IActionResult GetUserDetails(int customerNumber)
        {
            GetUserDetailsResponse response =
                _userService.GetUserDetails(customerNumber);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPut("{customerNumber}")]
        public IActionResult UpdateUser(int customerNumber, UpdateUserRequest request)
        {
            UpdateUserResponse response =
                _userService.UpdateUser(customerNumber, request);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPatch("{customerNumber}/deactivate")]
        public IActionResult DeactivateUser(int customerNumber)
        {
            DeactivateUserResponse response =
                _userService.DeactivateUser(customerNumber);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPatch("{customerNumber}/activate")]
        public IActionResult ActivateUser(int customerNumber)
        {
            ActivateUserResponse response =
                _userService.ActivateUser(customerNumber);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("customers")]
        public IActionResult GetAllCustomers()
        {
            GetAllCustomersResponse response =
                _userService.GetAllCustomers();

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}