using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Orquestrator.Service.Contracts.Events;

namespace Entrypoint.Service.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class DepositsController : ControllerBase
    {
        private readonly IBus _bus;

        public DepositsController(IBus bus)
            => _bus = bus ?? throw new ArgumentNullException(nameof(bus));

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] BankDepositTransactionWasReceived value, CancellationToken cancellationToken)
            => await _bus.Publish(value, cancellationToken);
    }
}
