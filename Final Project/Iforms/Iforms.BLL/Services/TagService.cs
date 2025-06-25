using AutoMapper;
using Iforms.BLL.DTOs;
using Iforms.DAL;
using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.Services
{
    public class TagService
    {
        private readonly DataAccess DA;

        public TagService(DataAccess DA)
        {
            this.DA = DA;
        }

        static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Tag, TagDTO>();
                cfg.CreateMap<TagDTO, Tag>();
            });
            return new Mapper(config);
        }

        public List<TagDTO> GetAll()
        {
            var tags = DA.TagData().GetTagsWithUsageCount();
            return GetMapper().Map<List<TagDTO>>(tags);
        }


        public List<TagDTO> SearchTags(string searchTerm)
        {
            var tags = DA.TagData().SearchTags(searchTerm);
            return GetMapper().Map<List<TagDTO>>(tags);
        }
    }
}
