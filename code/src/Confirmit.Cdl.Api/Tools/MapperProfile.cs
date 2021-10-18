using AutoMapper;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.ViewModel;

namespace Confirmit.Cdl.Api.Tools
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Document, DocumentDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (DocumentType) src.Type));
            CreateMap<Document, DocumentShortDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (DocumentType) src.Type));
            CreateMap<DocumentDto, Document>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (byte) src.Type));
            CreateMap<DocumentToCreateDto, Document>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (byte) src.Type));
            CreateMap<AliasToCreateDto, DocumentAlias>();
            CreateMap<DocumentAlias, AliasDto>();
            CreateMap<Revision, RevisionDto>();
            CreateMap<RevisionToCreateDto, Revision>();
        }
    }
}
