using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.ClassFiles;
using CollabSphere.Application.DTOs.TeamFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ClassFiles.Queries.GetFilesOfClass
{
    public class GetFilesOfClassResult : QueryResult
    {
        /// <summary>
        /// Dictionary<filePathPrefix, classFileList>
        /// </summary>
        public Dictionary<string, IGrouping<string, ClassFileVM>> Grouping { get; set; } = new();
    }
}
