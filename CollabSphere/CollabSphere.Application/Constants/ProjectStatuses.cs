﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Constants
{
    //public class ProjectStatuses
    //{
    //    public const int PENDING = 0;
    //    public const int APPROVED = 1;
    //    public const int DENIED = 2;
    //}

    public enum ProjectStatuses: int
    {
        REMOVED = -1,
        PENDING = 0,
        APPROVED = 1,
        DENIED = 2,
    }
}
