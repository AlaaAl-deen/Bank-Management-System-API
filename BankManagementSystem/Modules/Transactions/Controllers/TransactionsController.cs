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

        [HttpPost("transfer")]
        public ActionResult<TransferResponse> Transfer([FromBody] TransferRequest request)
        {
            TransferResponse response = _transactionService.Transfer(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

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
    }
}