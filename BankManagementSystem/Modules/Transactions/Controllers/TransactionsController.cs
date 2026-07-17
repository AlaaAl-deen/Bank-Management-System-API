using Microsoft.AspNetCore.Mvc;
using BankManagementSystem.Modules.Transactions.Requests;
using BankManagementSystem.Modules.Transactions.Responses;
using BankManagementSystem.Modules.Transactions.Services;

namespace BankManagementSystem.Modules.Transactions.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionsController()
        {
            _transactionService = new TransactionService();
        }

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
    }
}