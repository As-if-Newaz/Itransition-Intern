using AutoMapper;
using Iforms.BLL.DTOs;
using Iforms.DAL.Entity_Framework.Table_Models;

namespace Iforms.BLL.Services
{
    public class TemplateProfile : Profile
    {
        public TemplateProfile()
        {
            CreateMap<Template, TemplateDTO>()
                .ForMember(dest => dest.TemplateTags, opt => opt.MapFrom(src => src.TemplateTags.Select(tt => tt.Tag)))
                .ForMember(dest => dest.TemplateAccesses, opt => opt.MapFrom(src => src.TemplateAccesses.Select(ta => ta.User)))
                .ForMember(dest => dest.Topic, opt => opt.MapFrom(src => src.Topic))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions))
                .ForMember(dest => dest.Forms, opt => opt.MapFrom(src => src.Forms))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(dest => dest.Likes, opt => opt.MapFrom(src => src.Likes))
                .ReverseMap()
                .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.Topic != null ? src.Topic.Id : src.TopicId))
                .ForMember(dest => dest.Topic, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.Ignore())
                .ForMember(dest => dest.Forms, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.Likes, opt => opt.Ignore())
                .ForMember(dest => dest.TemplateTags, opt => opt.Ignore())
                .ForMember(dest => dest.TemplateAccesses, opt => opt.Ignore());

            CreateMap<Tag, TagDTO>().ReverseMap();
            CreateMap<TemplateTagDTO, TemplateTag>();
            CreateMap<TemplateAccessDTO, TemplateAccess>();
            CreateMap<Topic, TopicDTO>().ReverseMap();
            CreateMap<User, UserDTO>().ReverseMap();

            CreateMap<Question, QuestionDTO>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options != null ? src.Options.ToList() : new List<string>()))
                .ReverseMap()
                .ForMember(dest => dest.Template, opt => opt.Ignore())
                .ForMember(dest => dest.Answers, opt => opt.Ignore())
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options ?? new List<string>()));

            CreateMap<Form, FormDTO>()
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers))
                .ReverseMap()
                .ForMember(dest => dest.Template, opt => opt.Ignore())
                .ForMember(dest => dest.FilledBy, opt => opt.Ignore())
                .ForMember(dest => dest.Answers, opt => opt.Ignore());

            CreateMap<Comment, CommentDTO>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : null))
                .ReverseMap()
                .ForMember(dest => dest.Template, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<Like, LikeDTO>().ReverseMap()
                .ForMember(dest => dest.Template, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<Answer, AnswerDTO>().ReverseMap()
                .ForMember(dest => dest.Form, opt => opt.Ignore())
                .ForMember(dest => dest.Question, opt => opt.Ignore());
        }
    }
} 