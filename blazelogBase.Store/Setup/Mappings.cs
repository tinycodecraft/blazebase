using AutoMapper;
using blazelogBase.Store.Dtos;
using blazelogBase.Store.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blazelogBase.Store.Setup;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserDto, CoreUser>().ReverseMap();
    }
}

