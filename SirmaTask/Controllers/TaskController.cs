using Microsoft.AspNetCore.Mvc;
using SirmaTask.Models;
using SirmaTask.Services;
using System.Text;

namespace SirmaTask.Controllers
{
    public class TaskController : Controller
    {
        private readonly IQueryService _queryService;
        public TaskController(IQueryService queryService)
        {
            _queryService = queryService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReadFile(UploadedFileViewModel csvFile)
        {
            return _queryService.GetPairs(csvFile, this);
        }
    }
}