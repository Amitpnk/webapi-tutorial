using AutoMapper;
using Customer.API.Data.Entities;
using Customer.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customer.API.Data
{
    public class CampMappingProfile : Profile
    {
        public CampMappingProfile()
        {
            CreateMap<Camp, CampModel>()
                .ForMember(c => c.Venue, opt => opt.MapFrom(m => m.Location.VenueName))
                .ReverseMap();
            CreateMap<Speaker, SpeakerModel>();
            CreateMap<Talk, TalkModel>();
        }
    }
}
