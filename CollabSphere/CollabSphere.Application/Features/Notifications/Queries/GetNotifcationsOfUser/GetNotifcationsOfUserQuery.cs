using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Notifications.Queries.GetNotifcationsOfUser
{
    public class GetNotifcationsOfUserQuery : PaginationQuery, IQuery<GetNotifcationsOfUserResult>
    {
        public int UserId = -1;

        public int UserRole = -1;
    }
}
