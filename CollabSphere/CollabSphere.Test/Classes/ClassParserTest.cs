using CollabSphere.Application;
using CollabSphere.Application.Common;
using CollabSphere.Application.Features.Classes.Commands.ImportClass;
using CollabSphere.Domain.Intefaces;
using Microsoft.AspNetCore.Routing;
using Moq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Classes
{
    public class ClassParserTest
    {
        public ClassParserTest()
        {
            ExcelPackage.License.SetNonCommercialOrganization("Collab_sphere");
        }

        [Fact]
        public async Task ParseClassFromExcel_ValidSheet_ReturnsDtos()
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Classes");

            // Headers
            ws.Cells[1, 1].Value = "ClassName";
            ws.Cells[1, 2].Value = "EnrolKey";
            ws.Cells[1, 3].Value = "SubjectCode";
            ws.Cells[1, 4].Value = "SemesterCode";
            ws.Cells[1, 5].Value = "LecturerCode";
            ws.Cells[1, 6].Value = "StudentCodes";
            ws.Cells[1, 7].Value = "IsActive";

            // Data row
            ws.Cells[2, 1].Value = "CS101A";
            ws.Cells[2, 2].Value = "KEY123";
            ws.Cells[2, 3].Value = "CS101";
            ws.Cells[2, 4].Value = "FA25";
            ws.Cells[2, 5].Value = "LECT001";
            ws.Cells[2, 6].Value = "STU001,STU002,STU003";
            ws.Cells[2, 7].Value = true;

            using var ms = new MemoryStream();
            package.SaveAs(ms);
            ms.Position = 0;

            var result = await FileParser.ParseClassFromExcel(ms);


            Assert.Single(result);
            var dto = result[0];
            Assert.Equal("CS101A", dto.ClassName);
            Assert.Equal(3, dto.StudentCodes.Count);
            Assert.True(dto.IsActive);
        }

        [Fact]
        public async Task ParseClassFromExcel_WhitespaceStudentCodes_ParsesTrimmed()
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Classes");

            // Headers
            ws.Cells[1, 1].Value = "ClassName";
            ws.Cells[1, 2].Value = "EnrolKey";
            ws.Cells[1, 3].Value = "SubjectCode";
            ws.Cells[1, 4].Value = "SemesterCode";
            ws.Cells[1, 5].Value = "LecturerCode";
            ws.Cells[1, 6].Value = "StudentCodes";
            ws.Cells[1, 7].Value = "IsActive";

            // Row with student codes having whitespace
            ws.Cells[2, 1].Value = "CS102A";
            ws.Cells[2, 2].Value = "KEY456";
            ws.Cells[2, 3].Value = "CS102";
            ws.Cells[2, 4].Value = "FA25";
            ws.Cells[2, 5].Value = "LECT002";
            ws.Cells[2, 6].Value = " STU001 ,  STU002 ,STU003 "; // extra spaces
            ws.Cells[2, 7].Value = true;

            using var ms = new MemoryStream();
            package.SaveAs(ms);
            ms.Position = 0;

            var result = await FileParser.ParseClassFromExcel(ms);

            Assert.Single(result);
            var dto = result[0];
            Assert.Equal(3, dto.StudentCodes.Count);
            Assert.Equal("STU001", dto.StudentCodes[0]);
            Assert.Equal("STU002", dto.StudentCodes[1]);
            Assert.Equal("STU003", dto.StudentCodes[2]);
        }

        [Fact]
        public async Task ParseClassFromExcel_EmptyStudentCodes_ReturnsEmptyList()
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Classes");

            // Headers
            ws.Cells[1, 1].Value = "ClassName";
            ws.Cells[1, 2].Value = "EnrolKey";
            ws.Cells[1, 3].Value = "SubjectCode";
            ws.Cells[1, 4].Value = "SemesterCode";
            ws.Cells[1, 5].Value = "LecturerCode";
            ws.Cells[1, 6].Value = "StudentCodes";
            ws.Cells[1, 7].Value = "IsActive";

            // Row with empty student codes
            ws.Cells[2, 1].Value = "CS103A";
            ws.Cells[2, 2].Value = "KEY789";
            ws.Cells[2, 3].Value = "CS103";
            ws.Cells[2, 4].Value = "FA25";
            ws.Cells[2, 5].Value = "LECT003";
            ws.Cells[2, 6].Value = ""; // empty
            ws.Cells[2, 7].Value = true;

            using var ms = new MemoryStream();
            package.SaveAs(ms);
            ms.Position = 0;

            var result = await FileParser.ParseClassFromExcel(ms);

            Assert.Single(result);
            var dto = result[0];
            Assert.NotNull(dto.StudentCodes);
            Assert.Empty(dto.StudentCodes); // should be an empty list, not null
        }
    }
}
