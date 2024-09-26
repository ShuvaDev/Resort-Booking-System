using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels.Identity;

namespace WhiteLagoon.Web.Controllers
{
    public class AccountController : Controller
    {
		private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
			_unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Register(string? returnurl = null)
        {
            if (!_roleManager.RoleExistsAsync(SD.Admin).GetAwaiter().GetResult())
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.User));
            }

            RegisterVM registerViewModel = new()
            {
                RoleList = _roleManager.Roles.Select(r => r.Name).Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
                {
                    Text = r,
                    Value = r
                })
            };

            ViewData["ReturnUrl"] = returnurl;

            return View(registerViewModel);
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterVM registerViewModel, string? returnurl = null)
		{
			returnurl = returnurl ?? Url.Content("~/");

			if (ModelState.IsValid)
			{
				var user = new ApplicationUser()
				{
					UserName = registerViewModel.Email,
					Email = registerViewModel.Email,
					Name = registerViewModel.Name,
					CreatedAt = DateTime.Now
				};

				var result = await _userManager.CreateAsync(user, registerViewModel.Password);
				if (result.Succeeded)
				{
					if (registerViewModel.RoleSelect != null)
					{
						await _userManager.AddToRoleAsync(user, registerViewModel.RoleSelect);
					}
					else
					{
						await _userManager.AddToRoleAsync(user, SD.User);
					}

					await _signInManager.SignInAsync(user, isPersistent: false);
					return LocalRedirect(returnurl);
				}
				else
				{
					foreach (IdentityError error in result.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
				}
			}
			registerViewModel.RoleList = _roleManager.Roles.Select(r => r.Name).Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem()
			{
				Text = r,
				Value = r
			});

			return View(registerViewModel);
		}

		public IActionResult Login(string? returnurl = null)
		{
			ViewData["ReturnUrl"] = returnurl;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginVM loginViewModel, string? returnurl)
		{
			returnurl = returnurl ?? Url.Content("~/");
			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, isPersistent: loginViewModel.RememberMe, lockoutOnFailure : false);
				if (result.Succeeded)
				{
					var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
					if(await _userManager.IsInRoleAsync(user, SD.Admin))
					{
						return RedirectToAction("Index", "Dashboard");
					}
					return LocalRedirect(returnurl);
				}
				
				else
				{
					ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
					return View(loginViewModel);
				}
			}

			return View(loginViewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		// Microsoft.AspeNetCore.ViewFeature
		public async Task<IActionResult> IsEmailAlreadyRegisterd(string email)
		{
			ApplicationUser user = await _userManager.FindByEmailAsync(email);
			if (user == null)
			{
				return Json(true);
			}
			else
			{
				return Json(false);
			}
		}
	}
}
