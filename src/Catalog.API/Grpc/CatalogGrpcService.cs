using System.Threading.Tasks;
using Bshop.Catalog.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Catalog.API.Grpc
{
    public class CatalogGrpcService : CatalogService.CatalogServiceBase
    {
        public override Task<Empty> Ping(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new Empty());
        }
    }
}
