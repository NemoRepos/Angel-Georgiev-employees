using Microsoft.AspNetCore.Mvc;
using SirmaTask.Models;
using SirmaTask.Services;
using System.Text;

namespace SirmaTask.Controllers
{
    public class TaskController : Controller
    {
        private readonly IDateParseService _dateParser;
        public TaskController(IDateParseService dateParseService)
        {
            _dateParser = dateParseService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReadFile(UploadedFileViewModel csvFile)
        {
            if (!ValidateCSV(csvFile))
            {
                return View("Index", csvFile);
            }
            List<EmployeeProjectViewModel> projects = Parse(csvFile.CSVTable);
            List<PairsViewModel> pairs = GetPairs(projects);
            return Json(pairs);
        }

        private bool ValidateCSV(UploadedFileViewModel fileModel)
        {
            if (!ModelState.IsValid)
            {
                return false;
            }

            if (fileModel.CSVTable == null || fileModel.CSVTable.Length == 0)
            {
                ModelState.AddModelError("CSVTable", "Invalid CSV file.");
                return false;
            }
            else if (!fileModel.CSVTable.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("CSVTable", "Only CSV files are allowed.");
            }
            return true;
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

                    if(_dateParser.ParseDate(values[2]) == null)
                        continue; // Invalid DateFrom, skip this record

                    EmployeeProjectViewModel project = new EmployeeProjectViewModel()
                    {
                        EmployeeID = int.Parse(values[0]),
                        ProjectID = int.Parse(values[1]),
                        DateFrom = _dateParser.ParseNonNullDate(values[2]),
                        DateTo = _dateParser.ParseDate(values[3]) ?? DateTime.Today
                    };
                    projects.Add(project);
                }
            }
            return projects;
        }
        private List<PairsViewModel> GetPairs(List<EmployeeProjectViewModel> projects)
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