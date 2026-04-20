using ArderBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArderBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            
            if (user != null)
            {
                // User exists, update their onboarding fields for testing convenience
                user.Goal = model.Goal ?? user.Goal;
                user.BirthDate = model.BirthDate ?? user.BirthDate;
                user.Weight = model.Weight ?? user.Weight;
                user.Height = model.Height ?? user.Height;
                user.Experience = model.Experience ?? user.Experience;
                user.DiscoverySource = model.DiscoverySource ?? user.DiscoverySource;
                user.TermsAcceptedDate = model.TermsAcceptedDate ?? user.TermsAcceptedDate;

                await _userManager.UpdateAsync(user);
                return Ok(new { Message = "User updated (logged in) successfully", UserId = user.Id });
            }

            // Create new ApplicationUser populating with all onboarding options
            user = new ApplicationUser
            {
                UserName = string.IsNullOrWhiteSpace(model.Username) ? model.Email : model.Username,
                Email = model.Email,
                Goal = model.Goal,
                BirthDate = model.BirthDate,
                Weight = model.Weight,
                Height = model.Height,
                Experience = model.Experience,
                DiscoverySource = model.DiscoverySource,
                TermsAcceptedDate = model.TermsAcceptedDate
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { Message = $"User creation failed: {errors}" });
            }

            // In a real application, you would generate a JWT here and return it.
            return Ok(new { Message = "User registered successfully", UserId = user.Id });
        }
    }
}
