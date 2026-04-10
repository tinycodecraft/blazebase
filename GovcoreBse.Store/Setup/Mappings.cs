
using AgileObjects.AgileMapper.Configuration;
using GovcoreBse.Store.Dtos;
using GovcoreBse.Store.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GovcoreBse.Common.Interfaces;

namespace GovcoreBse.Store.Setup;

public class NullMappingConfiguration: MapperConfiguration
{
    protected override void Configure()
    {
        WhenMapping.NullStringToEmpty<UserState>(x => x.Division);
    }
}

public class MappingRegister: IRegister

{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CoreUser, UserDto>().TwoWays();
        config.NewConfig<UserDto, UserState>()                
            .Map(dt => dt.Email, ex => ex.Email)
            .Map(dt => dt.IsAdmin, ex => ex.IsAdmin)
            .Map(dt => dt.Post, ex => ex.Post)
            .Map(dt => dt.UserID, ex => ex.UserId)
            .Map(dt => dt.UserName, ex => ex.UserName)
            .Map(dt => dt.Division, ex => ex.Division)            
            .Map(dt => dt.Level, ex => ex.Level)            
            ;

        config.NewConfig<IDocItem, UrlItem>()
            .Map(dt => dt.Url, ex => ex.RelativePath)
            .Map(dt => dt.Type, ex => ex.DocType)
            .Map(dt => dt.Name, ex => ex.RelativePath)
            .Map(dt => dt.Thumb, ex => ex.Id);

        //IgnoreNonMapped, which will ignore the properties that are not mapped explicitly
        //IgnoreIf, skip if the condition is met

    }
}