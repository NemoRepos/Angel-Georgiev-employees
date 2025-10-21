using Microsoft.AspNetCore.Mvc;
using SirmaTask.Models;
using System.IO;

namespace SirmaTask.Services
{
    public interface IValidatorService
    {
        public bool ValidateCSV(UploadedFileViewModel fileModel, Controller ctrl);
    }
    public class ValidatorService : IValidatorService
    {
        public bool ValidateCSV(UploadedFileViewModel fileModel,Controller ctrl)
        {
            if (!ctrl.ModelState.IsValid)
            {
                return false;
            }

            if (fileModel.CSVTable == null || fileModel.CSVTable.Length == 0)
            {
                ctrl.ModelState.AddModelError("CSVTable", "Invalid CSV file.");
                return false;
            }
            else if (!fileModel.CSVTable.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ctrl.ModelState.AddModelError("CSVTable", "Only CSV files are allowed.");
            }
            return true;
        }
    }
}
