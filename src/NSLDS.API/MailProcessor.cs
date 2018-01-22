using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.IO;
using Global.Domain;
using NSLDS.Domain;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace NSLDS.API
{
    public class EmailRecord
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string NSLDS_Role { get; set; }
    }

    // This scoped DI service class will process NSLDS sendmail requests
    public class MailProcessor
    {
        #region Private fields

        private IConfiguration _config;
        private IRuntimeOptions _runtimeOptions;
        private ClaimsPrincipal _claimsPrincipal;
        private GlobalContext _context;
        private ClientProfile _clientProfile;
        private EmailRecord _sender
        {
            get
            {
                var claims = _claimsPrincipal.Claims; return new EmailRecord
                {
                    FirstName = claims.FirstOrDefault(x => x.Type.Contains("givenname"))?.Value,
                    LastName = claims.FirstOrDefault(x => x.Type.Contains("surname"))?.Value,
                    Email = claims.FirstOrDefault(x => x.Type.Contains("emailaddress"))?.Value
                };
            }
        }

        #endregion

        #region Constructors

        public MailProcessor(IConfiguration config, IRuntimeOptions runtimeOptions)
        {
            _config = config;
            _runtimeOptions = runtimeOptions;
        }

        #endregion

        #region Public properties & methods

        public void Initialize(ClaimsPrincipal claimsPrincipal, ClientProfile clientProfile, GlobalContext context)
        {
            _claimsPrincipal = claimsPrincipal;
            _context = context;
            _clientProfile = clientProfile;
        }

        public List<string> SendInvites(IEnumerable<EmailRecord> emails)
        {
            var result = new List<string>();

            foreach (var item in emails)
                try
                {
                    var newid = Guid.NewGuid();
                    var userinvite = new UserInvite
                    {
                        Id = newid,
                        OpeId = _clientProfile.OPEID,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        UserEmail = item.Email,
                        SenderName = $"{_sender.FirstName} {_sender.LastName}",
                        SenderEmail = _sender.Email,
                        NSLDS_Role = item.NSLDS_Role,
                        ExpireOn = DateTime.Now.AddDays(_config.GetValue<int>("MailSettings:Invite:Expiry"))
                    };

                    bool success = SendInvite(item, newid);

                    // only add userinvite if sendmail succeeds
                    _context.UserInvites.Add(userinvite);
                    _context.SaveChanges();

                    result.Add($"Invitation sent to {item.Email}");
                }
                catch (Exception ex)
                {
                    result.Add($"{item.Email}: {ex.Message}");
                }

            return result;
        }

        public bool SendInvite(EmailRecord email, Guid newid)
        {
            var success = false;

            using (var msg = new MailMessage())
            using (var smtp = new SmtpClient())
            {
                var toName = $"{email.FirstName} {email.LastName}";
                var fromName = $"{_sender.FirstName} {_sender.LastName}";
                msg.IsBodyHtml = true;
                msg.To.Add(new MailAddress(email.Email, toName));
                msg.From = new MailAddress(_sender.Email, fromName);
                //msg.ReplyToList.Add(new MailAddress(_config["MailSettings:From"]));
                msg.Subject = _config["MailSettings:Invite:Subject"];
                var link = string.Format(_config["MailSettings:Invite:Link"], newid);
                var template = _config["MailSettings:Invite:Template"];
                using (var sr = new StreamReader(template))
                {
                    var mailbody = sr.ReadToEnd();
                    msg.Body = mailbody
                        .Replace("{invite_sender}", fromName)
                        .Replace("{invite_org}", _clientProfile.Organization_Name)
                        .Replace("{invite_days}", _config["MailSettings:Invite:Expiry"])
                        .Replace("{invite_link}", link);
                }
                smtp.Host = _config["MailSettings:Server"];
                smtp.Port = _config.GetValue<int>("MailSettings:Port");
                smtp.Send(msg);
                success = true;
            }
            return success;
        }

        public bool SendConfirmation(UserInvite invite)
        {
            using (var msg = new MailMessage())
            using (var smtp = new SmtpClient())
            {
                msg.IsBodyHtml = true;
                msg.To.Add(new MailAddress(invite.SenderEmail, invite.SenderName));
                msg.From = new MailAddress(_config["MailSettings:From"]);
                msg.Subject = _config["MailSettings:Invite:Confirm:Subject"];
                //var mailbody = _config.Get<string[]>("MailSettings:Invite:Confirm:Body");
                var template = _config["MailSettings:Invite:Confirm:Template"];
                using (var sr = new StreamReader(template))
                {
                    var mailbody = sr.ReadToEnd();
                    msg.Body = mailbody
                    .Replace("{confirm_name}", $"{invite.FirstName} {invite.LastName}");
                }
                smtp.Host = _config["MailSettings:Server"];
                smtp.Port = _config.GetValue<int>("MailSettings:Port");
                try
                {
                    smtp.Send(msg);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion
    }
}
