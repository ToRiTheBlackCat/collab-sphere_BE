using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.ProjectRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectRepo.Queries.GetReposOfProject
{
    public class GetReposOfProjectResult : QueryResult
    {
        public List<AllReposOfProjectDto>? Installations {  get; set; }
    }
}
