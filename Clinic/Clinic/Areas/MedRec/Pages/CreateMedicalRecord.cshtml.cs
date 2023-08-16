using Clinic.Database;
using Clinic.Database.Dictionaries;
using Clinic.Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace Clinic.Areas.MedRec.Pages
{
    public class CreateMedicalRecordModel : PageModel
    {
        public CreateMedicalRecordModel(ClinicTestContext clinicTestContext)
        {
            _clinicTestContext = clinicTestContext;
        }

        private readonly ClinicTestContext _clinicTestContext;

        public class ServiceOutputModel
        {
            public int RecDocContId { get; set; }
            public int ServId { get; set; }
            public int Count { get; set; }
            public int? WorkId { get; set; }
            public Dictionary<int, string> Workers { get; set; } = new Dictionary<int, string>();
            public List<MaterialModel> SelectedMaterials { get; set; } = new List<MaterialModel>();
        }

        public class ServiceInputModel
        {
            public int RecDocContId { get; set; }
            public int ServId { get; set; }
            public int Count { get; set; }
            public int WorkId { get; set; }
            public List<MaterialModel> SelectedMaterials { get; set; } = new List<MaterialModel>();
        }

        public class MaterialModel
        {
            public int RecDocMatId { get; set; }
            public int MatId { get; set; }
            public double Count { get; set; }
        }

        public class InputMedicalRecordModel
        {
            public string? Comment { get; set; }

            public List<ServiceInputModel> SelectedServices { get; set; } = new List<ServiceInputModel>();
        }

        public List<Service> AllService { get; set; }
        public List<Material> AllMaterials { get; set; }
        public List<ServiceOutputModel> SelectedServices { get; set; }
        public Dictionary<int, string> AllDoctors { get; set; }
        public Dictionary<int, string> AllServiceGroups { get; set; }
        public int CurrentDoctId { get; set; }
        public string PerSex { get; set; }

        public async System.Threading.Tasks.Task OnGetAsync(int RecDocId)
        {
            ReceptionDocumentRepository receptionDocumentRepository = new ReceptionDocumentRepository(_clinicTestContext);
            ReceptionDocument? currentReceptionDocument = await receptionDocumentRepository.GetAll()
                .Include(recDoc => recDoc.ReceptionDocumentContexts)
                    .ThenInclude(recDocCont => recDocCont.ReceptionDocumentMaterials)
                .Include(recDoc => recDoc.ReceptionDocumentContexts)
                    .ThenInclude(recDocCont => recDocCont.Serv)
                    .ThenInclude(serv => serv.Doct)
                    .ThenInclude(doct => doct.WorkerDoctors)
                    .ThenInclude(workDoct => workDoct.Work)
                .Include(recDoc => recDoc.Doc)
                    .ThenInclude(doc => doc.Per)
                .Where(recDoc => recDoc.RecDocId == RecDocId)
                .FirstOrDefaultAsync();

            if (currentReceptionDocument != null)
            {
                PerSex = currentReceptionDocument.Doc.Per.PerSex;
                CurrentDoctId = currentReceptionDocument.DoctId;
                SelectedServices = currentReceptionDocument.ReceptionDocumentContexts
                .Select(recDocCont => new ServiceOutputModel()
                {
                    RecDocContId = recDocCont.RecDocContId,
                    ServId = recDocCont.ServId,
                    Count = recDocCont.RecDocContCount,
                    WorkId = recDocCont.WorkId,
                    Workers = recDocCont.Serv.Doct.WorkerDoctors
                        .Where(workDoct => workDoct.DoctId == recDocCont.Serv.DoctId)
                        .Select(workDoct => new KeyValuePair<int, string>(
                            workDoct.WorkId,
                            workDoct.Work.WorkSurname.Trim() + " " + workDoct.Work.WorkName.Trim() + " " + workDoct.Work.WorkPatronimic.Trim()
                        ))
                    .ToDictionary(pair => pair.Key, pair => pair.Value),
                    SelectedMaterials = recDocCont.ReceptionDocumentMaterials
                        .Select(recDocMat => new MaterialModel()
                        {
                            RecDocMatId = recDocMat.RecDocMatId,
                            MatId = recDocMat.MatId,
                            Count = recDocMat.RecDocMatCount
                        })
                        .ToList()
                })
                .ToList();
                AllService = await receptionDocumentRepository.GetAllServices();
                AllDoctors = await receptionDocumentRepository.GetAllDoctors();
                AllServiceGroups = await receptionDocumentRepository.GetAllServiceGroups();
                AllMaterials = await receptionDocumentRepository.GetAllMaterials();

                TempData["RecDocId"] = RecDocId;
                TempData["PerSex"] = PerSex;
            }
            else
            {
                // todo
            }
        }


        [BindProperty]
        public InputMedicalRecordModel inputMedicalRecordModel { get; set; }

        public async System.Threading.Tasks.Task<IActionResult> OnPostAsync()
        {
            int recDocId = (int)TempData["RecDocId"];
            string perSex = (string)TempData["PerSex"];

            ReceptionDocumentRepository receptionDocumentRepository = new ReceptionDocumentRepository(_clinicTestContext);
            ReceptionDocument? currentReceptionDocument = await receptionDocumentRepository.GetAll()
                .Include(recDoc => recDoc.ReceptionDocumentContexts)
                    .ThenInclude(recDocCont => recDocCont.ReceptionDocumentMaterials)
                .Include(recDoc => recDoc.ReceptionDocumentContexts)
                    .ThenInclude(recDocCont => recDocCont.Serv)
                    .ThenInclude(serv => serv.PriceServices)
                .Where(recDoc => recDoc.RecDocId == recDocId)
                .FirstOrDefaultAsync();

            //Удаление материалов
            List<int> dbRecDocMatIds = currentReceptionDocument.ReceptionDocumentMaterials
                .Select(recDocMat => recDocMat.RecDocMatId)
                .ToList();
            List<MaterialModel> curMaterialModels = new List<MaterialModel>();
            foreach (var selectedService in inputMedicalRecordModel.SelectedServices)
            {
                curMaterialModels.AddRange(selectedService.SelectedMaterials);
            }
            List<int> carRecDocMatIds = curMaterialModels.Select(curMatModel => curMatModel.RecDocMatId).ToList();
            List<int> removeRecDocMat = dbRecDocMatIds.Except(carRecDocMatIds).ToList();
            foreach (var recDocMat in removeRecDocMat)
            {
                await receptionDocumentRepository.DeleteRecDocMaterialAsync(recDocMat);
            }

            //Удаление услуг
            List<int> dbRecDocContIds = currentReceptionDocument.ReceptionDocumentContexts
                .Select(recDocCont => recDocCont.RecDocContId)
                .ToList();
            List<int> curRecDocContIds = inputMedicalRecordModel.SelectedServices
                .Select(servInputModel => servInputModel.RecDocContId)
                .ToList();
            List<int> removeRecDocCont = dbRecDocContIds.Except(curRecDocContIds).ToList();
            foreach (var recDocCont in removeRecDocCont)
            {
                await receptionDocumentRepository.DeleteRecDocContextAsync(recDocCont);
            }

            //Получение цен на услуги
            Dictionary<int, double> priceServices = await receptionDocumentRepository
                .GetPriceServices(inputMedicalRecordModel.SelectedServices
                .Select(selectServ => selectServ.ServId)
                .ToList(), perSex);

            //Получение цен на материалы
            List<int> MatIds = new List<int>();
            foreach (var selectedService in inputMedicalRecordModel.SelectedServices)
            {
                foreach (var selectedMaterial in selectedService.SelectedMaterials)
                {
                    MatIds.Add(selectedMaterial.MatId);
                }
            }
            Dictionary<int, double> priceMaterials = await receptionDocumentRepository
                .GetPriceMaterials(MatIds);

            //Добавление или изменение услуг и материалов
            foreach (var selectedService in inputMedicalRecordModel.SelectedServices)
            {
                if (selectedService.RecDocContId != 0) //Изменение услуги
                {
                    var dbRecDocCont = currentReceptionDocument.ReceptionDocumentContexts
                        .SingleOrDefault(recDocCont => recDocCont.RecDocContId == selectedService.RecDocContId);
                    dbRecDocCont.ServId = selectedService.ServId;
                    dbRecDocCont.RecDocContCount = selectedService.Count;
                    dbRecDocCont.WorkId = selectedService.WorkId;

                    foreach (var selectedMaterial in selectedService.SelectedMaterials)
                    {
                        if (selectedMaterial.RecDocMatId != 0) //Изменение материала
                        {
                            var dbRecDocMat = currentReceptionDocument.ReceptionDocumentMaterials
                                .SingleOrDefault(recDocMat => recDocMat.RecDocMatId == selectedMaterial.RecDocMatId);
                            dbRecDocMat.MatId = selectedMaterial.MatId;
                            dbRecDocMat.RecDocMatCount = selectedMaterial.Count;
                        }
                        else //Добавление материала
                        {
                            ReceptionDocumentMaterial recDocMat = new ReceptionDocumentMaterial
                            {
                                RecDocId = recDocId,
                                RecDocContId = selectedService.RecDocContId,
                                MatId = selectedMaterial.MatId,
                                RecDocMatCount = selectedMaterial.Count,
                                RecDocMatCost = priceMaterials[selectedMaterial.MatId],
                                RecDocMatNumber = 5,
                                RecDocMatNorm = 0,
                                RecDocMatUseSum = true
                            };
                            dbRecDocCont.ReceptionDocumentMaterials.Add(recDocMat);
                        }
                    }
                }
                else //Добавление услуги
                {
                    if (selectedService.WorkId == 0) continue;
                    ReceptionDocumentContext recDocCont = new ReceptionDocumentContext
                    {
                        RecDocId = recDocId,
                        ServId = selectedService.ServId,
                        WorkId = selectedService.WorkId,
                        InspResId = 2,
                        RecDocContCost = priceServices[selectedService.ServId],
                        RecDocContCount = selectedService.Count
                    };
                    currentReceptionDocument.ReceptionDocumentContexts.Add(recDocCont);

                    foreach (var selectedMaterial in selectedService.SelectedMaterials)
                    {
                        //Добавление материала
                        ReceptionDocumentMaterial recDocMat = new ReceptionDocumentMaterial
                        {
                            RecDocId = recDocId,
                            RecDocContId = selectedService.RecDocContId,
                            MatId = selectedMaterial.MatId,
                            RecDocMatCount = selectedMaterial.Count,
                            RecDocMatCost = priceMaterials[selectedMaterial.MatId],
                            RecDocMatNumber = 5,
                            RecDocMatNorm = 0,
                            RecDocMatUseSum = true
                        };
                        recDocCont.ReceptionDocumentMaterials.Add(recDocMat);
                    }
                }
            }
            await receptionDocumentRepository.SaveAsync(currentReceptionDocument);

            return RedirectToPage("ViewReceptionDocument");
        }

        public async Task<IActionResult> OnGetWorkersByDoctorAsync(int DoctId)
        {
            ReceptionDocumentRepository receptionDocumentRepository = new ReceptionDocumentRepository(_clinicTestContext);
            List<Worker>? workers = await receptionDocumentRepository.GetWorkersByDoctor(DoctId);

            Dictionary<int, string> workersDictionary = new Dictionary<int, string>();
            if (workers != null)
            {
                workersDictionary = workers
                    .Select(worker => new KeyValuePair<int, string>(
                        worker.WorkId,
                        worker.WorkSurname.Trim() + " " + worker.WorkName.Trim() + " " + worker.WorkPatronimic.Trim()
                    ))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            return new JsonResult(workersDictionary);
        }
    }
}
