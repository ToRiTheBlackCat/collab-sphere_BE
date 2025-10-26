﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Student
{
    public class GetAllStudentRequestDto
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public int Yob { get; set; } = 0;
        public string? StudentCode { get; set; }
        public string? Major { get; set; }
        public bool IsDesc { get; set; } = false;
    }
}
