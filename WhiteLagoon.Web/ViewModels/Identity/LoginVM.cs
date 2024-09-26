using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WhiteLagoon.Web.ViewModels.Identity
{
	public class LoginVM
	{
		[Required]
		[DataType(DataType.EmailAddress)]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DisplayName("Remember Me?")]
		public bool RememberMe { get; set; }
	}
}
