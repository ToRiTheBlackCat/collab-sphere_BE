using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Constants
{
    public class SemesterConstants
    {
        public static int MAX_WEEKS_PER_SEMESTER
        {
            get
            {
                var weekString = Environment.GetEnvironmentVariable("MAX_WEEKS_PER_SEMESTER");
                if (string.IsNullOrWhiteSpace(weekString) || !int.TryParse(weekString, out var weekNumber))
                {
                    return 15;
                }

                return weekNumber;
            }
        }
    }
}
