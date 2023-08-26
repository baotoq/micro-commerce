﻿using MediatR;

namespace Application;

public class PingCommand : IRequest<string>
{
}

public class PingCommandHandler : IRequestHandler<PingCommand, string>
{
    public Task<string> Handle(PingCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong");
    }
}
