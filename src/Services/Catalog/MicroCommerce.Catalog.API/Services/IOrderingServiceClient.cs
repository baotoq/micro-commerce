using System.Threading.Tasks;
using MicroCommerce.Ordering.API;

namespace MicroCommerce.Catalog.API.Services
{
    public interface IOrderingServiceClient
    {
        Task<HelloReply> SayHello(HelloRequest request);
    }
}