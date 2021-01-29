﻿using AutoMapper;
using CoreCodeCamp.DTOs;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            this.CreateMap<Camp, CampDto>()
                .ReverseMap();

            this.CreateMap<Talk, TalkDto>()
                .ReverseMap()
                .ForMember(t => t.Camp, opt => opt.Ignore())
                .ForMember(t => t.Speaker, opt => opt.Ignore());

            this.CreateMap<Speaker, SpeakerDto>()
                .ReverseMap();
        }
    }
}
