using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
            CreateMap<Item, AuctionDto>();
            CreateMap<CreateActionDto, Auction>().ForMember(d => d.Item, o => o.MapFrom(s => s));
            CreateMap<CreateActionDto, Item>();
            CreateMap<AuctionDto, AuctionCreated>();
            CreateMap<Auction, AuctionUpdated>().IncludeMembers(a => a.Item);
            CreateMap<Item, AuctionUpdated>();
        }
    }
}