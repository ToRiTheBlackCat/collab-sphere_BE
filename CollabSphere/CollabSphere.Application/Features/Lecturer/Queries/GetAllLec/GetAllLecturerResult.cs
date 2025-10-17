using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Lecturer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Lecturer.Queries.GetAllLec
{
    public class GetAllLecturerResult : QueryResult
    {
        public PagedList<LecturerResponseDto>? PaginatedLecturers { get; set; }
    }
  }
