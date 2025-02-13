﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iPhoneBE.Data.Models.CategoryModel
{
    public class UpdateCategoryModel
    {
        [MaxLength(255, ErrorMessage = "Category name cannot exceed 255 characters.")]
        public string? Name { get; set; }

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
    }
}
