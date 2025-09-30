using CollabSphere.Application.DTOs.Classes;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Common
{
    public static class FileParser
    {
        public static async Task<List<ImportClassDto>> ParseClassFromExcel(Stream fileStream)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Collab_sphere");
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            var result = new List<ImportClassDto>();
            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row < rowCount; row++)
            {
                var className = worksheet.Cells[row, 1].Text;
                var enrolKey = worksheet.Cells[row, 2].Text;
                var subjectCode = worksheet.Cells[row, 3].Text;
                var lecturerCode = worksheet.Cells[row, 4].Text;
                var studentCodes = worksheet.Cells[row, 5].Text;
                var isActive = worksheet.Cells[row, 6].GetCellValue<bool>();

                if (string.IsNullOrEmpty(className))
                {
                    continue;
                }

                result.Add(new ImportClassDto()
                {
                    ClassName = className,
                    EnrolKey = enrolKey,
                    SubjectCode = subjectCode,
                    LecturerCode = lecturerCode,
                    StudentCodes = studentCodes
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList(),
                    IsActive = isActive,
                });
            }

            return await Task.FromResult(result);
        }
    }
}
