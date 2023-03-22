using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MeteorStrike.Data;
using MeteorStrike.Models;
using MeteorStrike.Extentions;
using MeteorStrike.Services.Interfaces;
using MeteorStrike.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using DailyRoarBlog.Data;
using Microsoft.AspNetCore.Authorization;
using MailKit;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MeteorStrike.Controllers
{
    [Authorize(Roles = "Admin")]

    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _btProjectService;
        private readonly IBTCompanyService _btCompanyService;
        private readonly IEmailSender _btMailService;
        private readonly IBTInviteService _btInviteService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IDataProtector _protector;
        private readonly IConfiguration _configuration;

        public InvitesController(ApplicationDbContext context,
                                 IBTProjectService bTProjectService,
                                 IBTCompanyService btCompanyService,
                                 IEmailSender btEmailService,
                                 IBTInviteService bTInviteService,
                                 UserManager<BTUser> userManager,
                                 IDataProtectionProvider dataProtectionProvider,
                                 IConfiguration configuration)
        {
            _context = context;
            _btProjectService = bTProjectService;
            _btCompanyService = btCompanyService;
            _btMailService = btEmailService;
            _btInviteService = bTInviteService;
            _userManager = userManager;
            _configuration = configuration;
            _protector = dataProtectionProvider.CreateProtector(configuration.GetValue<string>("ProtectKey")! ?? Environment.GetEnvironmentVariable("ProtectKey")!);
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor).Include(i => i.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }
        [Authorize(Roles = "Admin")]
        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            int? companyId = User.Identity!.GetCompanyId();

            ViewData["ProjectId"] = new SelectList(await _btProjectService.GetProjectsAsync(companyId.Value), "Id", "Name");

            return View();
        }
        [Authorize(Roles = "Admin")]
        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,InviteeEmail,InviteeFirstName,InviteeLastName,Message")] Invite invite)
        {
            int companyId = User.Identity!.GetCompanyId();

            ModelState.Remove("InvitorId");

            if (ModelState.IsValid)
            {
                try
                {
                    Guid guid = Guid.NewGuid();

                    string? token = _protector.Protect(guid.ToString());
                    string? email = _protector.Protect(invite.InviteeEmail!);
                    string? company = _protector.Protect(companyId.ToString());

                    string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, protocol: Request.Scheme);

                    string body = $@"{invite.Message} <br />
                       Please join my Company. <br />
                       Click the following link to join our team. <br />
                       <a href=""{callbackUrl}"">COLLABORATE</a>";

                    string? destination = invite.InviteeEmail;

                    Company btCompany = await _btCompanyService.GetCompanyInfoAsync(companyId);

                    string? subject = $"AstraTracker : {btCompany.Name} Invite";

                    await _btMailService.SendEmailAsync(destination!, subject, body);

                    // Save Invite to Database
                    invite.CompanyToken = guid;
                    invite.CompanyId = companyId;
                    invite.InviteDate = DataUtility.GetPostGresDate(DateTime.Now);
                    invite.InvitorId = _userManager.GetUserId(User);
                    invite.IsValid= true;

                    await _btInviteService.AddNewInviteAsync(invite);

                    return RedirectToAction(nameof(Index), "Home");

                    //TODO Sweet Alert?
                }
                catch (Exception)
                {

                    throw;
                }

            }

            ViewData["ProjectId"] = new SelectList(await _btProjectService.GetProjectsAsync(companyId), "Id", "Name");
            return View(invite);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessInvite(string token, string email, string company)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(company)) 
            {
                return NotFound();
            }

            Guid? companyToken = Guid.Parse(_protector.Unprotect(token));
            string? inviteeEmail = _protector.Unprotect(email);
            int companyId = int.Parse(_protector.Unprotect(company));

            try
            {
                Invite? invite = await _btInviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

                if(invite != null)  
                { 
                    return View(invite);
                }

                return NotFound();
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}
