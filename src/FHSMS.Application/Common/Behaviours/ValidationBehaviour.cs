using FHSMS.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace FHSMS.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline step that runs all FluentValidation validators registered for
/// a given request before the handler executes. Keeps handlers free of manual
/// "if (...) throw" input-validation boilerplate.
/// </summary>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new FHSMS.Application.Common.Exceptions.ValidationException(failures);
        }

        return await next();
    }
}
