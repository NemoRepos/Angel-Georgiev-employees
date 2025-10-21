using Microsoft.AspNetCore.Mvc;
using SirmaTask.Models;

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
            //validate
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

            using (var reader = new StreamReader(csv.OpenReadStream()))
            {
                string line = reader.ReadLine(); // Skip header line
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(',');

                    if (_dateParseService.ParseDate(values[2]) == null)
                        continue; // Invalid DateFrom, skip this record

                    EmployeeProjectViewModel project = new EmployeeProjectViewModel()
                    {
                        EmployeeID = int.Parse(values[0]),
                        ProjectID = int.Parse(values[1]),
                        DateFrom = _dateParseService.ParseNonNullDate(values[2]),
                        DateTo = _dateParseService.ParseDate(values[3]) ?? DateTime.Today
                    };
                    projects.Add(project);
                }
            }
            return projects;
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
                let daysWorked = (overlapEnd - overlapStart).Value.TotalDays
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
