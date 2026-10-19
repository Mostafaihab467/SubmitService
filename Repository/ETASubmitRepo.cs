using DEFC.Util.DataValidation;
using SubmitServ.Models.ETA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmitServ.Repository
{
   
    public class ETASubmitRepo
    {        

        //public async Task<ETASubmissionAPIResultModel> ETAReceipt(ReceiptsModel model)
        //{
          
        //    ETASubmissionAPIResultModel _Result = new ETASubmissionAPIResultModel();
        //    string error = "No Proccess Happend";

        //    try
        //    {
        //        if (!DataType.IsNullOrEmptyOrWhiteSpace(model.Token))
        //            _Result = ETAApiRepository.Get().SendEReceiptSubmit(model, AppSettings.ETAApiURL);
        //        else
        //            _Result =await ETAApiRepository.Get().SendEReceiptSubmit(model);

        //    }
        //    catch (Exception ex)
        //    {
        //        _Result.IsSuccess = false;
        //        _Result.error = Convert.ToString(ex.InnerException);
               
        //    }
        //    return _Result;
        //}

        //public async Task<ETASubmissionAPIResultModel> ETASubmissionResponse(Models.EReceiptModel TaxReceipt, DateTime? TokenRequestDate, int? expires_in, string Token)
        //{
        //    ReceiptsModel receiptsModel = new ReceiptsModel();
           
        //    if (TokenRequestDate == null)
        //    {
        //        receiptsModel.TokenRequestDate = DateTime.Now;
        //        receiptsModel.expires_in = 0;
        //        receiptsModel.Token = "";

        //        receiptsModel.receipts = new Models.EReceiptModel[]
        //        {
        //                   new Models.EReceiptModel
        //                   {
        //                        header = TaxReceipt.header,
        //                        documentType = TaxReceipt.documentType,
        //                        seller = TaxReceipt.seller,
        //                        buyer = TaxReceipt.buyer,
        //                        itemData = TaxReceipt.itemData,
        //                        totalSales = TaxReceipt.totalSales,
        //                        netAmount = TaxReceipt.netAmount, 
        //                        totalAmount = TaxReceipt.totalAmount,
        //                        taxTotals = TaxReceipt.taxTotals,
        //                        paymentMethod = TaxReceipt.paymentMethod,
        //                   }
        //        };
        //        var response = await ETAReceipt(receiptsModel);
 
        //        return response;
        //    }

        //    else
        //    { 

        //        TimeSpan? timeSpan = DateTime.Now - TokenRequestDate;
        //        var minutes = timeSpan.Value.TotalMinutes;
        //        if (expires_in > minutes)
        //        {
        //            receiptsModel.expires_in =Convert.ToInt32(expires_in);
        //            receiptsModel.Token = Token;
        //            receiptsModel.TokenRequestDate = TokenRequestDate;
        //            receiptsModel.receipts = new Models.EReceiptModel[]
        //            {
        //             new Models.EReceiptModel
        //             {
        //                 header = TaxReceipt.header,
        //                 documentType = TaxReceipt.documentType,
        //                 seller = TaxReceipt.seller,
        //                 buyer = TaxReceipt.buyer,
        //                 itemData = TaxReceipt.itemData,
        //                 totalSales = TaxReceipt.totalSales,
        //                 netAmount = TaxReceipt.netAmount, 
        //                 totalAmount = TaxReceipt.totalAmount,
        //                 taxTotals = TaxReceipt.taxTotals,
        //                 paymentMethod = TaxReceipt.paymentMethod,
        //             }
        //            };

        //            return await ETAReceipt(receiptsModel);
        //        }
        //    }
        //    // }
        //    return null;
        //}
    }
}
