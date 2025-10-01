using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.SubjectModels;
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

            for (int row = 2; row <= rowCount; row++)
            {
                var className = worksheet.Cells[row, 1].Text.Trim();
                var enrolKey = worksheet.Cells[row, 2].Text.Trim();
                var subjectCode = worksheet.Cells[row, 3].Text.Trim();
                var lecturerCode = worksheet.Cells[row, 4].Text.Trim();
                var studentCodes = worksheet.Cells[row, 5].Text.Trim();
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

        /// <summary>
        /// Parse subjects from import Excel File
        /// </summary>
        public static async Task<List<ImportSubjectDto>> ParseSubjectFromExcel(Stream fileStream)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Collab_sphere");
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            var result = new List<ImportSubjectDto>();
            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var subjectCode = worksheet.Cells[row, 1].Text.Trim();
                var subjectName = worksheet.Cells[row, 2].Text.Trim();
                var isActive = bool.Parse(worksheet.Cells[row, 3].Text);

                if (string.IsNullOrEmpty(subjectName))
                {
                    continue;
                }

                result.Add(new ImportSubjectDto()
                {
                    SubjectName = subjectName,
                    SubjectCode = subjectCode,
                    IsActive = isActive,
                });
            }

            return await Task.FromResult(result);
        }
    }
}
