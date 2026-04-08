using AutoMapper;
using GovcoreBse.Store.Dtos;
using GovcoreBse.Store.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Store.Setup;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CoreUser, UserDto>().ReverseMap();
        CreateMap<UserDto, UserState>()
            .ForMember(dt=> dt.Email,ex=> ex.MapFrom(ex=> ex.Email))
            .ForMember(dt => dt.IsAdmin, ex => ex.MapFrom(ex => ex.IsAdmin))
            .ForMember(dt => dt.Post, ex => ex.MapFrom(ex => ex.Post))
            .ForMember(dt => dt.UserID, ex => ex.MapFrom(ex => ex.UserId))
            .ForMember(dt => dt.UserName, ex => ex.MapFrom(ex => ex.UserName))
            .ForMember(dt => dt.Division, ex => ex.MapFrom(ex => ex.Division))
            .ForMember(dt => dt.Level, ex => ex.MapFrom(ex => ex.Level));

    }
}

