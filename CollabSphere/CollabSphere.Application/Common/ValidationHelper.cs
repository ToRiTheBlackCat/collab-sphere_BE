using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Common
{
    public class ValidationHelper
    {
        public static List<OperationError> ValidateList<T>(IEnumerable<T> items, string listName = "items")
        {
            if (!items.Any())
            {
                return new List<OperationError>();
            }

            var errors = new List<OperationError>();

            for (int i = 0; i < items.Count(); i++)
            {
                var item = items.ElementAt(i);
                if (item == null) continue;

                var itemErrors = ValidateObjectRecursive(item, $"{listName}[{i}]");
                if (!itemErrors.Any())
                {
                    errors.AddRange(itemErrors);
                }
            }

            return errors;
        }

        public static List<OperationError> ValidateObjectRecursive(object obj, string prefix = "")
        {
            var errors = new List<OperationError>();
            if (obj == null) return errors;

            var type = obj.GetType();

            // Handle collections separately
            if (obj is IEnumerable enumerable && !(obj is string))
            {
                int index = 0;
                foreach (var item in enumerable)
                {
                    var nestedPrefix = $"{prefix}[{index}]";
                    errors.AddRange(ValidateObjectRecursive(item, nestedPrefix));
                    index++;
                }
                return errors;
            }

            // Validate current object's properties via data annotations
            var context = new ValidationContext(obj);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(obj, context, results, validateAllProperties: true);

            foreach (var result in results)
            {
                foreach (var member in result.MemberNames)
                {
                    errors.Add(new OperationError
                    {
                        Field = string.IsNullOrEmpty(prefix) ? member : $"{prefix}.{member}",
                        Message = result.ErrorMessage ?? "Invalid value."
                    });
                }
            }

            // Recursively validate properties that are complex types
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType == typeof(string)) continue; // skip strings
                if (property.PropertyType.IsValueType) continue;       // skip primitives

                var value = property.GetValue(obj);
                if (value == null) continue;

                var nestedPrefix = string.IsNullOrEmpty(prefix)
                    ? property.Name
                    : $"{prefix}.{property.Name}";

                errors.AddRange(ValidateObjectRecursive(value, nestedPrefix));
            }

            return errors;
        }
    }
}

