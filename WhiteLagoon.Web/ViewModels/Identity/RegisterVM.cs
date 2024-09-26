using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WhiteLagoon.Web.ViewModels.Identity
{
	public class RegisterVM
	{
		[Required]
		public string Name { get; set; }

		[Required]
		[DataType(DataType.EmailAddress)]
		[EmailAddress]
		[Remote(action: "IsEmailAlreadyRegisterd", controller: "Account", ErrorMessage = "Email is already use")]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} character long", MinimumLength = 6)]
		public string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[DisplayName("Confirm Password")]
		[Compare("Password", ErrorMessage = "The password and confirm password don't match")]
		public string ConfirmPassword { get; set; }

		[Display(Name = "Role")]
		public string? RoleSelect { get; set; }
		[ValidateNever]
		public IEnumerable<SelectListItem> RoleList { get; set; }
	}
}
