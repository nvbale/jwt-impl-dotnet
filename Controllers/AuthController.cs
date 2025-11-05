using jwt_impl.Constants;
using jwt_impl.Models;
using jwt_impl.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace jwt_impl.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup(SignupModel model)
    {
        try
        {
            var existingUser = await _userManager.FindByNameAsync(model.Email);
            if (existingUser != null)
                return BadRequest("User already exists");

            if (!await _roleManager.RoleExistsAsync(Roles.User))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(Roles.User));
                if (!roleResult.Succeeded)
                {
                    var roleErrors = roleResult.Errors.Select(e => e.Description);
                    _logger.LogError($"Failed to create user role. Errors: {string.Join(", ", roleErrors)}");
                    return BadRequest($"Failed to create user role. Errors: {string.Join(", ", roleErrors)}");
                }
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                UserName = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                Name = model.Name,
                EmailConfirmed = true,
            };

            var createUserResult = await _userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                var errors = createUserResult.Errors.Select(e => e.Description);
                _logger.LogError($"Failed to create user. Errors {string.Join(", ", errors)}");
                return BadRequest($"Failed to create user. Errors {string.Join(", ", errors)}");
            }

            var addUserToRoleResult = await _userManager.AddToRoleAsync(user, Roles.User);

            if (!addUserToRoleResult.Succeeded)
            {
                var errors = addUserToRoleResult.Errors.Select(e => e.Description);
                _logger.LogError($"Failed to add role to the user. Errors: {string.Join(", ", errors)}");
            }

            return CreatedAtAction(nameof(Signup), null);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
}