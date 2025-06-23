using Iforms.DAL.Entity_Framework;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.DAL.Interfaces;
using Iforms.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL
{
    public class DataAccess
    {
        private readonly ApplicationDBContext db;

        public DataAccess(ApplicationDBContext dbContext)
        {
            db = dbContext;
        }
        public IUser UserData()
        {
            return new UserRepository(db);
        }
        public IAnswer AnswerData()
        {
            return new AnswerRepository(db);
        }

        public IAuditLog AuditLogData()
        {
            return new AuditLogRepository(db);
        }

        public IComment CommentData()
        {
            return new CommentRepository(db);
        }

        public IForm FormData()
        {
            return new FormRepository(db);
        }

        public ILike LikeData()
        { 
            return new LikeRepository(db); 
        }

        public IQuestion QuestionData()
        {
            return new QuestionRepository(db);
        }

        public ITag TagData()
        {
            return new TagRepository(db);
        }

        public ITemplate TemplateData()
        {
            return new TemplateRepository(db);
        }

        public ITemplateAccess TemplateAccessData()
        {
            return new TemplateAccessRepository(db);
        }

        public ITemplateTag TemplateTagData()
        {
            return new TemplateTagRepository(db);
        }

        public ITopic TopicData()
        {
            return new TopicRepository(db);
        }

    }
}
