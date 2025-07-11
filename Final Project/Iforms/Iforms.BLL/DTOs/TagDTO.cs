﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class TagDTO
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        public int UsageCount { get; set; }
    }
}
