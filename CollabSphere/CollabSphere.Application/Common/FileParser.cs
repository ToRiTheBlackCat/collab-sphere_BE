using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.DTOs.Student;
using CollabSphere.Application.DTOs.SubjectModels;
using CollabSphere.Application.DTOs.SubjectOutcomeModels;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Common
{
    public static class FileParser
    {
        private static bool IsCorrectPropertiesOrder<T>(ExcelWorksheet worksheet, out List<string>? correctNames)
        {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propertyNames = properties.Select(prop => prop.Name).ToList();
            var propCount = propertyNames.Count;

            var checkNames = new List<string>();
            for (int column = 1; column <= propCount; column++)
            {
                var checkField = worksheet.Cells[1, column].Text.Trim();
                checkNames.Add(checkField);
            }

            if (propertyNames.SequenceEqual(checkNames))
            {
                correctNames = null;
                return true;
            }

            correctNames = propertyNames;
            return false;
        }

        public static async Task<List<ImportClassDto>> ParseClassFromExcel(Stream fileStream)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Collab_sphere");
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            if (!IsCorrectPropertiesOrder<ImportClassDto>(worksheet, out var correctNames))
            {
                throw new Exception($"Incorrect input order, must be: {string.Join(", ", correctNames!)}");
            }

            var result = new List<ImportClassDto>();
            var rowCount = worksheet.Dimension.Rows;

            int col = 1;
            for (int row = 2; row <= rowCount; row++)
            {
                var className = worksheet.Cells[row, col++].Text.Trim();
                var enrolKey = worksheet.Cells[row, col++].Text.Trim();
                var subjectCode = worksheet.Cells[row, col++].Text.Trim();
                var semesterCode = worksheet.Cells[row, col++].Text.Trim();
                var lecturerCode = worksheet.Cells[row, col++].Text.Trim();
                var studentCodes = worksheet.Cells[row, col++].Text.Trim();
                var isActive = worksheet.Cells[row, col++].GetCellValue<bool>();
                col = 1;

                if (string.IsNullOrEmpty(className))
                {
                    continue;
                }

                result.Add(new ImportClassDto()
                {
                    ClassName = className,
                    EnrolKey = enrolKey,
                    SubjectCode = subjectCode,
                    SemesterCode = semesterCode,
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
                int column = 1;
                var subjectCode = worksheet.Cells[row, column++].Text.Trim();
                var subjectName = worksheet.Cells[row, column++].Text.Trim();
                var isActive = bool.Parse(worksheet.Cells[row, column++].Text);
                var syllabusName = worksheet.Cells[row, column++].Text.Trim();
                var description = worksheet.Cells[row, column++].Text.Trim();
                var noCredit = int.Parse(worksheet.Cells[row, column++].Text);
                var outcomesString = worksheet.Cells[row, column++].Text.Trim();
                var gradeCompsString = worksheet.Cells[row, column++].Text.Trim();

                if (string.IsNullOrEmpty(subjectCode))
                {
                    continue;
                }

                result.Add(new ImportSubjectDto()
                {
                    SubjectName = subjectName,
                    SubjectCode = subjectCode,
                    IsActive = isActive,
                    SubjectSyllabus = new DTOs.SubjectSyllabusModel.ImportSubjectSyllabusDto()
                    {
                        SyllabusName = syllabusName,
                        Description = description,
                        NoCredit = noCredit,
                        IsActive = isActive,
                        SubjectCode = subjectCode,
                        SubjectGradeComponents = gradeCompsString
                            .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                            .Select(x =>
                            {
                                var split = x.Trim().Split(":", StringSplitOptions.RemoveEmptyEntries);
                                return new DTOs.SubjectGradeComponentModels.ImportSubjectGradeComponentDto()
                                {
                                    ComponentName = split[0],
                                    ReferencePercentage = decimal.Parse(split[1]),
                                };
                            })
                            .ToList(),
                        SubjectOutcomes = outcomesString
                            .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => new ImportSubjectOutcomeDto()
                            {
                                OutcomeDetail = x.Trim()
                            })
                            .ToList(),
                    }
                });
            }

            return await Task.FromResult(result);
        }

        public static async Task<List<ImportLecturerDto>> ParseListLecturerFromExcel(Stream fileStream)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Collab_sphere");
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            var result = new List<ImportLecturerDto>();
            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var email = worksheet.Cells[row, 1].Text.Trim();
                var password = worksheet.Cells[row, 2].Text.Trim();
                var fullName = worksheet.Cells[row, 3].Text.Trim();
                var address = worksheet.Cells[row, 4].Text.Trim();
                var phoneCell = worksheet.Cells[row, 5].Value;
                string phoneNumber = phoneCell switch
                {
                    double d => d.ToString("0"),
                    decimal m => m.ToString("0"),
                    _ => phoneCell?.ToString().Trim()
                };
                var yob = worksheet.Cells[row, 6].Text.Trim();
                var school = worksheet.Cells[row, 7].Text.Trim();
                var lecturerCode = worksheet.Cells[row, 8].Text.Trim();
                var major = worksheet.Cells[row, 9].Text.Trim();

                if (string.IsNullOrEmpty(email)
                    || string.IsNullOrEmpty(password)
                    || string.IsNullOrEmpty(fullName)
                    || string.IsNullOrEmpty(lecturerCode))
                {
                    continue;
                }

                result.Add(new ImportLecturerDto()
                {
                    Email = email,
                    Password = password,
                    FullName = fullName,
                    Address = address,
                    PhoneNumber = phoneNumber,
                    Yob = yob,
                    School = school,
                    LecturerCode = lecturerCode,
                    Major = major,
                });
            }

            return await Task.FromResult(result);
        }

        public static async Task<List<ImportStudentDto>> ParseListStudentFromExcel(Stream fileStream)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Collab_sphere");
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            var result = new List<ImportStudentDto>();
            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var email = worksheet.Cells[row, 1].Text.Trim();
                var password = worksheet.Cells[row, 2].Text.Trim();
                var fullName = worksheet.Cells[row, 3].Text.Trim();
                var address = worksheet.Cells[row, 4].Text.Trim();
                var phoneCell = worksheet.Cells[row, 5].Value;
                string phoneNumber = phoneCell switch
                {
                    double d => d.ToString("0"),
                    decimal m => m.ToString("0"),
                    _ => phoneCell?.ToString().Trim()
                };
                var yob = worksheet.Cells[row, 6].Text.Trim();
                var school = worksheet.Cells[row, 7].Text.Trim();
                var studentCode = worksheet.Cells[row, 8].Text.Trim();
                var major = worksheet.Cells[row, 9].Text.Trim();

                if (string.IsNullOrEmpty(email)
                    || string.IsNullOrEmpty(password)
                    || string.IsNullOrEmpty(fullName)
                    || string.IsNullOrEmpty(studentCode))
                {
                    continue;
                }

                result.Add(new ImportStudentDto()
                {
                    Email = email,
                    Password = password,
                    FullName = fullName,
                    Address = address,
                    PhoneNumber = phoneNumber,
                    Yob = yob,
                    School = school,
                    StudentCode = studentCode,
                    Major = major,
                });
            }

            return await Task.FromResult(result);
        }
    }
}
