using System;
using FluentValidation;
using MediatR;
using OMAB.Application.Cores;
using OMAB.Application.Interfaces;

namespace OMAB.Application.Features.Identities;

public class ChangePassword
{
    public record Command(string CurrentPassword, string NewPassword) : IRequest<Result<Unit>>;
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
        }
    }

    public class Handler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IUserAccessor userAccessor) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var currentUserId = userAccessor.GetCurrentUserId();
            if (currentUserId == null)
                return Result<Unit>.Failure("User not authenticated.", 401);
            var currentUser = await userRepository.GetByIdAsync(currentUserId.Value, ct);
            if (currentUser == null)
                return Result<Unit>.Failure("User not found.", 404);
            if (!passwordHasher.Verify(request.CurrentPassword, currentUser.PasswordHash))
            {
                return Result<Unit>.Failure("Current password is incorrect.", statusCode: 400);
            }

            currentUser.ChangePassword(newPasswordHash: passwordHasher.Hash(request.NewPassword));
            userRepository.Update(currentUser);
            var result = await unitOfWork.SaveChangesAsync(ct);
            if (result <= 0) return Result<Unit>.Failure("Failed to change password.", 500);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
