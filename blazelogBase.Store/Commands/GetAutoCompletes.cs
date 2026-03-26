using AutoMapper;
using blazelogBase.Shared.ErrorOr;
using blazelogBase.Store.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazelogBase.Store.Commands;

public record GetAutoCompleteQuery(CN.AutoSuggestType returntype, string userid, string? search = null) : IRequest<ErrorOr<KeyValuePair<string, string>[]>>;
public class GetAutoCompleteQueryHandler : IRequestHandler<GetAutoCompleteQuery, ErrorOr<KeyValuePair<string, string>[]>>
{
    public readonly IBlazeLogDbContext context;
    public readonly IMapper mapper;
    public GetAutoCompleteQueryHandler(IBlazeLogDbContext ctx, IMapper mp)
    {
        context = ctx;
        mapper = mp;
    }

    public async Task<ErrorOr<KeyValuePair<string, string>[]>> Handle(GetAutoCompleteQuery request, CancellationToken cancellationToken)
    {
        var needSearch = !string.IsNullOrEmpty(request.search);
        var searchvalue = request.search ?? "";
        switch (request.returntype)
        {
            case CN.AutoSuggestType.Engineers:
                var engineers = await context.CoreUsers
                    .Where(e => !e.Disabled && (!needSearch || e.Post.Contains(searchvalue)))
                    .Select(e => e.Post)
                    .ToArrayAsync(cancellationToken);
                return engineers.Select(y => new KeyValuePair<string, string>(y, y)).ToArray();

            default:
                return Error.NotFound("AutoCompleteNotFound", $"AutoComplete not found for type {request.returntype}");
        }

    }
}
