using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Raven.DB.PSQL.Entity;
using Raven.DB.PSQL.Logger;

namespace Raven.Pages
{
    public class LogsModel : PageModel
    {

        public List<Logs> logs { get; set; } = new List<Logs>();

        [BindProperty(SupportsGet = true)]
        public DateTime startDate { get; set; } = DateTime.MinValue.ToUniversalTime();

        [BindProperty(SupportsGet = true)]
        public DateTime endDate { get; set; } = DateTime.MaxValue.ToUniversalTime();

        [BindProperty(SupportsGet = true)]
        public List<string> selectedLevels { get; set; } = new List<string>();


        public void OnGet()
        {
            var dbResponse = LogsHandler.GetLogs();
            logs = dbResponse.Result;
        }

    }
}
