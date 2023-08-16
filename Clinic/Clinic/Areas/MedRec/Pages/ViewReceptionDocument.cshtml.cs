using Clinic.Database;
using Clinic.Database.Dictionaries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Areas.MedRec.Pages
{
    public class ViewReceptionDocumentModel : PageModel
    {
        public ViewReceptionDocumentModel(ClinicTestContext clinicTestContext)
        {
            _clinicTestContext = clinicTestContext;
        }

        private readonly ClinicTestContext _clinicTestContext;

        public List<ReceptionDocument> AllReceptionDocuments { get; set; }
        public async System.Threading.Tasks.Task OnGetAsync()
        {
            AllReceptionDocuments = await _clinicTestContext.ReceptionDocuments.ToListAsync();
        }
    }
}
