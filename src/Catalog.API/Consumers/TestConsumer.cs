using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Catalog.API.Consumers
{
    public class Test : BaseMessage
    {
        public string Message { get; set; }
    }

    public class TestConsumer : BaseConsumer<Test>
    {
        private readonly ILogger<TestConsumer> _logger;

        public TestConsumer(ILogger<TestConsumer> logger)
        {
            _logger = logger;
        }

        public override async Task Consume(ConsumeContext<Test> context)
        {
            await Task.Delay(5000);
            var rand = new Random().Next(0, 2);
            if (rand == 0)
            {
                throw new Exception("Error");
            }
        }
    }
}
