using FluentValidation.Results;

namespace MicroCommerce.CartService.Application.Exceptions;

[Serializable]
public class InvalidValidationException : Exception
{
    public InvalidValidationException(string message = "One or more validation failures have occurred.") : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public InvalidValidationException(IEnumerable<ValidationFailure> failures, string message = "") : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}
