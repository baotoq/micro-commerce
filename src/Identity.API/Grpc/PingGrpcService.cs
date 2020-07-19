using System.Threading.Tasks;
using Bshop.V1.Common;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Identity.API.Grpc
{
    public class PingGrpcService : PingService.PingServiceBase
    {
        public override Task<Empty> Ping(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new Empty());
        }
    }
}
