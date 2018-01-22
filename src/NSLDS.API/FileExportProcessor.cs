using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSLDS.Domain;
using System.Security.Claims;
using System.Data.OleDb;
using NSLDS.Common;
using Microsoft.AspNetCore.Hosting;

namespace NSLDS.API
{
    // This scoped DI service class will export batch requests to Excel files
    // and save them to a repository folder 
    public class FileExportProcessor
    {
        #region Private fields

        private IHostingEnvironment _env;
        private IConfiguration _config;
        private IRuntimeOptions _runtimeOptions;
        private string[] _validFormats = { FileTypeConstant.Xlsx, FileTypeConstant.Xls };
        private string _fileName;
        private string _connectionString;
        private string _command;
        #endregion

        #region Protected properties

        #endregion

        #region Public properties

        public string FileName { get; private set; }
        public string FileRoute { get; private set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<ClientRequestStudent> StudentRequests { get; set; }
        public List<LoanExportRow> LoanHistory { get; set; }

        #endregion

        #region Constructors

        public FileExportProcessor(IHostingEnvironment env, IConfiguration config, IRuntimeOptions runtimeOptions)
        {
            _env = env;
            _config = config;
            _runtimeOptions = runtimeOptions;
        }

        #endregion

        #region Public Methods

        public bool Process(ClaimsPrincipal cp, int batchId)
        {
            if (StudentRequests.Count() == 0)
            {
                ErrorMessage = ErrorConstant.BatchHasNoRecords;
                return false;
            }

            _fileName = string.Empty;
            ErrorMessage = string.Empty;

            // we need these to build the upload path
            var OpeId = _runtimeOptions.GetTenantId(cp);
            var UserName = _runtimeOptions.GetUserName(cp);
            var WebPath = _env.WebRootPath; // use the wwwroot folder
            var RootPath = _config["AppSettings:UploadRootFolder"];
            var Date = DateTime.Today.ToString("yyyy-MM-dd");
            string[] FullPath = { WebPath, OpeId, UserName, Date };

            var path = Path.Combine(FullPath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Excel export template to copy
            var exName = _config["Data:Excel:ExportTemplate"];
            var exPath = Path.Combine(RootPath, exName);
            // destination Excel filename
            var fName = string.Format("Export_{0}_", batchId);
            var Time = DateTime.Now.ToString("HH_mm_ss");
            string[] fileParts = { fName, Time, FileTypeConstant.Xlsx };

            _fileName = Path.Combine(path, string.Concat(fileParts));
            // create empty Excel export file
            File.Copy(exPath, _fileName);
            
            // Assign virtual filename
            FileName = string.Concat(fileParts);
            FileRoute = string.Format("/{0}/{1}/{2}/{3}", 
                OpeId, UserName, Date, FileName);

            // process the file data 
            bool result = ExportData();

            return result;
        }

        public bool ProcessLoans(ClaimsPrincipal cp, int batchId, int studentId)
        {
            _fileName = string.Empty;
            ErrorMessage = string.Empty;

            // we need these to build the upload path
            var OpeId = _runtimeOptions.GetTenantId(cp);
            var UserName = _runtimeOptions.GetUserName(cp);
            var WebPath = _env.WebRootPath; // use the wwwroot folder
            var RootPath = _config["AppSettings:UploadRootFolder"];
            var Date = DateTime.Today.ToString("yyyy-MM-dd");
            string[] FullPath = { WebPath, OpeId, UserName, Date };

            var path = Path.Combine(FullPath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Excel loan export template to copy
            var exName = _config["Data:Excel:LoanExportTemplate"];
            var exPath = Path.Combine(RootPath, exName);
            // destination Excel filename
            var exType = (studentId > 0) ? $"student_{studentId}" : $"batch_{batchId}";
            var fName = $"LoanHistory_{exType}_";
            var Time = DateTime.Now.ToString("HH_mm_ss");
            string[] fileParts = { fName, Time, FileTypeConstant.Xlsx };

            _fileName = Path.Combine(path, string.Concat(fileParts));
            // create empty Excel export file
            File.Copy(exPath, _fileName);

            // Assign virtual filename
            FileName = string.Concat(fileParts);
            FileRoute = string.Format("/{0}/{1}/{2}/{3}",
                OpeId, UserName, Date, FileName);

            // process the file data 
            bool result = ExportLoanHistory();

            return result;
        }

        #endregion

        #region Private methods

        private bool ExportData()
        {
            var success = false;
            _connectionString = string.Format(_config["Data:Excel:ConnStringEX"], _fileName);

            try
            {
                using (OleDbConnection aConnection = new OleDbConnection(_connectionString))
                using (OleDbCommand aCommand = new OleDbCommand())
                {
                    // Open the connection
                    aConnection.Open();
                    aCommand.Connection = aConnection;
                    try
                    {
                        // Loop through the StudentRequests
                        foreach (var item in StudentRequests)
                        {
                            _command = string.Format(_config["Data:Excel:CmdClientExport"],
                                item.SSN, item.FirstName.Replace("'", "''"), item.LastName.Replace("'", "''"), 
                                (item.DOB.HasValue) ? item.DOB.Value.ToShortDateString() : null,
                                item.SID,
                                (item.StartDate.HasValue) ? item.StartDate.Value.ToShortDateString() : null, 
                                item.RequestType,
                                (item.EnrollBeginDate.HasValue) ? item.EnrollBeginDate.Value.ToShortDateString() : null,
                                (item.MonitorBeginDate.HasValue) ? item.MonitorBeginDate.Value.ToShortDateString() : null, 
                                item.DeleteMonitoring);

                            aCommand.CommandText = _command;
                            aCommand.ExecuteNonQuery();

                        }
                    }
                    finally
                    {
                        aConnection.Close();
                    }

                    success = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return success;
        }

        private bool ExportLoanHistory()
        {
            var success = false;
            _connectionString = string.Format(_config["Data:Excel:ConnStringEX"], _fileName);

            try
            {
                using (OleDbConnection aConnection = new OleDbConnection(_connectionString))
                using (OleDbCommand aCommand = new OleDbCommand())
                {
                    // Open the connection
                    aConnection.Open();
                    aCommand.Connection = aConnection;
                    _command =
                        "INSERT INTO [Sheet1$] VALUES(@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15)";

                    aCommand.CommandText = _command;
                    aCommand.Parameters.Add("@1", OleDbType.Char);
                    aCommand.Parameters.Add("@2", OleDbType.Char);
                    aCommand.Parameters.Add("@3", OleDbType.Char);
                    aCommand.Parameters.Add("@4", OleDbType.Char);
                    aCommand.Parameters.Add("@5", OleDbType.Char);
                    aCommand.Parameters.Add("@6", OleDbType.Char);
                    aCommand.Parameters.Add("@7", OleDbType.Char);
                    aCommand.Parameters.Add("@8", OleDbType.Char);
                    aCommand.Parameters.Add("@9", OleDbType.Char);
                    aCommand.Parameters.Add("@10", OleDbType.Char);
                    aCommand.Parameters.Add("@11", OleDbType.Char);
                    aCommand.Parameters.Add("@12", OleDbType.Char);
                    aCommand.Parameters.Add("@13", OleDbType.Char);
                    aCommand.Parameters.Add("@14", OleDbType.Char);
                    aCommand.Parameters.Add("@15", OleDbType.Char);

                    try
                    {
                        // Loop through the LoanHistory
                        foreach (var item in LoanHistory)
                        {
                            aCommand.Parameters["@1"].Value = item.SSN ?? string.Empty;
                            aCommand.Parameters["@2"].Value = item.FirstName ?? string.Empty;
                            aCommand.Parameters["@3"].Value = item.LastName ?? string.Empty;
                            aCommand.Parameters["@4"].Value = item.LoanType ?? string.Empty;
                            aCommand.Parameters["@5"].Value = item.LoanStatus ?? string.Empty;
                            aCommand.Parameters["@6"].Value = item.LoanFlag ?? string.Empty;
                            aCommand.Parameters["@7"].Value = $"{item.LoanStatusDate:yyyy-MM-dd}" ?? (object)DBNull.Value;
                            aCommand.Parameters["@8"].Value = $"{item.LoanAmount:C}" ?? (object)DBNull.Value;
                            aCommand.Parameters["@9"].Value = $"{item.DisbursedAmount:C}" ?? (object)DBNull.Value;
                            aCommand.Parameters["@10"].Value = item.LoanDates ?? string.Empty;
                            aCommand.Parameters["@11"].Value = item.AYDates ?? string.Empty;
                            aCommand.Parameters["@12"].Value = item.AcademicLevel ?? string.Empty;
                            aCommand.Parameters["@13"].Value = item.School ?? string.Empty;
                            aCommand.Parameters["@14"].Value = item.OpenAY ?? string.Empty;
                            aCommand.Parameters["@15"].Value = $"{item.TotalRemain:C}" ?? (object)DBNull.Value;

                            aCommand.ExecuteNonQuery();
                        }
                    }
                    finally
                    {
                        aConnection.Close();
                    }

                    success = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return success;
        }

        #endregion
    }
}
