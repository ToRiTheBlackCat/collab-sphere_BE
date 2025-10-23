﻿using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Domain.Intefaces
{
    public interface ISubjectGradeComponentRepository : IGenericRepository<SubjectGradeComponent>
    {
        Task<List<SubjectGradeComponent>?> GetComponentsBySubjectId(int subjectId);
    }
}
