using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soenneker.Dtos.ProblemDetails;
using Soenneker.Responses.Operation;
using System;

namespace Soenneker.Extensions.Responses.Operation;

/// <summary>
/// A collection of helpful OperationResponse extension methods.
/// </summary>
public static class OperationResponsesExtension
{
    private static IActionResult ToActionResultCore(bool succeeded, int statusCode, object? value, ProblemDetailsDto? problem)
    {
        if (succeeded)
        {
            if (statusCode == StatusCodes.Status204NoContent)
                return new StatusCodeResult(StatusCodes.Status204NoContent);

            // 2xx with body
            return new ObjectResult(value) {StatusCode = statusCode == 0 ? StatusCodes.Status200OK : statusCode};
        }

        // Ensure a ProblemDetails exists
        ProblemDetailsDto pd = problem ?? new ProblemDetailsDto
        {
            Title = "Unknown error",
            Status = statusCode == 0 ? StatusCodes.Status500InternalServerError : statusCode
        };

        return new ObjectResult(pd) {StatusCode = pd.Status ?? statusCode};
    }

    /// <summary>
    /// Converts the specified <see cref="OperationResponse{T}"/> into an <see cref="IActionResult"/> 
    /// suitable for use in ASP.NET Core MVC controllers.
    /// </summary>
    /// <typeparam name="T">The type of the successful result value contained in the operation response.</typeparam>
    /// <param name="resp">The operation response to convert.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the operation outcome:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// For successful results (<see cref="OperationResponse{T}.Succeeded"/> is <c>true</c>):
    /// returns a 2xx result with the associated value, or a 204 No Content result if 
    /// <see cref="OperationResponse{T}.StatusCode"/> is 204.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// For failed results (<see cref="OperationResponse{T}.Succeeded"/> is <c>false</c>):
    /// returns a result containing the associated <see cref="OperationResponse{T}.Problem"/> details, 
    /// or a default problem response if no details are provided.
    /// </description>
    /// </item>
    /// </list>
    /// </returns>
    public static IActionResult ToActionResult<T>(this OperationResponse<T> resp) =>
        ToActionResultCore(resp.Succeeded, resp.StatusCode, resp.Value, resp.Problem);

    ///<inheritdoc cref="ToActionResult{T}"/>
    public static IActionResult ToActionResult(this OperationResponse resp) => ToActionResultCore(resp.Succeeded, resp.StatusCode, resp.Value, resp.Problem);

    /// <summary>
    /// If the response failed, retypes it to TOut and preserves StatusCode/Problem.
    /// Throws if called on a successful response (use To/Map for that).
    /// </summary>
    public static OperationResponse<TOut> ToFailure<TOut>(this OperationResponse resp)
    {
        if (resp.Succeeded)
            throw new InvalidOperationException("AsFailureOf<> should only be used on failed responses.");

        return new OperationResponse<TOut>
        {
            Succeeded = false,
            StatusCode = resp.StatusCode,
            Problem = resp.Problem,
            Value = default
        };
    }
}