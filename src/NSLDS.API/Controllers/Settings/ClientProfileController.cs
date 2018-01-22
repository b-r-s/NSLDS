using System;
using System.Linq;
using Global.Domain;
using NSLDS.Domain;
using NSLDS.Common;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace NSLDS.API.Controllers
{
  //  [Authorize(Policy = "Editor")]
    [Produces("application/json")]
    [Route("api/Settings/ClientProfile")]
    public class ClientProfileController : DbContextController
    {
		private GlobalContext context;
		#region Private Methods

		private string EncodePassword(string password)
        {
            try
            {
                byte[] encData_byte = new byte[password.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch
            {
                return null;
            }
        }
        
        private string DecodePassword(string encodedData)
        {
            try
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();
                byte[] todecode_byte = Convert.FromBase64String(encodedData);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            catch
            {
                return null;
            }
        }

        private DateTime? GetLastPwdChange(ClientProfile clientProfile)
        {
            var cpHistory = NsldsContext.ClientProfiles_History
                .Where(x => x.Link2ClientProfile == clientProfile.Id).ToList();

            if (cpHistory.Count == 0)
            {
                if (string.IsNullOrEmpty(clientProfile.TD_Password)) { return null; }
                else { return clientProfile.RevOn; }
            }

            DateTime? lastPwdChange = cpHistory
                .LastOrDefault(x => DecodePassword(x.TD_Password) != clientProfile.TD_Password)?.ActionOn;

            if (!lastPwdChange.HasValue)
            {
                lastPwdChange = cpHistory.First().ActionOn;
            }

            return lastPwdChange;
        }

        private bool ClientProfileExists(int id)
        {
            return NsldsContext.ClientProfiles.Count(e => e.Id == id) > 0;
        }

        private bool ClientProfileExists(string opeId)
        {
            return NsldsContext.ClientProfiles.Count(e => e.OPEID == opeId) > 0;
        }

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                NsldsContext.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Constructor

        public ClientProfileController(GlobalContext globalContext, IHostingEnvironment hostingEnvironment, IConfiguration configuration, IRuntimeOptions runtimeOptions, ILogger<ClientProfileController> logger) 
            : base(globalContext, hostingEnvironment, configuration, runtimeOptions, logger)
        {
        }


		#endregion

		#region Public Methods

		/// <summary>
		/// Retrieve current user's Client profile
		/// </summary>
		/// <returns>ClientProfileResult</returns>
		// GET: api/settings/ClientProfile
		[HttpGet]
        public IActionResult GetClientProfile()
        {
            ClientProfile clientProfile;
			// for testing only

            // check the ClientProfileId from the loggedin user claims
            if (ClientProfileId != 0)
            {
                clientProfile = NsldsContext.ClientProfiles.Single(m => m.Id == ClientProfileId);
                clientProfile.TD_Password = DecodePassword(clientProfile.TD_Password);
                clientProfile.LastPwdChanged = GetLastPwdChange(clientProfile);
            }
            else
            {
                // datachk-159: Retention period defaults to 365 days
                // return empty client profile populated with OPEID & default values
                clientProfile = new ClientProfile
                {
                    OPEID = OpeId,
                    RevBy = UserName,
                    Retention = 365,
                    Upload_Method = UploadMethod.Empty
                };
            }

            return Ok(new ClientProfileResult { Result = clientProfile });
        }

        /// <summary>
        /// Get profile of client by "id"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/settings/ClientProfile/4
        //[HttpGet("{id}", Name = "GetClientProfileById")]
        //public IActionResult GetClientProfileById([FromRoute] int id)
        //{
        //    //if (!ModelState.IsValid)
        //    //{
        //    //    return BadRequest(ModelState);
        //    //}

        //    if (ClientProfileExists(id))
        //    {
        //        ClientProfile clientProfile = NsldsContext.ClientProfiles.Single(m => m.Id == id);
        //        return Ok(new ClientProfileResult { Result = clientProfile });
        //        // return new JsonResult(clientProfile);
        //    } else { 
        //        return NotFound(new ErrorResult { Message = ErrorConstant.ClientProfileNotFound });
        //    }
        //}

        /// <summary>
        /// Save new client profile
        /// </summary>
        /// <param name="clientProfile"></param>
        /// <returns></returns>
        // POST: api/settings/ClientProfile
      //  [Authorize(Policy = "Administrator")]
        [HttpPost]
        public IActionResult PostClientProfile([FromBody] ClientProfile clientProfile)
        {
            // don't create a client profile if there is already one for this tenantId
            if (ClientProfileId != 0)
            {
                return BadRequest(new ErrorResult { Message = ErrorConstant.ClientProfileAlreadyExists });
            }

            // ensure OPEID hasn't been modified
            if (clientProfile.OPEID != OpeId)
            {
                clientProfile.OPEID = OpeId;
            }

            // datachk-168: password validation change, length >= 8
            var newpwd = clientProfile.TD_Password;
            if (newpwd != null && newpwd.Length >= 8) { clientProfile.IsPwdValid = true; }
            clientProfile.TD_Password = EncodePassword(newpwd);

            // nslds-110 validate upload method
            string[] validupload = { UploadMethod.TdClient, UploadMethod.EdConnect, UploadMethod.TdGlobal, UploadMethod.TdManual, UploadMethod.Empty };
            if (!validupload.Contains(clientProfile.Upload_Method))
            {
                clientProfile.Upload_Method = UploadMethod.Empty;
            }

            clientProfile.RevOn = DateTime.Now;
            clientProfile.RevBy = this.UserName;

            NsldsContext.ClientProfiles.Add(clientProfile);

            NsldsContext.SaveChanges();

            // nslds-115: set the tenant record to active for this opeid
            var tenant = GlobalContext.Tenants.Single(t => t.TenantId == clientProfile.OPEID);
            if (!tenant.IsActive)
            {
                tenant.IsActive = true;
                GlobalContext.Entry(tenant).State = EntityState.Modified;
                GlobalContext.SaveChanges();
            }
            // decrypt TD mailbox password before sending back
            clientProfile.TD_Password = DecodePassword(clientProfile.TD_Password);
            // datachk-226: if TD password is not empty populate LastPwdChanged
            if (!string.IsNullOrEmpty(clientProfile.TD_Password))
            {
                clientProfile.LastPwdChanged = DateTime.Now;
            }
            var result = new ClientProfileResult { Result = clientProfile };
            return CreatedAtRoute(new { id = clientProfile.Id }, result);
        }

        /// <summary>
        /// Save modified client profile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // PUT: api/settings/ClientProfile/5
      //  [Authorize(Policy = "Administrator")]
        [HttpPut("{id}")]
        public IActionResult PutClientProfile([FromRoute] int id, [FromBody] ClientProfile clientProfile)
        {
            if (id != clientProfile.Id)
            {
                return BadRequest(new ErrorResult { Message = ErrorConstant.InvalidClientProfile });
            }

            // retrieve and archive previous record to _history table
            var oldRec = NsldsContext.ClientProfiles.SingleOrDefault(x => x.Id == id);
            if (oldRec == null)
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.ClientProfileNotFound });
            }
            // check for previous history record & detach the old record from context
            var exist = NsldsContext.ClientProfiles_History.Any(x => x.Link2ClientProfile == id);
            NsldsContext.Entry(oldRec).State = EntityState.Detached;

            // Initialize Automapper engine
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<ClientProfile, ClientProfile_History>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Link2ClientProfile, opt => opt.MapFrom(src => src.Id));
            });
            // Convert old to history record
            var hisRec = AutoMapper.Mapper.Map<ClientProfile, ClientProfile_History>(oldRec);
            hisRec.Action = (exist) ? "Updated" : "New";
            hisRec.ActionOn = DateTime.Now;
            hisRec.ActionBy = UserName;
            NsldsContext.ClientProfiles_History.Add(hisRec);

            // Update client profile record
            // check old password & validate new password if it has been changed
            // datachk-168: password validation change, length >= 8
            var oldpwd = DecodePassword(oldRec.TD_Password);
            var newpwd = clientProfile.TD_Password;
            if (newpwd == null || newpwd.Length < 8) { clientProfile.IsPwdValid = false; }
            else if (newpwd != oldpwd) { clientProfile.IsPwdValid = true; }
            clientProfile.TD_Password = EncodePassword(newpwd);

            // nslds-110 validate upload method
            string[] validupload = { UploadMethod.TdClient, UploadMethod.EdConnect, UploadMethod.TdGlobal, UploadMethod.TdManual, UploadMethod.Empty };
            if (!validupload.Contains(clientProfile.Upload_Method))
            {
                clientProfile.Upload_Method = UploadMethod.Empty;
            }

            // ensure OPEID hasn't been modified
            if (clientProfile.OPEID != OpeId)
            {
                clientProfile.OPEID = OpeId;
            }

            clientProfile.RevOn = DateTime.Now;
            clientProfile.RevBy = this.UserName;

            NsldsContext.Entry(clientProfile).State = EntityState.Modified;

            NsldsContext.SaveChanges();

            clientProfile.TD_Password = DecodePassword(clientProfile.TD_Password);
            // datachk-226: if TD password was changed populate LastPwdChanged
            if (newpwd != oldpwd)
            {
                clientProfile.LastPwdChanged = DateTime.Now;
            }
            var result = new ClientProfileResult { Result = clientProfile };

            return Ok(result);
        }

        /// <summary>
        /// Delete client profile by "id"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       // [Authorize(Policy = "Administrator")]
        // DELETE: api/settings/ClientProfile/4
        [HttpDelete("{id}")]
        public IActionResult DeleteClientProfile([FromRoute] int id)
        {
            if (ClientProfileExists(id))
            {
                ClientProfile clientProfile = NsldsContext.ClientProfiles.Single(m => m.Id == id);
                NsldsContext.ClientProfiles.Remove(clientProfile);
                NsldsContext.SaveChanges();

                return new StatusCodeResult(StatusCodes.Status204NoContent);
            }
            else
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.ClientProfileNotFound });
            }
        }

        /// <summary>
        /// Get profile history of client by "id"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/settings/ClientProfile/History
      //  [Authorize(Policy = "Administrator")]
        [HttpGet("History", Name = "GetClientProfileHistory")]
        public IActionResult GetClientProfileHistory()
        {
            if (ClientProfileExists(ClientProfileId))
            {
                var history = NsldsContext.ClientProfiles_History
                    .Where(m => m.Link2ClientProfile == ClientProfileId)
                    .OrderByDescending(x => x.ActionOn);
                foreach (var item in history)
                {
                    item.TD_Password = DecodePassword(item.TD_Password);
                }
                return Ok(history);
            }
            else
            {
                return NotFound(new ErrorResult { Message = ErrorConstant.ClientProfileNotFound });
            }
        }

        #endregion
    }
}