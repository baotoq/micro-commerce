using System.Threading.Tasks;
using Bshop.Shared.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Shared.Grpc
{
    public class PingGrpcService : PingService.PingServiceBase
    {
        public override Task<Empty> Ping(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new Empty());
        }
    }
}
