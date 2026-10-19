using System.Collections.Generic;
using EReceiptMiddleware.DTO;
using MediatR;
using System.Threading.Tasks;
using SubmitServ.IRepository.ETA;




namespace SubmitServ.Repository.ETA
{

    public class EReceiptToolsService : IEReceiptToolsService
    {
        private readonly IMediator _mediator;

        public EReceiptToolsService(IMediator mediator)
        {
            _mediator = mediator;
        }



        public async Task<GenerateUuidResponseDto> GenerateUuidFromJson(string jsonPayload)
        {
            global::ETA.eReceipt.IntegrationToolkit.Application.Commands.GenerateUuid.Command command = new global::ETA.eReceipt.IntegrationToolkit.Application.Commands.GenerateUuid.Command
            {
                ReceiptJson = jsonPayload
            };
            global::ETA.eReceipt.IntegrationToolkit.Application.Commands.GenerateUuid.Result result = await _mediator.Send(command);

            return new GenerateUuidResponseDto
            {
                CorrelationId = result.CorrelationId,
                Code = result.Code,
                Target = result.Target,
                Message = result.Message,
                Details = result.Details as List<string>,
                Uuid = result.Uuid,
                UpdatedReceiptJson = result.UpdatedReceiptJson?.ToString()
            };
        }




        public async Task<GenerateQrCodeResponseDto> GenerateQrCode(string receipt)
        {
            global::ETA.eReceipt.IntegrationToolkit.Application.Commands.GenerateQrCode.Command command = new global::ETA.eReceipt.IntegrationToolkit.Application.Commands.GenerateQrCode.Command
            {
                ReceiptJson = receipt
            };
            global::ETA.eReceipt.IntegrationToolkit.Application.Commands.GenerateQrCode.Result result = await _mediator.Send(command);
            return new GenerateQrCodeResponseDto
            {
                CorrelationId = result.CorrelationId,
                Code = result.Code,
                Target = result.Target,
                Message = result.Message,
                Details = result.Details as List<string>,
                QrCode = result.QrCode,
                QrCodeImageBase64 = result.QrCodeImageBase64
            };
        }

    }
}

