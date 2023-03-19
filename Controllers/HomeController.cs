using MeteorStrike.Extentions;
using MeteorStrike.Models;
using MeteorStrike.Models.Enums;
using MeteorStrike.Models.ViewModels;
using MeteorStrike.Services;
using MeteorStrike.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MeteorStrike.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _btProjectService;
        private readonly IBTCompanyService _btCompanyService;
        private readonly IBTTicketService _btTicketService;

        public HomeController(ILogger<HomeController> logger, 
                                      UserManager<BTUser> userManager, 
                                      IBTProjectService btProjectService, 
                                      IBTCompanyService bTCompanyService, 
                                      IBTTicketService btTicketService)
        {
            _logger = logger;
            _userManager = userManager;
            _btProjectService = btProjectService;
            _btCompanyService = bTCompanyService;
            _btTicketService = btTicketService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            //List of Tickets
            IEnumerable<Ticket> tickets = await _btTicketService.GetTicketsAsync(companyId);

            //List of Projects
            IEnumerable<Project> projects = await _btProjectService.GetProjectsAsync(companyId);

            //List of Developers
            List<BTUser> members = await _btCompanyService.GetMembersAsync(companyId);

            //Company
            Company company = await _btCompanyService.GetCompanyInfoAsync(companyId);

            DashboardViewModel viewModel = new()
            {
                Tickets = tickets.ToList(),
                Projects = projects.ToList(),
                Members = members.ToList(),
                Company = company,

            };

            return View(viewModel);
        }

        public IActionResult LandingPage()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}