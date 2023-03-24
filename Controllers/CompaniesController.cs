using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MeteorStrike.Data;
using MeteorStrike.Models.ViewModels;
using MeteorStrike.Models;
using MeteorStrike.Extentions;
using Microsoft.AspNetCore.Identity;
using MeteorStrike.Services.Interfaces;
using MeteorStrike.Services;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Authorization;

namespace MeteorStrike.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _btRolesService;
        private readonly IBTCompanyService _btCompanyService;

        public CompaniesController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IBTRolesService btRolesService,
                                  IBTCompanyService btCompanyService)
        {
            _context = context;
            _userManager = userManager;
            _btRolesService = btRolesService;
            _btCompanyService = btCompanyService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            // 1 Add an instance of the ViewModel as a list
            List<ManageUserRolesViewModel> viewModelList = new List<ManageUserRolesViewModel>();
            
            // 2 Get CompanyId
            int companyId = User.Identity!.GetCompanyId();

            // 3 Get all company users
            IEnumerable<BTUser> members = await _btCompanyService.GetMembersAsync(companyId);

            //4 Loop over the users to populate an instance of the ViewModel
            foreach (BTUser member in members)
            {
                IEnumerable<string> currentRoles = await _btRolesService.GetUserRolesAsync(member);

                // - instantiate single ViewModel
                ManageUserRolesViewModel viewModel = new()
                {
                    BTUser = member,
                    // - use _roleService to help populate the viewmodel instance 
                    // - Create multiselect
                    Roles = new MultiSelectList(await _btRolesService.GetRolesAsync(), "Name", "Name", currentRoles)
                };
                // - viewModel to model list
                viewModelList.Add(viewModel);
            }

            //5 Return the model to the view
            return View(viewModelList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {
            // 1 Get the company Id
            int companyId = User.Identity!.GetCompanyId();

            // 2 Instantiate the BTUser
            BTUser? btUser = await _context.Users.FindAsync(viewModel.BTUser.Id);

            // 3 Get Roles for the User
            IEnumerable<string> currentRoles = await _btRolesService.GetUserRolesAsync(btUser);

            // 4 Get Selected Role(s) for the User submitted from the form
            IEnumerable<string> selectedRoles = viewModel.SelectedRoles.ToList();

            // 5 Remove current role(s) and Add new role
            await _btRolesService.RemoveUserFromRolesAsync(btUser, currentRoles);

            foreach (string role in selectedRoles)
            {
                await _btRolesService.AddUserToRoleAsync(btUser, role);
            }

            await _context.SaveChangesAsync();  

            // 6 Navigate
            return RedirectToAction("ManageUserRoles");


        }
    //    if (viewModel.SelectedMembers != null) 
    //        {
    //            //Remove current members
    //            await _btProjectService.RemoveMembersFromProjectAsync(viewModel.Project!.Id, companyId);

    //    //Add newly selected members
    //    await _btProjectService.AddMembersToProjectAsync(viewModel.SelectedMembers, viewModel.Project!.Id, companyId);

    //            return RedirectToAction(nameof(Details), new { id = viewModel.Project!.Id
    //});
    //        }





// GET: Companies
        public async Task<IActionResult> Index()
        {
              return _context.Companies != null ? 
                          View(await _context.Companies.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
          return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
