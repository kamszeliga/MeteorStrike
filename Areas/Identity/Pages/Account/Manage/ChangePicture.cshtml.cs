using MeteorStrike.Extentions;
using MeteorStrike.Models;
using MeteorStrike.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeteorStrike.Areas.Identity.Pages.Account.Manage
{
    public class ChangePictureModel : PageModel
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyService _btCompanyService;

        public ChangePictureModel(
            UserManager<BTUser> userManager,
            IBTCompanyService btCompanyService)
        {
            _userManager = userManager;
            _btCompanyService = btCompanyService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public virtual byte[]? ImageFileData { get; set; }

        public virtual string? ImageFileType { get; set; }

        [NotMapped]
        public virtual IFormFile? ImageFormFile { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            public virtual byte[]? ImageFileData { get; set; }

            public virtual string? ImageFileType { get; set; }
            public virtual IFormFile? ImageFormFile { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string? userId = _userManager.GetUserId(User);

            int companyId = User.Identity.GetCompanyId();

            BTUser user = await _btCompanyService.GetMemberAsync(userId, companyId);


            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            ViewData["CurrentUser"] = user;
            return Page();
        }

        private async Task LoadAsync(BTUser user)
        {
            byte[]? imageData = user.ImageFileData;
            string? imageType = user.ImageFileType;


            Input = new InputModel
            {
                ImageFileType = imageType,
                ImageFileData = imageData
            };
        }

        public async Task<IActionResult> OnPostAsync(IFormFile imageFormFile)
        {

            BTUser? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            else
            {
                await _btCompanyService.UpdateMemberAsync(imageFormFile, user);

                await LoadAsync(user);

                ViewData["CurrentUser"] = user;
                return Page();
            }

        }

    }

}
