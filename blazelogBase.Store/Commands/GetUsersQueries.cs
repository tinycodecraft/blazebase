using AutoMapper;
using AutoMapper.QueryableExtensions;
using blazelogBase.Shared.Models;
using blazelogBase.Shared.Tools;
using blazelogBase.Store.Dtos;
using blazelogBase.Store.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazelogBase.Store.Commands
{

    public record GetUsersQuery(string AskSearch,int Start,int Size,params SortDescription[] Sorts): IRequest<List<UserDto>>;


    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
    {
        public readonly IBlazeLogDbContext context;
        public readonly IMapper mapper;
        public GetUsersQueryHandler(IBlazeLogDbContext ctx,IMapper mp)
        {
            context = ctx;
            mapper = mp;    
        }
        public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var query = context.CoreUsers.AsQueryable();
            if (request.AskSearch != null)
            {
                query= query.Where(x => x.UserName.Contains(request.AskSearch) || x.Email.Contains(request.AskSearch));
                
            }
            if (request.Sorts.Length > 0)
            {
                query= query.BuildOrder(request.Sorts).Skip(request.Start).Take(request.Size);
            }

            return await query.ProjectTo<UserDto>(mapper.ConfigurationProvider).ToListAsync(cancellationToken);

            
        }
    }

}
