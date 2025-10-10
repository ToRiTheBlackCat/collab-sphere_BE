using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.ClassFiles
{
    public class ClassFileVM
    {
        public int FileId { get; set; }

        public int ClassId { get; set; }

        public string FilePath { get; set; }

        public string Type { get; set; }

        public static explicit operator ClassFileVM(ClassFile classFile)
        {
            return new ClassFileVM()
            {
                FileId = classFile.FileId,
                ClassId = classFile.ClassId,
                FilePath = classFile.FilePath,
                Type = classFile.Type,
            };
        }
    }
}
