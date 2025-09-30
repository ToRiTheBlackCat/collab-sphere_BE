using CollabSphere.Application.DTOs.Lecturer;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Common
{
    public interface IExcelFormatValidator
    {
        bool ValidateTableHeaderFormat(Stream fileStream, string type);
    }

    public class ValidateTableFormat : IExcelFormatValidator
    {
        private readonly List<string> _expectedImportLecturerHeaders = new() { "Email", "Password", "FullName", "Address", "PhoneNumber", "Yob", "School", "LecturerCode", "Major" };

        public bool ValidateTableHeaderFormat(Stream fileStream, string type)
        {
            bool isValid = false;

            ExcelPackage.License.SetNonCommercialOrganization("Collab_sphere");
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            int colCount = worksheet.Dimension.End.Column;

            var headers = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                string? header = worksheet.Cells[1, col].Text?.Trim();
                if (!string.IsNullOrEmpty(header))
                    headers.Add(header);
            }

            if (type.Equals("LECTURER"))
            {
                isValid = !_expectedImportLecturerHeaders.Except(headers, StringComparer.OrdinalIgnoreCase).Any();

            }

            return isValid;
        }
    }
}
