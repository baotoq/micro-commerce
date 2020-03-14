using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Catalog.API.HealthCheck;
using FluentAssertions;
using Grpc.Core;
using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Xunit;

namespace Catalog.API.UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization()); ;

            var healthMock = fixture.Freeze<Mock<Health.HealthClient>>();

            var sut = fixture.Create<BasketHealthCheck>();

            var act = await sut.CheckHealthAsync(null, CancellationToken.None);

            act.Status.Should().Be(HealthStatus.Healthy);
        }
    }

    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(() => new Fixture().Customize(new AutoMoqCustomization()))
        {
        }
    }

    public class InlineAutoMoqDataAttribute : CompositeDataAttribute
    {
        public InlineAutoMoqDataAttribute(params object[] values)
            : base(new InlineDataAttribute(values), new AutoMoqDataAttribute())
        {
        }
    }

    public static class GrpcTestCalls
    {
        public static AsyncUnaryCall<TResponse> AsyncUnaryCall<TResponse>(
            TResponse response, Task<Metadata> responseHeadersAsync = null, Func<Status> getStatusFunc = null,
            Func<Metadata> getTrailersFunc = null, Action disposeAction = null)
        {
            return new AsyncUnaryCall<TResponse>(Task.FromResult(response), responseHeadersAsync, getStatusFunc, getTrailersFunc, disposeAction);
        }

        public static AsyncUnaryCall<TResponse> AsyncUnaryCall<TResponse>(
            Task<TResponse> responseAsync, Task<Metadata> responseHeadersAsync = null, Func<Status> getStatusFunc = null,
            Func<Metadata> getTrailersFunc = null, Action disposeAction = null)
        {
            return new AsyncUnaryCall<TResponse>(responseAsync, responseHeadersAsync, getStatusFunc, getTrailersFunc, disposeAction);
        }
    }
}
