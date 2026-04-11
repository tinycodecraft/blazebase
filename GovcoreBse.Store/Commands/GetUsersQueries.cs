
using AgileObjects.AgileMapper.Extensions;
using Cortex.Mediator.Queries;
using GovcoreBse.Shared.Tools;
using GovcoreBse.Store.Models;
using GovcoreBse.Store.Setup;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GovcoreBse.Store.Commands
{

    public record GetUserQuery(string userId) : IQuery<ErrorOr<UserDto>>;

    public class GetUserQueryHandler: IQueryHandler<GetUserQuery,ErrorOr<UserDto>>
    {
        
        private readonly IBlazeLogDbContext context;
        public GetUserQueryHandler(IBlazeLogDbContext ctx)
        {
            context = ctx;
            
        }
        public async Task<ErrorOr<UserDto>> Handle(GetUserQuery query,CancellationToken cancellationToken)
        {
            var data = context.CoreUsers.AsQueryable();
            var user = await data.FirstOrDefaultAsync(e => e.UserId == query.userId && !e.Disabled);
            if(user!=null)
                return user.Adapt<UserDto>();
            return Error.NotFound("UserNotFound", $"User not found for id {query.userId}");
        }
        
    }


    public record GetUsersQuery(string AskSearch,int Start=1,int Size=0,params SortDescription[] Sorts): IQuery<List<UserDto>>;


    public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, List<UserDto>>
    {
        private readonly IBlazeLogDbContext context;
        
        public GetUsersQueryHandler(IBlazeLogDbContext ctx)
        {
            context = ctx;
              
        }
        public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var query = context.CoreUsers.AsQueryable();
            var size = request.Size == 0 ? CN.Setting.PageSize: request.Size;
            var start = request.Start == 0 ? CN.Setting.PageStart : request.Start;
            //var nv = new Dictionary<string,string>();
            //nv["UserId.at"] = "STO4|SEGU|ESD1";
            //var testquery = QueriesExtensions.GetFilter<CoreUser>(context as DbContext, nv);
            
            if (request.AskSearch != null)
            {
                query= query.Where(x => x.UserName.Contains(request.AskSearch) || x.email.Contains(request.AskSearch));
                
            }
            if (request.Sorts!=null && request.Sorts.Length > 0)
            {
                query= query.BuildOrder(request.Sorts).Skip(start).Take(size);
            }

            return await query.ProjectToType<UserDto>().ToListAsync(cancellationToken);

            
        }
    }

}
