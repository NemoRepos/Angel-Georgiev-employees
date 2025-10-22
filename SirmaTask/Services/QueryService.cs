using Microsoft.AspNetCore.Mvc;
using SirmaTask.Models;
using System.Diagnostics.Metrics;
using System.Text;

namespace SirmaTask.Services
{
    public interface IQueryService
    {
        public IActionResult GetPairs(UploadedFileViewModel csvFile,Controller ctrl);
    }
    public class QueryService:IQueryService
    {
        private IDateParseService _dateParseService;
        private IValidatorService _validatorService;
        public QueryService(IDateParseService dateParseService, IValidatorService validatorService)
        {
            _dateParseService = dateParseService;
            _validatorService = validatorService;
        }

        public IActionResult GetPairs(UploadedFileViewModel csvFile,Controller ctrl)
        {
            if (!_validatorService.ValidateCSV(csvFile,ctrl))
            {
                return ctrl.View("Index", csvFile);
            }
            List<EmployeeProjectViewModel> projects = Parse(csvFile.CSVTable);
            List<PairsViewModel> pairs = GeneratePairsQuery(projects);
            return ctrl.Json(pairs);
        }

        private List<EmployeeProjectViewModel> Parse(IFormFile csv)
        {
            List<EmployeeProjectViewModel> projects = new List<EmployeeProjectViewModel>();
            StringBuilder lineErrors = new StringBuilder();
            int counter = 0;
            using (var reader = new StreamReader(csv.OpenReadStream()))
            {
                string line = reader.ReadLine(); // Skip header line
                while ((line = reader.ReadLine()) != null)
                {
                    counter++;
                    if(TryExtractProject(line,counter,out EmployeeProjectViewModel project,lineErrors))
                        projects.Add(project);
                }
            }
            if(lineErrors.Length > 0)
            {
                //show to user maybe JavaScript
            }
            return projects;
        }
        private bool TryExtractProject(string line,int lineNumber,out EmployeeProjectViewModel project,StringBuilder errors)
        {
            project = null;
            string[] values = line.Split(',');

            //check if line is valid
            if (values.Length > 4)
            {
                errors.AppendLine($"Line {lineNumber}: Expected 4 values, but found {values.Length}. Skipping line");
                return false;
            }
            //check id && project id
            int employeeID;
            if (!int.TryParse(values[0].Trim(), out employeeID))
            {
                errors.AppendLine($"Line {lineNumber}: Employee ID ('{values[0]}') is not a valid integer. Skipping line.");
                return false;
            }
            int projectID;
            if (!int.TryParse(values[1].Trim(), out projectID))
            {
                errors.AppendLine($"Line {lineNumber}: Project ID ('{values[1]}') is not a valid integer. Skipping line.");
                return false;
            }
            //check datefrom
            DateTime? dateFrom;
            if (!_dateParseService.ParseDate(values[2].Trim(), out dateFrom))
            {
                errors.AppendLine($"Line {lineNumber}: Date From ('{values[2]}') could not be parsed. Skipping line.");
                return false;
            }
            //check dateto
            DateTime? dateTo;
            if (!_dateParseService.ParseDate(values[3].Trim(), out dateTo))
                dateTo = DateTime.Today;

            project = new EmployeeProjectViewModel()
            {
                EmployeeID = employeeID,
                ProjectID = projectID,
                DateFrom = dateFrom.Value,
                DateTo = dateTo.Value
            };
            return true;
        }
        private List<PairsViewModel> GeneratePairsQuery(List<EmployeeProjectViewModel> projects)
        {
            List<PairsViewModel> pairs = new List<PairsViewModel>();
            //Group by project and days worked
            var pairProjectOverlaps = projects
            .GroupBy(r => r.ProjectID)
            .SelectMany(g =>
                from e1 in g
                from e2 in g
                where e1.EmployeeID < e2.EmployeeID
                let overlapStart = e1.DateFrom > e2.DateFrom ? e1.DateFrom : e2.DateFrom
                let overlapEnd = e1.DateTo < e2.DateTo ? e1.DateTo : e2.DateTo
                let daysWorked = (overlapEnd - overlapStart).TotalDays
                where daysWorked > 0
                select new PairsViewModel
                {
                    Employee1ID = e1.EmployeeID,
                    Employee2ID = e2.EmployeeID,
                    ProjectID = g.Key,
                    DaysWorked = (int)daysWorked
                });

            // Pair with the most days worked together
            var topPair = pairProjectOverlaps
            .OrderByDescending(x => x.DaysWorked)
            .Select(x => new { x.Employee1ID, x.Employee2ID })
            .FirstOrDefault();

            if (topPair != null)
            {
                // All projects for that pair
                pairs = pairProjectOverlaps
                .Where(x => x.Employee1ID == topPair.Employee1ID && x.Employee2ID == topPair.Employee2ID)
                .OrderByDescending(x => x.DaysWorked)
                .Select(x => new PairsViewModel
                {
                    Employee1ID = x.Employee1ID,
                    Employee2ID = x.Employee2ID,
                    ProjectID = x.ProjectID,
                    DaysWorked = x.DaysWorked
                })
                .ToList();
            }
            return pairs;
        }

    }
}
