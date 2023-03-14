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
using Org.BouncyCastle.Bcpg;

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
        private readonly IBTTicketHistoryService _btTicketHistoryService;
        private readonly IBTProjectService _btProjectService;
        private readonly IBTNotification _btNotification;

        public TicketsController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IBTFileService btFileService,
                                  IBTTicketService btTicketService,
                                  IBTRolesService btRolesService,
                                  IBTTicketHistoryService btTicketHistoryService, 
                                  IBTProjectService btProjectService,
                                  IBTNotification btNotification)
        {
            _context = context;
            _userManager = userManager;
            _btFileService = btFileService;
            _btTicketService = btTicketService;
            _btRolesService = btRolesService;
            _btTicketHistoryService = btTicketHistoryService;
            _btProjectService = btProjectService;
            _btNotification = btNotification;
        }


        //GET: MyTickets
        public async Task<IActionResult> TicketHistory()
        { 
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<TicketHistory> ticketHistory = await _btTicketHistoryService.GetCompanyTicketHistoriesAsync(companyId); 

            return View(ticketHistory);
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Ticket> tickets = await _btTicketService.GetTicketsAsync(companyId);

            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _btTicketService.GetTicketAsync(id);

            return View(ticket);
        }


        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        //GET: Assign Ticket Members
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
        //POST: Assign Ticket Members
        public async Task<IActionResult> AssignTicketMember(TicketMemberViewModel viewModel)
        {
            Ticket ticket = await _btTicketService.GetTicketAsync(viewModel.Ticket?.Id);
			int companyId = User.Identity!.GetCompanyId();

			if (viewModel.DeveloperId != null)
            {
                //Add newly selected member
                ticket.DeveloperUserId = viewModel.DeveloperId;

                await _btTicketService.UpdateTicketAsync(ticket);

                return RedirectToAction(nameof(Details), new { id = viewModel.Ticket!.Id });
            }

            ModelState.AddModelError("SelectedMembers", "No Users selected. Please select some Users.");

            //History
            string? userId = _userManager.GetUserId(User);
			Ticket? newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

			await _btTicketHistoryService.AddHistoryAsync(null, newTicket, userId);

			// Notification
			BTUser? btUser = await _userManager.GetUserAsync(User);

			Notification? notification = new()
			{
				TicketId = ticket.Id,
				Title = "Developer Assigned",
				Message = $"Ticket: {ticket.Title} was assigned by by {btUser?.FullName}.",
				Created = DataUtility.GetPostGresDate(DateTime.UtcNow),
				SenderId = userId,
				RecipientId =ticket.DeveloperUserId,
				NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
			};
			    
            await _btNotification.AdminNotificationAsync(notification, companyId);
			    
            await _btNotification.SendAdminEmailNotificationAsync(notification, "New Ticket Added; No Project Manager Found", companyId);
                


			return View(viewModel);

        }


        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {

            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Project> projects = await _btProjectService.GetCompanyProjectsAsync(companyId);

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(await _btTicketService.GetTicketPriorities(), "Id", "Name");
            //ViewData["TicketStatusId"] = new SelectList(await _btTicketService.GetTicketStatuses(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _btTicketService.GetTicketTypes(), "Id", "Name");
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

            int companyId = User.Identity!.GetCompanyId();

            if (ModelState.IsValid)
            {

                BTUser? btUser = await _userManager.GetUserAsync(User);

                ticket.SubmitterUserId = btUser!.Id;

                ticket.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);

                ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);

                ticket.TicketStatusId = 1;

                await _btTicketService.AddTicketAsync(ticket);

                string? userId = _userManager.GetUserId(User);

                // History
                Ticket? newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _btTicketHistoryService.AddHistoryAsync(null, newTicket, userId);


                // Notification
                BTUser? projectManager = await _btProjectService.GetProjectManagerAsync(ticket.ProjectId);

                Notification? notification = new()
                {
                    ProjectId = ticket.ProjectId,
                    TicketId = ticket.Id,
                    Title = "New Ticket Added",
                    Message = $"New Ticket: {ticket.Title} was created by {btUser?.FullName}.",
                    Created= DataUtility.GetPostGresDate(DateTime.UtcNow),
                    SenderId = userId,
                    RecipientId = projectManager?.Id,
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n=>n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                };

                if (projectManager != null )
                {
                    await _btNotification.AddNotificationAsync(notification);
                    await _btNotification.SendEmailNotificationAsync(notification, "New Ticket Added");
                }
                else
                {
                    await _btNotification.AdminNotificationAsync(notification, companyId);
                    await _btNotification.SendAdminEmailNotificationAsync(notification, "New Ticket Added; No Project Manager Found", companyId);

                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["DeveloperUserId"] = new SelectList(await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId), "Id", "FullName", ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(await _btTicketService.GetTicketPriorities(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(await _btTicketService.GetTicketStatuses(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _btTicketService.GetTicketTypes(), "Id", "Name");

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _btTicketService.GetTicketAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["DeveloperUserId"] = new SelectList(await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId), "Id", "FullName", ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(await _btTicketService.GetTicketPriorities(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(await _btTicketService.GetTicketStatuses(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _btTicketService.GetTicketTypes(), "Id", "Name");


            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string? userId = _userManager.GetUserId(User);

                Ticket? oldTicket = await _btTicketService.GetTicketAsNoTrackingAsync(id, companyId);

                try
                {
                    ticket.Created = DataUtility.GetPostGresDate(ticket.Created);
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);

                    await _btTicketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _btTicketService.TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                //Add History
                Ticket newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(id, companyId);

                await _btTicketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);



                return RedirectToAction(nameof(Index));
            }

            ViewData["DeveloperUserId"] = new SelectList(await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId), "Id", "FullName", ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(await _btTicketService.GetTicketPriorities(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(await _btTicketService.GetTicketStatuses(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _btTicketService.GetTicketTypes(), "Id", "Name");

            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _btTicketService.GetTicketAsync(id);


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

            Ticket ticket = await _btTicketService.GetTicketAsync(id);

            if (ticket != null)
            {
                ticket.Archived = true;
            }

            await _btTicketService.UpdateTicketAsync(ticket);

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

                await _btTicketService.AddTicketComment(ticketComment);

                await _btTicketHistoryService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.BTUserId);

                return RedirectToAction("Details", new { id = ticketId });
            }

            return RedirectToAction(nameof(Details));
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
		{
			string statusMessage;

            ModelState.Remove("BTUserId");

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
				ticketAttachment.FileData = await _btFileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
				ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
				ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

				ticketAttachment.Created = DataUtility.GetPostGresDate(DateTime.UtcNow);
				ticketAttachment.BTUserId = _userManager.GetUserId(User);

                await _btTicketService.AddTicketAttachmentAsync(ticketAttachment);

                await _btTicketHistoryService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.BTUserId);

                statusMessage = "Success: New attachment added to Ticket.";
			}
			else
			{
				statusMessage = "Error: Invalid data.";

			}



			return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
		}

		public async Task<IActionResult> ShowFile(int id)
		{
			TicketAttachment ticketAttachment = await _btTicketService.GetTicketAttachmentByIdAsync(id);
			string fileName = ticketAttachment.FileName;
			byte[] fileData = ticketAttachment.FileData;
			string ext = Path.GetExtension(fileName).Replace(".", "");

			Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
			return File(fileData, $"application/{ext}");
		}

	}
}
