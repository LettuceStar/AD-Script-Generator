using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Services.Excel
{
    public static class DataAnnotationRowValidator
    {
        public static List<ExcelRowError> ValidateRow<T>(T model, int rowNumber)
        {
            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(model!);

            Validator.TryValidateObject(
                model!,
                ctx,
                results,
                validateAllProperties: true);

            var errors = new List<ExcelRowError>();

            foreach (var vr in results)
            {
                var members = vr.MemberNames?.ToList() ?? new List<string>();

                // If no member name is supplied, show as "General"
                if (members.Count == 0)
                {
                    errors.Add(new ExcelRowError(rowNumber, "General", vr.ErrorMessage ?? "Validation error"));
                    continue;
                }

                foreach (var m in members)
                {
                    errors.Add(new ExcelRowError(rowNumber, m, vr.ErrorMessage ?? "Validation error"));
                }
            }

            return errors;
        }
    }

}
