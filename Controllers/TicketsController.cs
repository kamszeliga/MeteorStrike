using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MeteorStrike.Data;
using MeteorStrike.Models;
using Microsoft.AspNetCore.Identity;
using DailyRoarBlog.Data;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.Design;
using MeteorStrike.Services.Interfaces;
using MeteorStrike.Extentions;
using X.PagedList;
using System.Net.Sockets;
using MeteorStrike.Services;
using MeteorStrike.Models.Enums;
using MeteorStrike.Models.ViewModels;

namespace MeteorStrike.Controllers
{
    [Authorize]

    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _btFileService;
        private readonly IBTTicketService _btTicketService;
        private readonly IBTRolesService _btRolesService;

        public TicketsController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IBTFileService btFileService,
                                  IBTTicketService btTicketService,
                                  IBTRolesService btRolesService)
        {
            _context = context;
            _userManager = userManager;
            _btFileService = btFileService;
            _btTicketService = btTicketService;
            _btRolesService = btRolesService;
        }
        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tickets.Where(p => p.Archived == false)
                                                       .Include(t => t.DeveloperUser)
                                                       .Include(t => t.Project)
                                                       .Include(t => t.SubmitterUser)
                                                       .Include(t => t.TicketPriority)
                                                       .Include(t => t.TicketStatus)
                                                       .Include(t => t.TicketType);

            return View(await applicationDbContext.ToListAsync());

            //int pageSize = 6;

            //int page = pageNum ?? 1;

            //int companyId = User.Identity!.GetCompanyId();

            //IPagedList<Ticket> tickets = (await _btTicketService.GetTicketsAsync(companyId)).ToPagedList(page, pageSize);

            //return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.Comments)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }


        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        //GET: Assign Project Members
        public async Task<IActionResult> AssignTicketMember(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _btTicketService.GetTicketAsync(id);

            List<BTUser> developers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            string? currentDeveloperId = ticket.DeveloperUser?.Id;

            TicketMemberViewModel viewModel = new()
            {
                Ticket = ticket,
                UserList = new SelectList(developers, "Id", "FullName", currentDeveloperId),
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        //POST: Assign Project Members
        //public async Task<IActionResult> AssignProjectMembers(ProjectMembersViewModel viewModel)
        //{
        //    int companyId = User.Identity!.GetCompanyId();

        //    if (viewModel.SelectedMembers != null)
        //    {
        //        //Remove current members
        //        await _btProjectService.RemoveMembersFromProjectAsync(viewModel.Project!.Id, companyId);

        //        //Add newly selected members
        //        await _btProjectService.AddMembersToProjectAsync(viewModel.SelectedMembers, viewModel.Project!.Id, companyId);

        //        return RedirectToAction(nameof(Details), new { id = viewModel.Project!.Id });
        //    }

        //    ModelState.AddModelError("SelectedMembers", "No Users selected. Please select some Users.");

        //    //Reset the form
        //    viewModel.Project = await _btProjectService.GetProjectByIdAsync(viewModel.Project!.Id, companyId);
        //    List<string> currentMembers = viewModel.Project.Members.Select(m => m.Id).ToList();

        //    List<BTUser> submitters = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
        //    List<BTUser> developers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
        //    List<BTUser> userList = submitters.Concat(developers).ToList();

        //    viewModel.UserList = new MultiSelectList(userList, "Id", "FullName", currentMembers);

        //    return View(viewModel);

        //}


        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            BTUser? btUser = await _userManager.GetUserAsync(User);

            IEnumerable<Project> projects = _context.Projects.Where(p => p.CompanyId == btUser!.CompanyId);

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                BTUser? btUser = await _userManager.GetUserAsync(User);

                ticket.SubmitterUserId = btUser!.Id;

                //Created Date
                ticket.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);

                ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);

                ticket.TicketStatusId = 1;

                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Id", ticket.TicketTypeId);

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Created = DataUtility.GetPostGresDate(ticket.Created);
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
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

            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.Project)
                .Include(t => t.SubmitterUser)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                ticket.Archived = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,BTUserId,Comment,Created")] TicketComment ticketComment)
        {
            ModelState.Remove("BTUserId");

            if (ModelState.IsValid)
            {
                BTUser? btUser = await _userManager.GetUserAsync(User);

                int ticketId = ticketComment.TicketId;

                ticketComment.BTUserId = btUser!.Id;

                ticketComment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);

                _context.Add(ticketComment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = ticketId });
            }

            return RedirectToAction(nameof(Details));
        }


        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
		{
			string statusMessage;

			if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
				ticketAttachment.FileData = await _btFileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
				ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

				ticketAttachment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
				ticketAttachment.BTUserId = _userManager.GetUserId(User);

				await _btTicketService.AddTicketAttachmentAsync(ticketAttachment);
				statusMessage = "Success: New attachment added to Ticket.";
			}
			else
			{
				statusMessage = "Error: Invalid data.";

			}

			return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
		}



	}
}
