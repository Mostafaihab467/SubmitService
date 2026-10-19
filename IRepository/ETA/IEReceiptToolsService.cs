using EReceiptMiddleware.DTO;
using System.Threading.Tasks;

namespace SubmitServ.IRepository.ETA
{
    public interface IEReceiptToolsService
    {
        Task<GenerateUuidResponseDto> GenerateUuidFromJson(string jsonPayload);
        Task<GenerateQrCodeResponseDto> GenerateQrCode(string receipt);
    }
}
