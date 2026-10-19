using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using SubmitServ.ApiModels;
using SubmitServ.APIModels;
using SubmitServ.Helpers;
using SubmitServ.Models;
using System.Reflection;
using System.Collections.Generic;
using System.Data.Common;
using System;
using Newtonsoft.Json;
using DEFC.Util.DataValidation;

namespace SubmitServ.Repository
{
    public class EreciptsRepository
    {


        public async Task<List<EReceiptLogsForSubmit>> GetEReceiptLogsAsync(int limit)
        {

            List<EReceiptLogsForSubmit> models = new List<EReceiptLogsForSubmit>();

            using (SqlConnection connection = new SqlConnection(AppSettings.connectionStr))
            {
                using (SqlCommand command = new SqlCommand("EReceipt_Logs_ForSubmit_Billing", connection))
                {
                    try
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        // Add parameters if needed
                        command.Parameters.AddWithValue("@CountOfRecoreds", limit);

                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                EReceiptLogsForSubmit model = new EReceiptLogsForSubmit
                                {
                                    LogID = reader.GetInt64(reader.GetOrdinal("logID")),
                                    LogTypeID = reader.GetInt32(reader.GetOrdinal("LogTypeID")),
                                    EReceiptID = reader.GetInt64(reader.GetOrdinal("eReceiptID")),
                                    EReceiptNumber = reader.IsDBNull(reader.GetOrdinal("eReceiptNumber")) ? null : reader.GetString(reader.GetOrdinal("eReceiptNumber")),
                                    LogDatetime = reader.GetDateTime(reader.GetOrdinal("LogDatetime")),
                                    IsSendTosubmit = reader.GetBoolean(reader.GetOrdinal("IsSendTosubmit")),
                                    SubmitStatus = reader.GetByte(reader.GetOrdinal("SubmitStatus")),
                                    ReuestPayload = reader.IsDBNull(reader.GetOrdinal("ReuestPayload")) ? null : reader.GetString(reader.GetOrdinal("ReuestPayload")),
                                    ResponsePayload = reader.IsDBNull(reader.GetOrdinal("ResponsePayload")) ? null : reader.GetString(reader.GetOrdinal("ResponsePayload")),
                                    UUID = reader.IsDBNull(reader.GetOrdinal("UUID")) ? null : reader.GetString(reader.GetOrdinal("UUID")),
                                    QRCode = reader.IsDBNull(reader.GetOrdinal("QRCode")) ? null : reader.GetString(reader.GetOrdinal("QRCode")),
                                    ETARequestPayload = reader.IsDBNull(reader.GetOrdinal("ETARequestPayload")) ? null : reader.GetString(reader.GetOrdinal("ETARequestPayload")),
                                    ETAResponsePayload = reader.IsDBNull(reader.GetOrdinal("ETAResponsePayload")) ? null : reader.GetString(reader.GetOrdinal("ETAResponsePayload")),
                                    ETAQRCode = reader.IsDBNull(reader.GetOrdinal("ETAQRCode")) ? null : reader.GetString(reader.GetOrdinal("ETAQRCode")),
                                    ETAUUID = reader.IsDBNull(reader.GetOrdinal("ETAUUID")) ? null : reader.GetString(reader.GetOrdinal("ETAUUID")),
                                    Error = reader.IsDBNull(reader.GetOrdinal("Error")) ? null : reader.GetString(reader.GetOrdinal("Error")),
                                    EReceiptSource = reader.GetByte(reader.GetOrdinal("EReceiptSource")),
                                    EReceiptStatus = reader.IsDBNull(reader.GetOrdinal("EReceiptStatus")) ? null : reader.GetString(reader.GetOrdinal("EReceiptStatus"))
                                };
                                models.Add(model);
                            }
                        }
                        System.Console.WriteLine($"GetEReceiptLogsAsync retrieved {models.Count} records.");
                    }
                    catch (Exception ex)
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            connection.Close();
                        }
                        System.Console.WriteLine($"GetEReceiptLogsAsync SQL ERROR: " + ex.ToString());
                        // Log exception or handle as needed
                    }
                }
            }
            return models;
        }

        public async Task<bool> ETALogsAsync(long? LogID, long? EReceiptID, string ETAUUID, string ETAQRCode, string ETAResponsePayload, string Error)
        {
            if (DataType.IsNullOrEmptyOrWhiteSpace(Error))
                Error = "";

            using (SqlConnection connection = new SqlConnection(AppSettings.connectionStr))
            {
                using (SqlCommand command = new SqlCommand("EReceipt_Logs_Submitted_Billing", connection))
                {
                    try
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        // Add parameters if needed 
                        command.Parameters.AddWithValue("@LogID", LogID);
                        command.Parameters.AddWithValue("@EReceiptID", EReceiptID);
                        command.Parameters.AddWithValue("@ETAUUID", ETAUUID);
                        command.Parameters.AddWithValue("@ETAQRCode", ETAQRCode);
                        command.Parameters.AddWithValue("@ETAResponsePayload", ETAResponsePayload);
                        command.Parameters.AddWithValue("@Error", Error);

                        await connection.OpenAsync();
                        int rows = await command.ExecuteNonQueryAsync();
                        System.Console.WriteLine($"ETALogsAsync executed. LogID={LogID}, EReceiptID={EReceiptID}, Error={Error}, UUID={ETAUUID}, RowsAffected={rows}");
                        connection.Close();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            connection.Close();
                        }
                        System.Console.WriteLine($"ETALogsAsync EXCEPTION: " + ex.ToString());
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the total count of accepted receipts (SubmitStatus = 2 AND EReceiptSource = 2)
        /// </summary>
        public async Task<int> GetAcceptedCountAsync()
        {
            using (SqlConnection connection = new SqlConnection(AppSettings.connectionStr))
            {
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM EReceipt_Logs_Billing WHERE SubmitStatus = 2 AND EReceiptSource = 2", connection))
                {
                    try
                    {
                        await connection.OpenAsync();
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the total count of failed receipts (SubmitStatus NOT IN (1, 2) AND EReceiptSource = 2)
        /// </summary>
        public async Task<int> GetFailedCountAsync()
        {
            using (SqlConnection connection = new SqlConnection(AppSettings.connectionStr))
            {
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM EReceipt_Logs_Billing WHERE SubmitStatus NOT IN (1, 2) AND EReceiptSource = 2", connection))
                {
                    try
                    {
                        await connection.OpenAsync();
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }
            }
        }
        //public void UpdateReceipt(UpdateReceiptDTO model)
        //{


        //    using (SqlConnection connection = new SqlConnection(AppSettings.connectionStr))
        //    {
        //        using (SqlCommand command = new SqlCommand("EReceipt_UpdateStatus", connection))
        //        {
        //            try
        //            {
        //                command.CommandType = CommandType.StoredProcedure;
        //                // Add parameters if needed
        //                command.Parameters.AddWithValue("@LogID", model.logID);
        //                command.Parameters.AddWithValue("@EReceiptID", model.eReceiptID);
        //                command.Parameters.AddWithValue("@status", model.status);
        //                command.Parameters.AddWithValue("@uuid", model.uuid);

        //                connection.Open();
        //                command.ExecuteNonQuery();
        //                connection.Close();
        //            }
        //            catch (Exception ex)
        //            {
        //                if (connection.State == ConnectionState.Open)
        //                {
        //                    connection.Close();
        //                }
        //               // return (ex.Message.ToString());
        //            }
        //        }
        //    }
        //}

        //public async Task<GenericApiModel<EReceiptModel>> GetReceipt(int limit)
        //{
        //    HttpResponseMessage response = ApiTokenRepository.GetAPI().GetResponse($"v1/Receipt/Get/{limit}");
        //    ResponseResolver<EReceiptModel> responseResolver = new ResponseResolver<EReceiptModel>();
        //    return await responseResolver.ResolveResponse(response);
        //}

        //public async Task<PostApiModel<object>> UpdateReceipt(UpdateReceiptDTO model)
        //{
        //    HttpResponseMessage response = ApiTokenRepository.GetAPI().PostResponse("v1/Receipt/Erecipts", model);
        //    ResponseResolver<object> responseResolver = new ResponseResolver<object>();
        //    return await responseResolver.ResolvePostResponse(response);
        //}

    }
}

