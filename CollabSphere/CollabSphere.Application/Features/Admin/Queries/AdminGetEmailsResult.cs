using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Admin.Queries
{
    public class AdminGetEmailsResult : QueryResult
    {
        public PagedList<EmailDto>? PaginatedEmails { get; set; }
    }
    public class EmailDto
    {
        public string? Id { get; set; }
        public string? ThreadId { get; set; }
        public string? Subject { get; set; }
        public string? From { get; set; }
        public string? Date { get; set; }
        public string? Snippet { get; set; } 
        public bool IsRead { get; set; }
    }
}
