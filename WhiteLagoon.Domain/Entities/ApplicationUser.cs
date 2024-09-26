using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WhiteLagoon.Domain.Entities
{
	public class ApplicationUser : IdentityUser
	{
		[Required]
		public string Name { get; set; }
		public DateTime? CreatedAt { get; set; }
	}
}
