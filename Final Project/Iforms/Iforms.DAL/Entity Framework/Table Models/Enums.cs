using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    public class Enums
    {
        public enum UserRole
        {
            Admin,
            User,
        }

        public enum UserStatus
        {
            Active,
            Inactive,
            Blocked
        }

        public enum Language
        {
            English,
            Polish
        }

        public enum Theme
        {
            Light,
            Dark
        }
        public enum QuestionType
        {
            Text,
            Number,
            Date,
            SingleChoice,
            MultipleChoice,
            FileUpload
        }

    }
}
