﻿using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface ILecturerRepository : IGenericRepository<Lecturer>
    {
        Task InsertLecturer(Lecturer lecturer);
        void UpdateLecturer(Lecturer lecturer);
    }
}
