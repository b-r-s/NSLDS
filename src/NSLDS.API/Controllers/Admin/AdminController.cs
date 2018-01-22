using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Data;
using System.Data.OleDb;
using Microsoft.Net.Http.Headers;
using Global.Domain;
using NSLDS.Domain;
using System.Reflection;
using NSLDS.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NSLDS.API.Controllers
{
    /// <summary>
    /// Admin Controller
    /// </summary>
  //  [Authorize(Policy = "Administrator")]
    [Produces("application/json")]
    [Route("api/Admin")]
    public class AdminController : DbContextController
    {
        #region Private properties

        private ClientProfile _cp;

        private ClientProfile cp
        {
            get
            {
                if (_cp == null)
                {
                    _cp = NsldsContext.ClientProfiles.Single(x => x.Id == ClientProfileId);
                }
                return _cp;
            }
        }

        #endregion

        #region Constructor

        public AdminController(GlobalContext globalContext, IHostingEnvironment hostingEnvironment, IConfiguration configuration, IRuntimeOptions runtimeOptions, ILogger<AdminController> logger) 
            : base(globalContext, hostingEnvironment, configuration, runtimeOptions, logger)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks databases
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            var db = new
            {
                GlobalDb = GlobalContext.Database.GetDbConnection().Database,
                ClientDb = NsldsContext.Database.GetDbConnection().Database
            };

            return Ok(new MessageResult { Message = string.Format("Databases loaded: {0}", db) });
        }

        /// <summary>
        /// Apply database migrations
        /// </summary>
        /// <returns></returns>
        // GET: /<controller>/
        [Route("migrate")]
        [HttpGet]
        public IActionResult DbMigrate()
        {
            // update databases to latest migration
            //GlobalContext.Database.Migrate();
            NsldsContext.Database.Migrate();

            var db = new
            {
                GlobalDb = GlobalContext.Database.GetDbConnection().Database,
                ClientDb = NsldsContext.Database.GetDbConnection().Database
            };

            return Ok(new MessageResult { Message = string.Format("All database migrations have been applied to {0}", db) });
        }

        /// <summary>
        /// Invite users to register via email link
        /// </summary>
        /// <param name="emails"></param>
        /// <returns>200 OK</returns>
        [HttpPost("invite")]
        public IActionResult Invite([FromBody] IEnumerable<EmailRecord> emails, [FromServices] MailProcessor mailProcessor)
        {
            if (!ModelState.IsValid || emails == null || emails.Count() == 0)
            { return BadRequest(ModelState); }

            if (cp == null)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.ClientProfileNotFound });
            }

            mailProcessor.Initialize(User, cp, GlobalContext);

            var result = mailProcessor.SendInvites(emails);

            return Ok(result);
        }

        /// <summary>
        /// Retrieve all invitations and status - Admin only
        /// </summary>
        /// <returns></returns>
        [HttpGet("invite")]
        public IActionResult GetInvites()
        {
            var invites = GlobalContext.UserInvites.Where(u => u.OpeId == OpeId);

            return Ok(invites);
        }

        /// <summary>
        /// retrieve invitation detail from email link ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("invite/{id}")]
        public IActionResult GetInvite([FromRoute] Guid id)
        {
            var invite = GlobalContext.UserInvites.SingleOrDefault(u => u.Id == id);
            if (invite == null) { return NotFound(); }
            if (invite.HasRegistered)
            { return BadRequest(new ErrorResult { Message = ErrorConstant.InviteUsed }); }
            if (invite.ExpireOn < DateTime.Now)
            { return BadRequest(new ErrorResult { Message = ErrorConstant.InviteExpired }); }

            return Ok(invite);
        }

        /// <summary>
        /// delete invitation by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("invite/{id}")]
        public IActionResult DeleteInvite([FromRoute] Guid id)
        {
            var invite = GlobalContext.UserInvites.SingleOrDefault(u => u.Id == id);
            if (invite == null) { return NotFound(); }
            GlobalContext.Remove(invite);
            GlobalContext.SaveChanges();
            return Ok();
        }

        /// <summary>
        /// retrieve invitation detail from email link ID and resend invitation email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("reinvite/{id}")]
        public IActionResult ReInvite([FromRoute] Guid id, [FromServices] MailProcessor mailProcessor)
        {
            var invite = GlobalContext.UserInvites.SingleOrDefault(u => u.Id == id);
            if (invite == null) { return NotFound(); }

            var email = new EmailRecord
            {
                FirstName = invite.FirstName,
                LastName = invite.LastName,
                Email = invite.UserEmail,
                NSLDS_Role = invite.NSLDS_Role
            };

            string result;
            try
            {
                mailProcessor.Initialize(User, cp, GlobalContext);
                mailProcessor.SendInvite(email, invite.Id);
                result = $"Invitation re-sent to {email.Email}";
            }
            catch (Exception ex)
            {
                result = $"{email.Email}: {ex.Message}";
                return BadRequest(result);
            }

            // set new invite expiry
            var days = int.Parse(Configuration["MailSettings:Invite:Expiry"]);
            invite.ExpireOn = DateTime.Now.AddDays(days);
            GlobalContext.Entry(invite).State = EntityState.Modified;
            GlobalContext.SaveChanges();

            return Ok(result);
        }

        /// <summary>
        /// Confirm user has registered using the invitation link
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("invite/{id}")]
        public IActionResult PostInvite([FromRoute] Guid id, [FromServices] MailProcessor mailProcessor)
        {
            var invite = GlobalContext.UserInvites.SingleOrDefault(u => u.Id == id);
            if (invite == null) { return NotFound(); }
            if (invite.HasRegistered)
            { return BadRequest(new ErrorResult { Message = ErrorConstant.InviteUsed }); }

            mailProcessor.SendConfirmation(invite);

            // check invites for this email and remove them
            var invites = GlobalContext.UserInvites
                .Where(x => x.OpeId == invite.OpeId && x.UserEmail == invite.UserEmail).ToList();

            GlobalContext.RemoveRange(invites);

            GlobalContext.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Returns current API server version, date and time
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("version")]
        public IActionResult CheckServerVersion()
        {
            var currentTime = DateTime.Now.ToString();
            var currentName = Assembly.GetExecutingAssembly().GetName().Name;
            var projectVersion = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion;
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToSt‌​ring();
            var result = new
            {
                ProjectName = currentName,
                ProjectVersion = projectVersion,
                ProjectBuild = assemblyVersion,
                ServerTime = currentTime
            };

            return Ok(result);
        }

        #endregion

        #region Deprecated public methods

        /// <summary>
        /// Form for uploading. TEST ONLY
        /// </summary>
        /// <returns></returns>
        // test upload form
        [HttpGet("uploadFederalStudentCodes")]
        public IActionResult fsclistupload()
        {
            return View();
        }

        /// <summary>
        /// List of Federal School Codes
        /// </summary>
        /// <returns></returns>
        [Route("list")]
        [HttpGet]
        public IActionResult FSCList()
        {
            return View(GlobalContext.FedSchoolCodeList);
        }

        /// <summary>
        /// Upload Federal Student Codes from XLS file received from Department of Education
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("uploadFederalStudentCodes")]
        public async Task<IActionResult> fsclistupload(IFormFile file)
        {
            var path = Path.Combine(Configuration["AppSettings:UploadRootFolder"], "NSLDSUploads");   //Path.Combine(HostingEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fileName = ContentDispositionHeaderValue
                .Parse(file.ContentDisposition).FileName.Trim('"');

            if (file.Length == 0 || !fileName.EndsWith(FileTypeConstant.Xlsx))
            {
                return BadRequest(new ErrorResult { Message = "Invalid file: " + fileName });
            }
            var fileSaved = Path.Combine(path, fileName);

            // await file.SaveAsAsync(fileSaved);
            using (var stream = new FileStream(fileSaved, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // process the .xlsx data

            var xlConnectionString = string.Format(Configuration["Data:Excel:ConnString12"], fileSaved);
            var xlCommand = Configuration["Data:Excel:CmdFCSList"];

            using (OleDbConnection xlConnection = new OleDbConnection(xlConnectionString))
            using (OleDbDataAdapter xlAdapter = new OleDbDataAdapter(xlCommand, xlConnection))
            using (DataTable xlTable = new DataTable())
            {
                xlAdapter.Fill(xlTable);
                try
                {
                    GlobalContext.FedSchoolCodeList.RemoveRange(GlobalContext.FedSchoolCodeList);
                    GlobalContext.SaveChanges();

                    foreach (DataRow xlRow in xlTable.Rows)
                    {
                        GlobalContext.FedSchoolCodeList.Add(
                            new FedSchoolCodeList
                            {
                                SchoolCode = xlRow["SchoolCode"].ToString(),
                                SchoolName = xlRow["SchoolName"].ToString(),
                                Address = xlRow["Address"].ToString(),
                                City = xlRow["City"].ToString(),
                                ZipCode = xlRow["ZipCode"].ToString(),
                                StateCode = xlRow["StateCode"].ToString(),
                                Province = xlRow["Province"].ToString(),
                                PostalCode = xlRow["PostalCode"].ToString(),
                                Country = xlRow["Country"].ToString()
                            });
                    }
                    GlobalContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    return BadRequest(new ErrorResult { Message = ex.Message });
                }

                return Ok(new MessageResult
                {
                    Message = string.Format("{0} records were saved to the FederalSchoolCodeList table", xlTable.Rows.Count)
                });
            }
        }

        #endregion

    }
}
