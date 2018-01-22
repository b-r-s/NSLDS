using Global.Domain;
using NSLDS.Domain;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace NSLDS.API.Controllers
{
    [ServiceFilter(typeof(CustomExceptionFilter))]
    [ModelStateValidationFilter]
  //  [Authorize(Policy = "DataCheck")]
    public abstract class DbContextController : Controller
    {
        #region Private Fields

        private IRuntimeOptions _runtimeOptions = null;
        private NSLDS_Context _nsldsContext;
        private ILogger _logger;
        private int? _cp = null;

        #endregion

        #region Protected Fields

        protected ILogger Logger { get { return _logger; } }
        protected GlobalContext GlobalContext { get; set; }
        protected IHostingEnvironment HostingEnvironment { get; set; }
        protected IConfiguration Configuration { get; set; }
        protected string UserName { get { return _runtimeOptions.GetUserName(User); } }
        protected string OpeId { get { return _runtimeOptions.GetTenantId(User); } }
        protected int ClientProfileId
        {
            get
            {
                if (_cp == null || _cp == 0)
                {
                    _cp = NsldsContext.ClientProfiles
                        .Where(c => c.OPEID == OpeId)
                        .Select(cp => cp.Id)
                        .FirstOrDefault();
                }
                return _cp ?? 0;
            }
        }

        protected NSLDS_Context NsldsContext
        {
            get
            {
                if (_nsldsContext == null)
                {
                    var dbContextOptions = _runtimeOptions.GetDbContextOptions(this.User, this.GlobalContext);
                    _nsldsContext = new NSLDS_Context(dbContextOptions);
                    // workaround for setting transaction isolation level
                    //_nsldsContext.Database.OpenConnection();
                    //_nsldsContext.Database.ExecuteSqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");
                }

                return _nsldsContext;
            }
        }

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }

        #endregion

        #region Public Fields


        #endregion

        #region Constructors

        public DbContextController(GlobalContext globalContext, IHostingEnvironment hostingEnvironment, IConfiguration configuration,  IRuntimeOptions runtimeOptions, ILogger<Controller> logger)
        {
            GlobalContext = globalContext;
            HostingEnvironment = hostingEnvironment;
            Configuration = configuration;
            _runtimeOptions = runtimeOptions;
            _logger = logger;
        }

        #endregion
    }
}