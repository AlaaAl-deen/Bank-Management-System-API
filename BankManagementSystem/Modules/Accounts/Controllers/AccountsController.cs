using BankManagementSystem.Modules.Accounts.Responses;
using BankManagementSystem.Modules.Accounts.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankManagementSystem.Modules.Accounts.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountsController()
        {
            _accountService = new AccountService();
        }

        [HttpGet("{customerNumber}")]
        public ActionResult<GetAccountsResponse> GetAccounts(int customerNumber)
        {
            GetAccountsResponse response =
                _accountService.GetAccounts(customerNumber);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}