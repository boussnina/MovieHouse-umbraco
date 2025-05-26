using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using MovieHouse.Data;
using MovieHouse.Models;
using System.Linq;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.Iana;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Security;

namespace MovieHouse.Controllers
{
    public class AdminPageController : RenderController
    {
        private readonly ILogger<AdminPageController> _logger;
        private readonly MovieDbContext _db;
        private readonly IMemberService _memberService;
        private readonly IMemberManager _memberManager;


        public AdminPageController(
            ILogger<AdminPageController> logger,
            ICompositeViewEngine viewEngine,
            IUmbracoContextAccessor contextAccessor,
            MovieDbContext db,
            IMemberService memberService,
            IMemberManager memberManager)
            : base(logger, viewEngine, contextAccessor)
        {
            _db = db;
            _logger = logger;
            _memberManager = memberManager;
            _memberService = memberService;
        }

        private bool IsAdminSync()
        //Hjælpefunktion der tjekker om brugeren er admin
        {
            if (!User.Identity.IsAuthenticated)
                return false;

            var memberIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(memberIdClaim, out int memberId))
            {
                var member = _memberService.GetById(memberId);
                if (member != null)
                {
                    return _memberService.GetAllRoles(member.Id).Contains("Admin member");
                }
            }

            return false;
        }

        public override IActionResult Index()
        //Index funktionen der bliver kaldt når brugeren åbner AdminPage
        {
            //Kalder hjælpefunktion
            var isAdmin = IsAdminSync();    
            
            //Admin boolean - display logikken så kun admin har adgang håndteres i cshtml
            ViewData["IsAdmin"] = isAdmin;

            var search = HttpContext.Request.Query["search"].ToString();

            var ratings = _db.Ratings
                .Where(r => string.IsNullOrEmpty(search) || r.UserName.ToLower().Contains(search.ToLower()))
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            //Gemmer søgeresultaterne
            ViewData["Search"] = search;
            ViewData["Ratings"] = ratings;

            return CurrentTemplate(CurrentPage);
        }
    }
}
