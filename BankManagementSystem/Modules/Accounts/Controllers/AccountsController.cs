using BankManagementSystem.Modules.Accounts.Requests;
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

        [HttpPost("add-currency")]
        public ActionResult<AddCurrencyAccountResponse> AddCurrency(AddCurrencyAccountRequest request)
        {
            AddCurrencyAccountResponse response = _accountService.AddCurrencyAccount(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPatch("{accountNumber}/freeze")]
        public IActionResult FreezeAccount(long accountNumber)
        {
            FreezeAccountResponse response = _accountService.FreezeAccount(accountNumber);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }


        [HttpPatch("{accountNumber}/activate")]
        public IActionResult ActivateAccount(long accountNumber)
        {
            ActivateAccountResponse response = _accountService.ActivateAccount(accountNumber);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }


    }

    
}