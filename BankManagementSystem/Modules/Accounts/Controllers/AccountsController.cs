using BankManagementSystem.Common.Constants;
using BankManagementSystem.Modules.Accounts.Requests;
using BankManagementSystem.Modules.Accounts.Responses;
using BankManagementSystem.Modules.Accounts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankManagementSystem.Modules.Accounts.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;
        private int GetCurrentCustomerNumber()
        {
            return int.Parse(User.FindFirst("CustomerNumber")!.Value);
        }

        public AccountsController(AccountService accountService)
        {
            _accountService = accountService;
        }
        [Authorize(Roles = "Customer")]
        // [HttpGet("customer/{customerNumber}")]
        [HttpGet("my-accounts")]
        public ActionResult<GetAccountsResponse> GetMyAccounts()
        {
            int customerNumber = int.Parse(
        User.FindFirst("CustomerNumber")!.Value);

            GetAccountsResponse response =
                _accountService.GetAccounts(customerNumber);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("customer/{customerNumber}")]
        public ActionResult<GetAccountsResponse> GetCustomerAccounts(int customerNumber)
        {
            GetAccountsResponse response =
                _accountService.GetAccounts(customerNumber);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("my-accounts/add-currency")]
        public ActionResult<AddCurrencyAccountResponse> AddCurrencyForMe(
     AddMyCurrencyAccountRequest request)
        {
            AddCurrencyAccountRequest serviceRequest =
                new AddCurrencyAccountRequest
                {
                    CustomerNumber = GetCurrentCustomerNumber(),
                    CurrencyId = request.CurrencyId
                };

            AddCurrencyAccountResponse response =
                _accountService.AddCurrencyAccount(serviceRequest);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("customer/{customerNumber}/add-currency")]
        public ActionResult<AddCurrencyAccountResponse> AddCurrencyForCustomer(
    int customerNumber,
    AddMyCurrencyAccountRequest request)
        {
            AddCurrencyAccountRequest serviceRequest =
                new AddCurrencyAccountRequest
                {
                    CustomerNumber = customerNumber,
                    CurrencyId = request.CurrencyId
                };

            AddCurrencyAccountResponse response =
                _accountService.AddCurrencyAccount(serviceRequest);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }


        [Authorize(Roles = "Admin")]
        [HttpPatch("{accountNumber}/freeze")]
        public IActionResult FreezeAccount(long accountNumber)
        {
            FreezeAccountResponse response = _accountService.FreezeAccount(accountNumber);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{accountNumber}/activate")]
        public IActionResult ActivateAccount(long accountNumber)
        {
            ActivateAccountResponse response = _accountService.ActivateAccount(accountNumber);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{accountNumber}/close")]
        public IActionResult CloseAccount(long accountNumber)
        {
            CloseAccountResponse response = _accountService.CloseAccount(accountNumber);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{accountNumber}")]
        public IActionResult GetAccountDetails(long accountNumber)
        {
            GetAccountDetailsResponse response = _accountService.GetAccountDetails(accountNumber);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }


    }

    
}