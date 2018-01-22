using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSLDS.Domain;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Data.OleDb;
using System.Data;
using System.Globalization;
using NSLDS.Common;
using Microsoft.AspNetCore.Http;

namespace NSLDS.API
{
    // This scoped DI service class will process NSLDS uploaded files
    // save them to a repository folder & invoke the proper file format processor class
    public class FileImportProcessor
    {
        #region Private fields

        private IConfiguration _config;
        private IRuntimeOptions _runtimeOptions;
        private string[] _validFormats = { FileTypeConstant.Xlsx, FileTypeConstant.Xls };
        private string[] _dateFormats = 
            { "MM/dd/yyyy", "MM/dd/yy", "MM-dd-yyyy", "MM-dd-yy", "MMddyyyy", "MMddyy", "yyyy-MM-dd", "yyyy/MM/dd" };
        private string _fileName;
        private string _connectionString;
        private string _command;

        #endregion

        #region Protected properties

        #endregion

        #region Public properties

        public string FileName { get; private set; }
        public string ErrorMessage { get; set; }
        public HashSet<ClientRequestStudent> StudentRequests { get; private set; }

        #endregion

        #region Constructors

        public FileImportProcessor(IConfiguration config, IRuntimeOptions runtimeOptions)
        {
            _config = config;
            _runtimeOptions = runtimeOptions;
            StudentRequests = new HashSet<ClientRequestStudent>();
        }

        #endregion

        #region Public Methods

        public bool Process(ClaimsPrincipal cp, IFormFile file)
        {
            FileName = string.Empty;
            ErrorMessage = string.Empty;

            var fileName = ContentDispositionHeaderValue
                .Parse(file.ContentDisposition).FileName.Trim('"');

            FileName = fileName;

            if (file.Length == 0)
            {
                ErrorMessage = ErrorConstant.InvalidFile;
                return false;
            }

            var fileExt = Path.GetExtension(fileName).ToLower();

            if (!_validFormats.Contains(fileExt, StringComparer.InvariantCultureIgnoreCase))
            {
                ErrorMessage = ErrorConstant.FormatNotRecognized;
                return false;
            }

            // we need these to build the upload path
            var OpeId = _runtimeOptions.GetTenantId(cp);
            var UserName = _runtimeOptions.GetUserName(cp);
            var RootPath = _config["AppSettings:UploadRootFolder"];
            var Date = DateTime.Today.ToString("yyyy-MM-dd");
            string[] FullPath = { RootPath, OpeId, UserName, Date };

            var path = Path.Combine(FullPath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fName = Path.GetFileNameWithoutExtension(fileName);
            var Time = DateTime.Now.ToString("HH_mm_ss");
            string[] fileParts = { fName, "_", Time, fileExt };

            _fileName = Path.Combine(path, string.Concat(fileParts));

            // file.SaveAs(_fileName); *superceded
            using (var stream = new FileStream(_fileName, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // process the file data based on the extension
            switch (fileExt)
            {
                case FileTypeConstant.Xlsx:
                    _connectionString = string.Format(_config["Data:Excel:ConnString12"], _fileName);
                    _command = _config["Data:Excel:CmdClientImport"];
                    break;
                case FileTypeConstant.Xls:
                    _connectionString = string.Format(_config["Data:Excel:ConnString8"], _fileName);
                    _command = _config["Data:Excel:CmdClientImport"];
                    break;
                default:
                    ErrorMessage = ErrorConstant.FormatNotRecognized;
                    return false;
            }

            bool result = ImportData();
            return result;
        }

        #endregion

        #region Private methods

        private bool ImportData()
        {
            StudentRequests.Clear();
            var success = false;
            try
            {
                using (OleDbConnection aConnection = new OleDbConnection(_connectionString))
                using (OleDbDataAdapter aAdapter = new OleDbDataAdapter(_command, aConnection))
                using (DataTable aTable = new DataTable())
                {
                    aAdapter.Fill(aTable);
                    // first row contains header text, rename columns to header name
                    var headrow = aTable.Rows[0];
                    foreach (DataColumn col in aTable.Columns)
                    {
                        col.ColumnName = headrow[col].ToString();
                    }
                    // now remove the first header row
                    aTable.Rows.Remove(headrow);

                    foreach (DataRow aRow in aTable.Rows)
                    {
                        // datachk-174: remove rows where SSN is not valid
                        int _ssn = 0;
                        if (!int.TryParse(aRow["SSN"].ToString(), out _ssn)) { continue; }

                        double _days;
                        DateTime _dob, _startDate, _enrollBeginDate, _monitorBeginDate;
                        DateTime? dob = null, startDate = null, enrollBeginDate = null, monitorBeginDate = null;

                        if (DateTime.TryParse(aRow["DOB"].ToString(), out _dob) ||
                            DateTime.TryParseExact(aRow["DOB"].ToString(), _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _dob))
                        { dob = _dob; }
                        else if (double.TryParse(aRow["DOB"].ToString(), out _days))
                        { dob = DateTime.FromOADate(_days); }

                        if (DateTime.TryParse(aRow["STARTDATE"].ToString(), out _startDate) ||
                            DateTime.TryParseExact(aRow["STARTDATE"].ToString(), _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _startDate))
                        { startDate = _startDate; }
                        else if (double.TryParse(aRow["STARTDATE"].ToString(), out _days))
                        { startDate = DateTime.FromOADate(_days); }

                        if (DateTime.TryParse(aRow["ENROLLBEGINDATE"].ToString(), out _enrollBeginDate) ||
                            DateTime.TryParseExact(aRow["ENROLLBEGINDATE"].ToString(), _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _enrollBeginDate))
                        { enrollBeginDate = _enrollBeginDate; }
                        else if (double.TryParse(aRow["ENROLLBEGINDATE"].ToString(), out _days))
                        { enrollBeginDate = DateTime.FromOADate(_days); }

                        if (DateTime.TryParse(aRow["MONITORBEGINDATE"].ToString(), out _monitorBeginDate) ||
                            DateTime.TryParseExact(aRow["MONITORBEGINDATE"].ToString(), _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _monitorBeginDate))
                        { monitorBeginDate = _monitorBeginDate; }
                        else if (double.TryParse(aRow["MONITORBEGINDATE"].ToString(), out _days))
                        { monitorBeginDate = DateTime.FromOADate(_days); }

                        if (!string.IsNullOrEmpty(aRow["SSN"].ToString()))
                        StudentRequests.Add(
                            new ClientRequestStudent
                            {
                                SSN = aRow["SSN"].ToString().PadLeft(9, '0'),
                                FirstName = aRow["FIRSTNAME"].ToString(),
                                LastName = aRow["LASTNAME"].ToString(),
                                DOB = dob,
                                SID = aRow["SID"].ToString(),
                                StartDate = startDate,
                                RequestType = aRow["REQUESTTYPE"].ToString(),
                                EnrollBeginDate = enrollBeginDate,
                                MonitorBeginDate = monitorBeginDate,
                                DeleteMonitoring = aRow["DELETEMONITORING"].ToString()
                            });
                    }

                    if (StudentRequests.Count > 0)
                    { success = true; }
                    else
                    { ErrorMessage = ErrorConstant.FileHasNoValidRecords; }
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
