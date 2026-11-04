using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.Domain.Diagnostics;
using Temporalio.Api.Enums.V1;
using Temporalio.Client;

namespace MicroCommerce.ApiService.Features.Diagnostics;

public sealed record StartSampleProcessRequest(int? DelaySeconds);

public sealed class StartSampleProcessValidator : Validator<StartSampleProcessRequest>
{
    public StartSampleProcessValidator()
    {
        RuleFor(x => x.DelaySeconds)
            .NotNull().WithMessage("delaySeconds is required.")
            .InclusiveBetween(1, 300).WithMessage("delaySeconds must be an integer from 1 to 300.");
    }
}

/// <summary>
/// Starts a sample durable process. The workflow creates the record as its first step, so this
/// endpoint writes nothing to the database (research R5).
/// </summary>
public sealed class StartSampleProcessEndpoint(ITemporalClient temporal, TimeProvider timeProvider)
    : Endpoint<StartSampleProcessRequest, SampleProcessResponse>
{
    public override void Configure()
    {
        Post("/diagnostics/sample-processes");
        Policies(AuthPolicies.PlatformOperator);
        Summary(s =>
        {
            s.Summary = "Start a sample durable process";
            s.Responses[202] = "The process was accepted. It may take a moment before GET finds it.";
            s.Responses[400] = "delaySeconds is missing or outside 1 to 300.";
            s.Responses[401] = "Not signed in.";
            s.Responses[403] = "Not a Platform Operator.";
        });
    }

    public override async Task HandleAsync(StartSampleProcessRequest req, CancellationToken ct)
    {
        var id = SampleProcessId.New();
        var delay = TimeSpan.FromSeconds(req.DelaySeconds!.Value);
        var startedAt = timeProvider.GetUtcNow();

        await temporal.StartWorkflowAsync(
            (SampleProcessWorkflow wf) => wf.RunAsync(id.Value, delay, startedAt),
            new WorkflowOptions(SampleProcessWorkflow.WorkflowId(id.Value), SampleProcessWorkflow.TaskQueue)
            {
                IdReusePolicy = WorkflowIdReusePolicy.RejectDuplicate,
                Rpc = new RpcOptions { CancellationToken = ct },
            });

        HttpContext.Response.Headers.Location = $"/api/diagnostics/sample-processes/{id.Value}";
        await Send.ResponseAsync(
            new SampleProcessResponse(id.Value, nameof(SampleProcessStatus.Running), req.DelaySeconds.Value, startedAt, null, null),
            StatusCodes.Status202Accepted,
            ct);
    }
}
