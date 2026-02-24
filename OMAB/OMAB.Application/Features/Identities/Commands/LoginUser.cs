using System;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Interfaces;

namespace OMAB.Application.Features.Identities.Commands;

public class LoginUser
{
    public record Command(string Email, string Password) : IRequest<Result<string>>;

    public class Handler(IUserRepository userRepo, IPasswordHasher hasher, IJwtGenerator jwtGenerator)
        : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken ct)
        {
            var user = await userRepo.GetByEmailAsync(request.Email, ct);
            if (user == null)
                return Result<string>.Failure("Invalid credentials", 400);

            if (!hasher.Verify(request.Password, user.PasswordHash))
                return Result<string>.Failure("Invalid credentials", 400);

            var token = jwtGenerator.GenerateToken(user);
            return Result<string>.Success(token);
        }
    }
}
