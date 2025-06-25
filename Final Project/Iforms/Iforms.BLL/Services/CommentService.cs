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
    public class CommentService
    {
        private readonly DataAccess DA;
        public CommentService(DataAccess dataAccess)
        {
            DA = dataAccess;
        }
        static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Comment, CommentDTO>();
                cfg.CreateMap<CommentDTO, Comment>();
                cfg.CreateMap<User, UserDTO>();

            });
            return new Mapper(config);
        }
        public bool Create(CommentDTO createCommentDto)
        {
            var data = GetMapper().Map<Comment>(createCommentDto);

            return DA.CommentData().Create(data);

        }
        public bool Delete(int id, int currentUserId)
        {
            if (!DA.CommentData().CanUserDeleteComment(id, currentUserId))
                return false;

            var comment = DA.CommentData().Get(id);
            if (comment == null) return false;

            return DA.CommentData().Delete(comment);
        }

    }
}
