using HRMS.API.Common;
using HRMS.Application.DTOs;
using HRMS.Application.Features.Countries.Commands.CreateCountry;
using HRMS.Application.Features.Countries.Commands.DeleteCountry;
using HRMS.Application.Features.Countries.Commands.UpdateCountry;
using HRMS.Application.Features.Countries.Queries.GetAllCountries;
using HRMS.Application.Features.Countries.Queries.GetCountryById;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Endpoints;

public static class CountryEndpoints
{
    public static void MapCountryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/countries")
            .WithTags("Countries")
            .RequireAuthorization()
            .RequireRateLimiting("TenantRateLimiter");

        group.MapPost("/", CreateCountry)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateCountry)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteCountry)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", GetCountryById)
            .Produces<ApiResponse<CountryDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        group.MapGet("/", GetAllCountries)
            .Produces<ApiResponse<IReadOnlyList<CountryDto>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateCountry([FromBody] CreateCountryCommand command, ISender sender)
    {
        var result = await sender.Send(command);

        if (result.IsSuccess)
        {
            var response = ApiResponse<Guid>.Ok(result.Value, "Country created successfully.");
            return Results.Created($"/api/countries/{result.Value}", response);
        }

        return Results.BadRequest(ApiResponse<object>.Fail("Failed to create country.", result.Errors.Select(e => e.Message)));
    }

    private static async Task<IResult> UpdateCountry(Guid id, [FromBody] UpdateCountryCommand command, ISender sender)
    {
        if (id != command.Id)
        {
            return Results.BadRequest(ApiResponse<object>.Fail("Id mismatch in route and body."));
        }

        var result = await sender.Send(command);

        if (result.IsSuccess)
        {
            return Results.Ok(ApiResponse<Guid>.Ok(result.Value, "Country updated successfully."));
        }

        if (result.Error?.Severity == HRMS.Application.Common.ErrorSeverity.NotFound)
        {
            return Results.NotFound(ApiResponse<object>.Fail("Country not found."));
        }

        return Results.BadRequest(ApiResponse<object>.Fail("Failed to update country.", result.Errors.Select(e => e.Message)));
    }

    private static async Task<IResult> DeleteCountry(Guid id, ISender sender)
    {
        var result = await sender.Send(new DeleteCountryCommand(id));

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        if (result.Error?.Severity == HRMS.Application.Common.ErrorSeverity.NotFound)
        {
            return Results.NotFound(ApiResponse<object>.Fail("Country not found."));
        }

        return Results.BadRequest(ApiResponse<object>.Fail("Failed to delete country.", result.Errors.Select(e => e.Message)));
    }

    private static async Task<IResult> GetCountryById(Guid id, ISender sender)
    {
        var result = await sender.Send(new GetCountryByIdQuery(id));

        if (result.IsSuccess)
        {
            return Results.Ok(ApiResponse<CountryDto>.Ok(result.Value!));
        }

        return Results.NotFound(ApiResponse<object>.Fail("Country not found."));
    }

    private static async Task<IResult> GetAllCountries(ISender sender)
    {
        var result = await sender.Send(new GetAllCountriesQuery());

        if (result.IsSuccess)
        {
            return Results.Ok(ApiResponse<IReadOnlyList<CountryDto>>.Ok(result.Value!));
        }

        return Results.BadRequest(ApiResponse<object>.Fail("Failed to fetch countries.", result.Errors?.Select(e => e.Message)));
    }
}
