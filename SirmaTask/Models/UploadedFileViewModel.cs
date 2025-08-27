using System.ComponentModel.DataAnnotations;

namespace SirmaTask.Models
{
    public class UploadedFileViewModel
    {
        [Required(ErrorMessage ="Select file!")]
        public IFormFile CSVTable { get; set; }
    }
}