using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace OMAB.Application.Cores;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
            return CreateValidationResponse(failures);

        return await next();
    }
    private static TResponse CreateValidationResponse(IEnumerable<ValidationFailure> failures)
    {
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // 1. Lấy thông báo lỗi (Gộp các lỗi lại thành 1 chuỗi)
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            var statusCode = 400;

            var resultTypeArg = responseType.GetGenericArguments()[0];

            var failureMethod = responseType.GetMethod("Failure");

            if (failureMethod != null)
            {
                return (TResponse)failureMethod.Invoke(null, new object[] { errorMessage, statusCode })!;
            }
        }

        throw new ValidationException(failures);
    }
}
