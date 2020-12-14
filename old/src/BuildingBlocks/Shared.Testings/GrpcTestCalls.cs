using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Testing;

namespace Shared.Testings
{
    public static class GrpcTestCalls
    {
        public static AsyncUnaryCall<TResponse> AsyncUnaryCall<TResponse>(
            Task<TResponse> responseAsync, Task<Metadata> responseHeadersAsync = null, Func<Status> getStatusFunc = null,
            Func<Metadata> getTrailersFunc = null, Action disposeAction = null)
        {
            return TestCalls.AsyncUnaryCall(
                responseAsync,
                responseHeadersAsync,
                getStatusFunc,
                getTrailersFunc,
                disposeAction);
        }

        public static AsyncUnaryCall<TResponse> AsyncUnaryCall<TResponse>(
            TResponse responseAsync, Task<Metadata> responseHeadersAsync = null, Func<Status> getStatusFunc = null,
            Func<Metadata> getTrailersFunc = null, Action disposeAction = null)
        {
            return TestCalls.AsyncUnaryCall(
                Task.FromResult(responseAsync),
                responseHeadersAsync,
                getStatusFunc,
                getTrailersFunc,
                disposeAction);
        }
    }
}
