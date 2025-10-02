using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.SubjectGradeComponentModels;
using CollabSphere.Application.DTOs.SubjectOutcomeModels;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.SubjectTest
{
    public class SubjectParserTest
    {
        public SubjectParserTest()
        {
            ExcelPackage.License.SetNonCommercialOrganization("Collab_sphere");
        }

        [Fact]
        public async Task Parser_ShouldReturnDtos_ValidFile()
        {
            // Arrange
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Classes");

            // Headers
            ws.Cells[1, 1].Value = "SubjectCode";
            ws.Cells[1, 2].Value = "SubjectName";
            ws.Cells[1, 3].Value = "IsActive";
            ws.Cells[1, 4].Value = "SyllabusName";
            ws.Cells[1, 5].Value = "Description";
            ws.Cells[1, 6].Value = "NoCredit";
            ws.Cells[1, 7].Value = "SubjectOutcomes";
            ws.Cells[1, 8].Value = "SubjectGradeComponents";

            // Data row
            ws.Cells[2, 1].Value = "DS";
            ws.Cells[2, 2].Value = "Sub";
            ws.Cells[2, 3].Value = "  true ";
            ws.Cells[2, 4].Value = "Syllabus from File";
            ws.Cells[2, 5].Value = "A description for syllabus";
            ws.Cells[2, 6].Value = "1";
            ws.Cells[2, 7].Value = "Make a Product\r\nLearn how tos\r\nPresent final";
            ws.Cells[2, 8].Value = "  Product:25\r\nLearning:25   \r\nPresentation:50";

            using var ms = new MemoryStream();
            package.SaveAs(ms);
            ms.Position = 0;

            // Act
            var result = await FileParser.ParseSubjectFromExcel(ms);

            // Assert
            var expectedOutcomes = new List<ImportSubjectOutcomeDto>()
                {
                    new ImportSubjectOutcomeDto()
                    {
                        OutcomeDetail = "Make a Product"
                    },
                    new ImportSubjectOutcomeDto()
                    {
                        OutcomeDetail = "Learn how tos"
                    },
                    new ImportSubjectOutcomeDto()
                    {
                        OutcomeDetail = "Present final"
                    },
                };
            var expectedGradeComps = new List<ImportSubjectGradeComponentDto>()
            {
                new ImportSubjectGradeComponentDto()
                {
                    ComponentName = "Product",
                    ReferencePercentage = 25,
                },
                new ImportSubjectGradeComponentDto()
                {
                    ComponentName = "Learning",
                    ReferencePercentage = 25,
                },
                new ImportSubjectGradeComponentDto()
                {
                    ComponentName = "Presentation",
                    ReferencePercentage = 50,
                },
            };
            Assert.Single(result);
            var dto = result[0];
            Assert.Equal("DS", dto.SubjectCode);
            Assert.Equal("Sub", dto.SubjectName);
            Assert.True(dto.IsActive);
            Assert.Equal("Syllabus from File", dto.SubjectSyllabus.SyllabusName);
            Assert.Equal("A description for syllabus", dto.SubjectSyllabus.Description);
            Assert.Equal(1, dto.SubjectSyllabus.NoCredit);
            Assert.Equivalent(expectedOutcomes, dto.SubjectSyllabus.SubjectOutcomes);
            Assert.Equivalent(expectedGradeComps, dto.SubjectSyllabus.SubjectGradeComponents);
        }
    }
}
