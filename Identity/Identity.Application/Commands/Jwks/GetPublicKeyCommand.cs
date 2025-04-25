using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Application.Commands.Jwks
{
    public class GetPublicKeyCommand : IRequest<JsonWebKey>
    {
        public class GetPublicKeyCommandHandler : IRequestHandler<GetPublicKeyCommand, JsonWebKey>
        {
            private readonly IIdentity identity;

            public GetPublicKeyCommandHandler(IIdentity identity)
                => this.identity = identity;

            public Task<JsonWebKey> Handle(
                GetPublicKeyCommand request,
                CancellationToken cancellationToken)
                => Task.FromResult(this.identity.GetPublicKey());
        }
    }
}
