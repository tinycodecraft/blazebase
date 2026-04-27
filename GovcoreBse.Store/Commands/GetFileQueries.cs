using AgileObjects.AgileMapper.Extensions.Internal;
using Cortex.Mediator.Queries;
using GovcoreBse.Store.Models;
using GovcoreBse.Store.Setup;

using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Store.Commands
{
    public record GetFilesAsUrlQuery(int linkid, params string[] types) : IQuery<ErrorOr<KeyValuePair<UrlModel, string[]>>>;

    public class GetFilesAsUrlQueryHandler : IQueryHandler<GetFilesAsUrlQuery, ErrorOr<KeyValuePair<UrlModel, string[]>>>
    {
        private readonly IBlazeLogDbContext context;
        private readonly PathSetting setting;
        
        public GetFilesAsUrlQueryHandler(IBlazeLogDbContext ctx, IOptions<PathSetting> opt)
        {
            context = ctx;
            setting = opt.Value;

        }
        public async Task<ErrorOr<KeyValuePair<UrlModel, string[]>>> Handle(GetFilesAsUrlQuery query, CancellationToken cancellationToken)
        {
            var data = context.CoreFileDocs.AsQueryable();
            var files = await data.Where(x => x.LinkId == query.linkid).ToListAsync();
            var doctypes = context.CoreSettings.AsQueryable();
            var types = query.types;
            var docmaxsett = await doctypes.Where(e => e.SettingId == CN.DataKey.SETT_DOCMAXCNT).FirstOrDefaultAsync(cancellationToken);

            if(types==null || types.Length ==0 || docmaxsett==null )
            {
                return Error.NotFound("DocTypeNotFound", $"Doc Type is not found or specified for {query.linkid}");
            }

            var docmaxcnt = docmaxsett != null && int.TryParse(docmaxsett.SettingValue, out var maxcnt) ? maxcnt : 5;

            Dictionary<string, string> typedict = new();
            foreach(var pair in doctypes.Where(e => types.Any(y => e.SettingId.StartsWith(CN.DataKey.SETT_DOCTYPE + "." + y))).ToList()
                .OrderBy(e => int.Parse(string.Join("", e.SettingId.Replace(CN.DataKey.SETT_DOCTYPE + ".", "").Substring(1)))).ToArray().Select((e,i) => new { key = e.SettingValue, value =e.SettingId.Replace(CN.DataKey.SETT_DOCTYPE+".", "").Substring(0, 1) }))
            {
                typedict[pair.value] = pair.key!;
            }

            var urlmodelpair =  HelperQ.GetUrlModel<CoreFileDoc>(setting, typedict, files, docmaxcnt, types);

           

            return  urlmodelpair;
        }
    }


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
