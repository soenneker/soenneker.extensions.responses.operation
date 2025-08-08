using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soenneker.Dtos.ProblemDetails;
using Soenneker.Responses.Operation;

namespace Soenneker.Extensions.Responses.Operation;

/// <summary>
/// A collection of helpful OperationResponse extension methods.
/// </summary>
public static class OperationResponsesExtension
{
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
    public static IActionResult ToActionResult<T>(this OperationResponse<T> resp)
    {
        if (resp.Succeeded)
        {
            // 204 => no body
            if (resp.StatusCode == StatusCodes.Status204NoContent)
                return new StatusCodeResult(StatusCodes.Status204NoContent);

            // Single allocation path for 2xx with body (200/201/202/…)
            // If you don't need Location headers, ObjectResult is fine.
            return new ObjectResult(resp.Value) {StatusCode = resp.StatusCode == 0 ? 200 : resp.StatusCode};
        }

        ProblemDetailsDto? problem = resp.Problem;

        if (problem is null)
        {
            // Minimal construction only when missing.
            problem = new ProblemDetailsDto
            {
                Title = "Unknown error",
                Status = resp.StatusCode == 0 ? 500 : resp.StatusCode
            };
        }

        return new ObjectResult(problem) {StatusCode = problem.Status ?? resp.StatusCode};
    }
}