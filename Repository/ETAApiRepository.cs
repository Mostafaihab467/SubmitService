using SubmitServ.Models.ETA;
using SubmitServ.Repository.ETA;
using Newtonsoft.Json;
using SubmitServ.IRepository.ETA;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SubmitServ.Repository
{
    public class ETAApiRepository
    {
        private static ETAApiRepository _instance;
        private IEReceiptToolsService _etaToolsService;

        // Static fields for connection and token reuse
        private static readonly HttpClient _httpClient = new HttpClient();
        private static Token _cachedToken;
        private static DateTime _tokenExpiryTime;
        private static readonly object _tokenLock = new object();

        public int MaxTotalHoursSumittion = 23;

        public static ETAApiRepository Get()
        {
            if (_instance == null)
            {
                _instance = new ETAApiRepository();
            }
            return _instance;
        }

        public ETAApiRepository()
        {
            TryResolveEtaToolsService();
        }

        private bool TryResolveEtaToolsService()
        {
            if (_etaToolsService != null)
                return true;

            if (!Helpers.ServiceProvider.IsBuilt)
                return false;

            if (!Helpers.ServiceProvider.IsRegistered<IEReceiptToolsService>())
            {
                Console.WriteLine("Warning: IEReceiptToolsService is not registered in ServiceProvider.");
                return false;
            }

            try
            {
                _etaToolsService = Helpers.ServiceProvider.GetService<IEReceiptToolsService>();
                return _etaToolsService != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to resolve IEReceiptToolsService: {ex.Message}");
                _etaToolsService = null;
                return false;
            }
        }

        /// <summary>
        /// Extracts the first receipt from the JSON payload
        /// </summary>
        /// <param name="payload">JSON payload string</param>
        /// <returns>First receipt object with all fields</returns>
        private SubmitServ.Models.EReceiptModel ExtractFirstReceipt(string payload)
        {
            try
            {
                var receiptsModelObj = JsonConvert.DeserializeObject<SubmitServ.Models.receiptsModel>(payload);
                if (receiptsModelObj?.receipts != null && receiptsModelObj.receipts.Length > 0)
                {
                    return receiptsModelObj.receipts[0];
                }
            }
            catch (Exception)
            {
                // If parsing fails, return null and continue with original payload
            }
            return null;
        }

        /// <summary>
        /// Checks if the receipt date is older than 23 hours from current time
        /// </summary>
        /// <param name="receiptDate">The receipt date to check</param>
        /// <returns>True if date is older than 23 hours</returns>
        private bool ShouldRegenerateUuid(DateTime receiptDate)
        {
            var timeDifference = DateTime.Now - receiptDate;
            return timeDifference.TotalHours > MaxTotalHoursSumittion;
        }

        /// <summary>
        /// Updates the payload by clearing the UUID and updating the date to current time
        /// </summary>
        /// <param name="payload">Original JSON payload</param>
        /// <returns>Updated payload with cleared UUID and current date</returns>
        private string ClearUuidAndUpdateDate(string payload)
        {
            try
            {
                var receiptsModel = JsonConvert.DeserializeObject<SubmitServ.Models.receiptsModel>(payload);
                if (receiptsModel?.receipts != null && receiptsModel.receipts.Length > 0)
                {
                    // Clear the UUID and update the date to ISO 8601 format without timezone (minus 4 hours)
                    receiptsModel.receipts[0].header.uuid = "";
                    receiptsModel.receipts[0].header.dateTimeIssued = DateTime.Now.AddHours(-4).ToString("yyyy-MM-ddTHH:mm:ss");

                    return JsonConvert.SerializeObject(receiptsModel);
                }
            }
            catch (Exception)
            {
                // If parsing fails, return original payload
            }
            return payload;
        }

        /// <summary>
        /// Updates the payload with the new UUID
        /// </summary>
        /// <param name="payload">Original JSON payload</param>
        /// <param name="newUuid">New UUID to set</param>
        /// <returns>Updated payload with new UUID</returns>
        private string UpdatePayloadWithNewUuid(string payload, string newUuid)
        {
            try
            {
                var receiptsModel = JsonConvert.DeserializeObject<SubmitServ.Models.receiptsModel>(payload);
                if (receiptsModel?.receipts != null && receiptsModel.receipts.Length > 0)
                {
                    receiptsModel.receipts[0].header.uuid = newUuid;
                    return JsonConvert.SerializeObject(receiptsModel);
                }
            }
            catch (Exception)
            {
                // If parsing fails, return original payload
            }
            return payload;
        }

        /// <summary>
        /// Generates UUID for the first receipt and reconstructs the full payload
        /// </summary>
        /// <param name="payloadWithUpdatedDate">Original JSON payload with updated date</param>
        /// <param name="receipt">First receipt object to generate UUID for</param>
        /// <returns>Updated payload with new UUID</returns>
        private async Task<string> GenerateUuidForReceiptAsync(string payloadWithUpdatedDate, SubmitServ.Models.EReceiptModel receipt)
        {
            try
            {
                // Serialize the receipt directly (not wrapped in a "receipt" property)
                var receiptJson = JsonConvert.SerializeObject(receipt);

                // Generate UUID using the first receipt
                var uuidResult = await _etaToolsService.GenerateUuidFromJson(receiptJson);

                if (uuidResult != null && !string.IsNullOrEmpty(uuidResult.Uuid))
                {
                    // Update the payload with updated date (not original) with the new UUID
                    var receiptsModel = JsonConvert.DeserializeObject<SubmitServ.Models.receiptsModel>(payloadWithUpdatedDate);
                    if (receiptsModel?.receipts != null && receiptsModel.receipts.Length > 0)
                    {
                        receiptsModel.receipts[0].header.uuid = uuidResult.Uuid;
                        return JsonConvert.SerializeObject(receiptsModel);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating UUID for header: {ex.Message}");
            }

            return payloadWithUpdatedDate; // Return updated payload if UUID generation fails
        }

        public async Task<Token> GetTokenAsync()
        {
            // Return cached token if still valid (adding 5 minute safety buffer)
            if (_cachedToken != null && DateTime.Now < _tokenExpiryTime.AddMinutes(-5))
            {
                return _cachedToken;
            }

            try
            {
                var client = new HttpClient();
                var form = new Dictionary<string,
                    string> {
                            {
                                "grant_type",
                                "client_credentials"
                            },
                            {
                                "client_id",AppSettings.client_id
                            },
                            {
                                "client_secret",AppSettings.client_secret
                            }

            };
                client.DefaultRequestHeaders.Add("pososversion", AppSettings.pososversion);
                client.DefaultRequestHeaders.Add("posserial", AppSettings.posserial);

                System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                var tokenResponse = await client.PostAsync(AppSettings.ETATokenURL, new FormUrlEncodedContent(form));
                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<Token>(tokenJson);
                token.IsSuccess = true;

                // Cache the new token securely
                lock (_tokenLock)
                {
                    _cachedToken = token;
                    // Token expiresIn is usually seconds. Default to 1 hour if absent.
                    _tokenExpiryTime = DateTime.Now.AddSeconds(token.expires_in > 0 ? token.expires_in : 3600);
                }

                return token;
            }
            catch (Exception ex)
            {
                Token token = new Token();
                token.error = Convert.ToString(ex.InnerException ?? ex);
                return token;
            }
        }
        public async Task<ETASubmissionAPIResultModel> SendEReceiptSubmitDirect(string postData)
        {


            #region Payload

            //            {
            //                "receipts": [
            //                  {
            //                    "header": {
            //                        "dateTimeIssued": "2026-02-19T18:26:07",
            //        "receiptNumber": "B0001583422122025",
            //        "uuid": "8e164dc471746f613fb64abd28bab872926cddc7994398b3c94ecc4548af8e8b",
            //        "previousUUID": "",
            //        "currency": "EGP",
            //        "exchangeRate": 0.00000
            //                    },
            //      "documentType": {
            //                        "receiptType": "S",
            //        "typeVersion": "1.2"
            //      },
            //      "seller": {
            //                        "rin": "205006930",
            //        "companyTradeName": "Orange Egypt Tele",
            //        "branchCode": "0",
            //        "branchAddress": {
            //                            "country": "EG",
            //          "governate": "Cairo",
            //          "regionCity": "Bulaq",
            //          "street": "\u0627\u0644\u0646\u064A\u0644",
            //          "buildingNumber": "2005"
            //        },
            //        "deviceSerialNumber": "E1EA0240-3140-49CF-AFA6-25EC735E018A",
            //        "activityCode": "6920"
            //      },
            //      "buyer": {
            //                        "type": "P",
            //        "id": "",
            //        "name": "",
            //        "mobileNumber": ""
            //      },
            //      "itemData": [
            //        {
            //                        "internalCode": "3",
            //          "description": "Stamps and SDF",
            //          "itemType": "EGS",
            //          "itemCode": "EG-205006930-OE-3",
            //          "unitType": "EA",
            //          "quantity": 1.00000,
            //          "unitPrice": 10.00000,
            //          "netSale": 10.00000,
            //          "totalSale": 10.00000,
            //          "taxableItems": [
            //            {
            //                            "taxType": "T1",
            //              "amount": 1.51200,
            //              "subType": "V001",
            //              "rate": 14.00
            //            },
            //            {
            //                            "taxType": "T2",
            //              "amount": 0.80000,
            //              "subType": "Tbl01",
            //              "rate": 8.00
            //            }
            //          ],
            //          "total": 12.31200
            //        },
            //        {
            //                        "internalCode": "4",
            //          "description": "Services Access Fees",
            //          "itemType": "EGS",
            //          "itemCode": "EG-205006930-OE-4",
            //          "unitType": "EA",
            //          "quantity": 1.00000,
            //          "unitPrice": 5.00000,
            //          "netSale": 5.00000,
            //          "totalSale": 5.00000,
            //          "taxableItems": [
            //            {
            //                            "taxType": "T1",
            //              "amount": 0.75600,
            //              "subType": "V001",
            //              "rate": 14.00
            //            },
            //            {
            //                            "taxType": "T2",
            //              "amount": 0.40000,
            //              "subType": "Tbl01",
            //              "rate": 8.00
            //            }
            //          ],
            //          "total": 6.15600
            //        },
            //        {
            //                        "internalCode": "4",
            //          "description": "Services Access Fees",
            //          "itemType": "EGS",
            //          "itemCode": "EG-205006930-OE-4",
            //          "unitType": "EA",
            //          "quantity": 1.00000,
            //          "unitPrice": 5.00000,
            //          "netSale": 5.00000,
            //          "totalSale": 5.00000,
            //          "taxableItems": [
            //            {
            //                            "taxType": "T1",
            //              "amount": 0.75600,
            //              "subType": "V001",
            //              "rate": 14.00
            //            },
            //            {
            //                            "taxType": "T2",
            //              "amount": 0.40000,
            //              "subType": "Tbl01",
            //              "rate": 8.00
            //            }
            //          ],
            //          "total": 6.15600
            //        },
            //        {
            //                        "internalCode": "1",
            //          "description": "Telephony Access Fees",
            //          "itemType": "EGS",
            //          "itemCode": "EG-205006930-OE-1",
            //          "unitType": "EA",
            //          "quantity": 1.00000,
            //          "unitPrice": 105.00000,
            //          "netSale": 105.00000,
            //          "totalSale": 105.00000,
            //          "taxableItems": [
            //            {
            //                            "taxType": "T1",
            //              "amount": 15.87600,
            //              "subType": "V001",
            //              "rate": 14.00
            //            },
            //            {
            //                            "taxType": "T2",
            //              "amount": 8.40000,
            //              "subType": "Tbl01",
            //              "rate": 8.00
            //            }
            //          ],
            //          "total": 129.27600
            //        }
            //      ],
            //      "totalSales": 125.00000,
            //      "netAmount": 125.00000,
            //      "totalAmount": 153.90000,
            //      "taxTotals": [
            //        {
            //                        "taxType": "T1",
            //          "amount": 18.90000
            //        },
            //        {
            //                        "taxType": "T2",
            //          "amount": 10.00000
            //        }
            //      ],
            //      "extraReceiptDiscountData": [
            //        {
            //                        "amount": 0.00000,
            //          "description": "Tax",
            //          "rate": 0.0
            //        }
            //      ],
            //      "paymentMethod": "C"
            //                  }
            //  ]
            //}

            #endregion

            // first i will convert the payload into json then take only the header  
            // Check Here I want To Call Function Here that Will Check The Date of the Current Receipt  if it IS bigger than current Date with 23 hour it will 
            // Modify it with Date Now then Regenrate UUID 
            // regenrate UUID work As follow i will call   GenerateUuidFromJson()  from the repo @ETAToolKitUUID.cs pass it the Header i mentioned it will run Hash algorith then  update the UUID but first before apply the generate you discard the UUID feild 8e164dc471746f613fb64abd28bab872926cddc7994398b3c94ecc4548af8e8b  the UUID  = "" that should be before genrate 

            // Check if UUID regeneration is needed
            #region ShouldRegenerateUuid
            try
            {
                var receipt = ExtractFirstReceipt(postData);
                if (receipt != null && !string.IsNullOrEmpty(receipt.header.dateTimeIssued))
                {
                    // Parse the dateTimeIssued string to DateTime
                    DateTime issuedDate;
                    if (DateTime.TryParse(receipt.header.dateTimeIssued, out issuedDate))
                    {
                        // Check if we need to regenerate UUID (either because date is old OR UUID is empty)
                        bool shouldRegenerate = ShouldRegenerateUuid(issuedDate) || string.IsNullOrEmpty(receipt.header.uuid);

                        if (shouldRegenerate)
                        {
                            // Log the action
                            if (ShouldRegenerateUuid(issuedDate))
                            {
                                Console.WriteLine($"Updating receipt date from {receipt.header.dateTimeIssued} to {DateTime.Now:yyyy-MM-ddTHH:mm:ss}");
                            }
                            else if (string.IsNullOrEmpty(receipt.header.uuid))
                            {
                                Console.WriteLine("UUID is empty, generating new UUID");
                            }

                            // Generate new UUID using header-only approach with updated date
                            if (TryResolveEtaToolsService())
                            {
                                try
                                {
                                    // First, clear UUID and update date in the payload
                                    var payloadWithUpdatedDate = ClearUuidAndUpdateDate(postData);

                                    // Extract the updated receipt with current date
                                    var updatedReceipt = ExtractFirstReceipt(payloadWithUpdatedDate);
                                    if (updatedReceipt != null)
                                    {
                                        // Generate UUID using the receipt with updated date
                                        var uuidResult = await GenerateUuidForReceiptAsync(payloadWithUpdatedDate, updatedReceipt);

                                        if (!string.IsNullOrEmpty(uuidResult) && uuidResult != postData)
                                        {
                                            postData = uuidResult;
                                            Console.WriteLine($"Generated new UUID for header with updated date");
                                        }
                                        else
                                        {
                                            // If UUID generation fails, use the payload with updated date but no UUID
                                            postData = payloadWithUpdatedDate;
                                            Console.WriteLine("Warning: UUID generation failed, using payload with updated date");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // If UUID generation throws an exception, log warning but continue with original payload
                                    Console.WriteLine($"Warning: UUID generation error: {ex.Message}, continuing with original payload");
                                }
                            }
                            else
                            {
                                // If EReceiptToolsService is not available, update date but skip UUID generation
                                //postData = ClearUuidAndUpdateDate(postData);
                                //Console.WriteLine("Warning: EReceiptToolsService not available, updated date but skipped UUID generation");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If any error occurs during UUID regeneration, log and continue with original payload
                Console.WriteLine($"Warning: Error during UUID regeneration: {ex.Message}");
            }
            #endregion ShouldRegenerateUuid

            ETASubmissionAPIResultModel _Result = new ETASubmissionAPIResultModel();
            Token _Token = new Token();
            try
            {
                _Token = await GetTokenAsync();
                _Result.expires_in = _Token.expires_in;
                _Result.TokenRequestDate = DateTime.Now;
                _Result.Token = _Token.access_token;
                if (_Token.IsSuccess)
                {
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, AppSettings.ETAApiURL + "v1/receiptsubmissions"))
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _Token.access_token);
                        requestMessage.Content = new StringContent(postData, Encoding.UTF8, "application/json");

                        System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

                        HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
                        string jsonresponse = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Accepted)
                        {
                             _Result.IsSuccess = true;
                             _Result.Result = JsonConvert.DeserializeObject<SuccessModel>(jsonresponse);
                             return _Result;
                        }
                        else
                        {
                            _Result.IsSuccess = false;
                            _Result.error = $"HTTP {response.StatusCode}: {jsonresponse}";
                            return _Result;
                        }
                    }
                }

                _Result.IsSuccess = false;
                _Result.error = _Token.error;

            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    _Result.IsSuccess = false;
                    var x = reader.ReadToEnd();
                    //if (DEFC.Util.DataValidation.DataType.IsNullOrEmptyOrWhiteSpace(x))
                    _Result.error = Convert.ToString(ex.Message);
                    //else
                    //    _Result.error = JsonConvert.DeserializeObject<errorResponse>(reader.ReadToEnd()).error;
                }
            }
            catch (Exception ex)
            {
                _Result.IsSuccess = false;
                _Result.error = Convert.ToString(ex.Message);

            }
            return _Result;


        }
    }
}
