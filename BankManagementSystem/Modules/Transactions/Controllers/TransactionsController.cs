using Microsoft.AspNetCore.Mvc;
using BankManagementSystem.Modules.Transactions.Requests;
using BankManagementSystem.Modules.Transactions.Responses;
using BankManagementSystem.Modules.Transactions.Services;
using Microsoft.AspNetCore.Authorization;

namespace BankManagementSystem.Modules.Transactions.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _transactionService;
        private int GetCurrentCustomerNumber()
        {
            return int.Parse(User.FindFirst("CustomerNumber")!.Value);
        }

        public TransactionsController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("deposit")]
        public ActionResult<DepositResponse> Deposit([FromBody] DepositRequest request)
        {
            DepositResponse response = _transactionService.Deposit(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("transfer")]
        public ActionResult<TransferResponse> Transfer(
     [FromBody] TransferRequest request)
        {
            TransferResponse response =
                _transactionService.Transfer(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("exchange")]
        public ActionResult<ExchangeResponse> Exchange([FromBody] ExchangeRequest request)
        {
            ExchangeResponse response = _transactionService.Exchange(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{accountNumber}/statement")]
        public IActionResult GetAccountStatement(long accountNumber)
        {
            try
            {
                AccountStatementResponse response = _transactionService.GetAccountStatement(accountNumber);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new AccountStatementResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("my-accounts/{accountNumber}/statement")]
        public IActionResult GetMyAccountStatement(long accountNumber)
        {
            try
            {
                int customerNumber = GetCurrentCustomerNumber();

                AccountStatementResponse response =
                    _transactionService.GetMyAccountStatement(
                        customerNumber,
                        accountNumber);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new AccountStatementResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("my-accounts/transfer")]
        public ActionResult<TransferResponse> TransferForMe(
    [FromBody] TransferRequest request)
        {
            int customerNumber = GetCurrentCustomerNumber();

            TransferResponse response =
                _transactionService.Transfer(
                    request,
                    customerNumber);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("my-accounts/exchange")]
        public ActionResult<ExchangeResponse> ExchangeForMe(
    [FromBody] ExchangeRequest request)
        {
            int customerNumber = GetCurrentCustomerNumber();

            ExchangeResponse response =
                _transactionService.Exchange(
                    request,
                    customerNumber);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}