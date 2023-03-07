﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MeteorStrike.Data;
using MeteorStrike.Models;
using DailyRoarBlog.Data;
using Microsoft.AspNetCore.Identity;
using MeteorStrike.Services;
using MeteorStrike.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using MeteorStrike.Extentions;
using X.PagedList;
using MeteorStrike.Models.ViewModels;
using MeteorStrike.Models.Enums;
using System.Collections;

namespace MeteorStrike.Controllers
{
    [Authorize]

    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _btProjectService;
        private readonly IBTRolesService _btRolesService;  

        public ProjectsController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IBTFileService fileService,
                                  IBTProjectService btProjectService,
                                  IBTRolesService btRolesService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _btProjectService = btProjectService;
            _btRolesService = btRolesService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        // GET: Assign Project Manager
        public async Task<IActionResult> AssignPM(int? id) 
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);

            BTUser? currentPM = await _btProjectService.GetProjectManagerAsync(id);

            AssignPMViewModel viewModel = new()
            {
                Project = await _btProjectService.GetProjectByIdAsync(id, companyId),
                PMList = new SelectList(projectManagers, "Id", "FullName",currentPM?.Id),
                PMId = currentPM?.Id
            };

            return View(viewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        //POST: Assign Project Manager
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel) 
        {
            if (!string.IsNullOrEmpty(viewModel.PMId)) 
            {
                await _btProjectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project?.Id);

                return RedirectToAction("Details", new { id = viewModel.Project?.Id});
            }

            ModelState.AddModelError("PMId", "No Project Manager selected. Please select a PM!");

            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _btProjectService.GetProjectManagerAsync(viewModel.Project?.Id);
            viewModel.Project = await _btProjectService.GetProjectByIdAsync(viewModel.Project?.Id, companyId);
            viewModel.PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id);
            viewModel.PMId= currentPM?.Id;

            return View(viewModel);

        }

        // GET: Projects
        public async Task<IActionResult> Index(int? pageNum)
        {
            int pageSize = 6; 
            int page = pageNum ?? 1; 

            int companyId = User.Identity!.GetCompanyId();

            IPagedList<Project> projects = (await _btProjectService.GetProjectsAsync(companyId)).ToPagedList(page, pageSize);

            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _btProjectService.GetProjectAsync(companyId, id.Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            IEnumerable<ProjectPriority> priorities = await _btProjectService.GetProjectPriorityAsync();

            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");

            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,ImageFormFile,Archived,CompanyId")] Project project)
        {
            ModelState.Remove("CompanyId");

            if (ModelState.IsValid)
            {   //Get CompanyId
                BTUser? btUser = await _userManager.GetUserAsync(User);

                project.CompanyId = btUser!.CompanyId;

                //Created Date
                project.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
                //Start Date
                project.StartDate = DataUtility.GetPostGresDate(project.StartDate);
                //EndDate
                project.EndDate = DataUtility.GetPostGresDate(project.EndDate);

                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                await _btProjectService.AddProjectAsync(project);

                return RedirectToAction(nameof(Index));
            }

            IEnumerable<ProjectPriority> priorities = await _btProjectService.GetProjectPriorityAsync();

            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");

            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            var project = await _btProjectService.GetProjectAsync(companyId, id.Value);

            if (project == null)
            {
                return NotFound();
            }

            IEnumerable<ProjectPriority> priorities = await _btProjectService.GetProjectPriorityAsync();

            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");

            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,ImageFormFile,Archived,CompanyId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //Created Date
                    project.Created = DataUtility.GetPostGresDate(project.Created);

                    //Start Date
                    project.StartDate = DataUtility.GetPostGresDate(project.StartDate);

                    //EndDate
                    project.EndDate = DataUtility.GetPostGresDate(project.EndDate);

                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                    await _btProjectService.UpdateProjectAsync(project);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_btProjectService.ProjectExists(project.Id))
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

            IEnumerable<ProjectPriority> priorities = await _btProjectService.GetProjectPriorityAsync();

            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");            
            
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                int companyId = User.Identity!.GetCompanyId();

                Project? project = await _btProjectService.GetProjectAsync(companyId, id.Value);

                await _btProjectService.ArchiveProjectAsync(project);

                return RedirectToAction(nameof(Index)); 
            }
            catch (Exception)
            {

                throw;
            }

        }

        // POST: Projects/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Projects == null)
        //    {
        //        return NotFound();
        //    }

        //    int companyId = User.Identity!.GetCompanyId();

        //    Project? project = await _btProjectService.GetProjectAsync(companyId, id);

        //    if (project != null)
        //    {
        //        project.Archived = true;
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
    }
}
