using Cortex.Mediator.Queries;
using GovcoreBse.Shared.Tools;
using GovcoreBse.Store.Setup;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace GovcoreBse.Store.Commands
{

    public record GetRolesQuery(string userid) : IQuery<ErrorOr<UserRoleDto>>;
    public class GetRolesQueryHandler: IQueryHandler<GetRolesQuery,ErrorOr<UserRoleDto>>
    {
        private readonly IBlazeLogDbContext context;
        public GetRolesQueryHandler(IBlazeLogDbContext ctx)
        {
            context = ctx;
        }

        public async Task<ErrorOr<UserRoleDto>> Handle(GetRolesQuery query,CancellationToken cancellationToken)
        {
            var linksystem = context.CoreSettings.FirstOrDefault(e => e.SettingId == DK.SETT_LINKSYSTM)?.SettingValue;
            var uquery = from u in context.CoreUsers.Where(x=> x.UserId ==query.userid)
                         join r in context.CoreRoles.Where(x => x.LinkSystem == linksystem || x.LinkSystem== DK.WILDCARD_SYSTEM) on u.level equals r.level
                         select new { r.RoleName, u.UserId , u.level} ;
            if (uquery.Any() && linksystem!=null) {
                var datum = uquery.FirstOrDefault();
                var roles =await uquery.Select(x => x.RoleName).ToArrayAsync();
                return new UserRoleDto { Level = datum!.level, LinkSystem = linksystem, RoleNames = roles };

            }
            
            return Error.NotFound("UserRoleNotFound", $"User {query.userid} for Role not located");
        }
    }



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
