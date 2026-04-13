using Cortex.Mediator.Queries;
using GovcoreBse.Store.Setup;

using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Store.Commands
{
    
    public record GetFileQuery(long id) : IQuery<ErrorOr<FileItemDto>>;

    public class GetFileQueryHandler : IQueryHandler<GetFileQuery, ErrorOr<FileItemDto>>
    {

        private readonly IBlazeLogDbContext context;
        public GetFileQueryHandler(IBlazeLogDbContext ctx)
        {
            context = ctx;

        }
        public async Task<ErrorOr<FileItemDto>> Handle(GetFileQuery query, CancellationToken cancellationToken)
        {
            var data = context.CoreFileDocs.AsQueryable();
            var user = await data.FirstOrDefaultAsync(x=> x.Id == query.id, cancellationToken); 
            if (user != null)
                return user.Adapt<FileItemDto>();
            return Error.NotFound("FileNotFound", $"File not found for id {query.id}");
        }

    }

}
