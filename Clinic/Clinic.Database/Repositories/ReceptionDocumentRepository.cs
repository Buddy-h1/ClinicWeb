using Clinic.Database.Dictionaries;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Clinic.Database.Repositories
{
    public class ReceptionDocumentRepository : Repository<ClinicTestContext, ReceptionDocument>
    {
        public ReceptionDocumentRepository(ClinicTestContext clinicTestContext) : base(clinicTestContext)
        {
        }

        public async Task<bool> DeleteRecDocContextAsync(int recDocContId)
        {
            try
            {
                if (await _context.ReceptionDocumentContexts
                    .Where(recDocCont => recDocCont.RecDocContId == recDocContId)
                    .ExecuteDeleteAsync() > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> DeleteRecDocMaterialAsync(int recDocMatId)
        {
            try
            {
                if (await _context.ReceptionDocumentMaterials
                    .Where(recDocMat => recDocMat.RecDocMatId == recDocMatId)
                    .ExecuteDeleteAsync() > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<Worker>?> GetWorkersByDoctor(int doctId)
        {
            return await _context.WorkerDoctors
                .Where(workDoct => workDoct.DoctId == doctId)
                .Select(workDoct => workDoct.Work)
                .ToListAsync();
        }

        public async Task<List<Service>> GetAllServices()
        {
            return await _context.Services
                .Include(serv => serv.Doct)
                .Include(serv => serv.ServGrp)
                .Include(serv => serv.ServType)
                .Include(serv => serv.PriceServices)
                .ToListAsync();
        }

        public async Task<Dictionary<int, string>> GetAllDoctors()
        {
            return await _context.Doctors
                    .Select(doct => new KeyValuePair<int, string>(
                        doct.DoctId,
                        doct.DoctName
                    ))
                    .ToDictionaryAsync(pair => pair.Key, pair => pair.Value);
        }

        public async Task<Dictionary<int, string>> GetAllServiceGroups()
        {
            return await _context.ServiceGroups
                    .Select(servGrp => new KeyValuePair<int, string>(
                        servGrp.ServGrpId,
                        servGrp.ServGrpName
                    ))
                    .ToDictionaryAsync(pair => pair.Key, pair => pair.Value);
        }

        public async Task<List<Material>> GetAllMaterials()
        {
            return await _context.Materials
                .Include(mat => mat.Meas)
                .Include(mat => mat.PriceMaterials)
                .ToListAsync();
        }

        public async Task<Dictionary<int, double>> GetPriceServices(List<int> ServIds, string PerSex)
        {
            return await _context.Services
                .Include(serv => serv.PriceServices)
                .Where(serv => ServIds.Contains(serv.ServId))
                .Select(serv => new KeyValuePair<int, double>(
                    serv.ServId,
                    PerSex == "Муж" ? serv.PriceServices.FirstOrDefault(priceSer => priceSer.PriceTypeId == 1).PriceServValueMan :
                        serv.PriceServices.FirstOrDefault(priceSer => priceSer.PriceTypeId == 1).PriceServValueWoman
                 ))
                .ToDictionaryAsync(pair => pair.Key, pair => pair.Value);
        }

        public async Task<Dictionary<int, double>> GetPriceMaterials(List<int> MatIds)
        {
            return await _context.Materials
                .Include(mat => mat.PriceMaterials)
                .Where(mat => MatIds.Contains(mat.MatId))
                .Select(mat => new KeyValuePair<int, double>(
                    mat.MatId,
                    mat.PriceMaterials.FirstOrDefault().PriceMatValue
                 ))
                .ToDictionaryAsync(pair => pair.Key, pair => pair.Value);
        }
    }
}
