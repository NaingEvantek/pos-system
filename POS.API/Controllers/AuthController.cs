using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.DTOs;
using POS.API.Models;
using POS.API.Services;
using BCrypt.Net;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly POSDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(POSDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            // Find user by username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is inactive. Please contact administrator." });
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Update last login
            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            var response = new LoginResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    LastLogin = user.LastLogin
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create new user
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                FullName = registerDto.FullName,
                Role = registerDto.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            var response = new LoginResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    LastLogin = user.LastLogin
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
        }
    }

    [HttpPost("init")]
    public async Task<IActionResult> InitializeDefaultUsers()
    {
        try
        {
            // Check if users already exist with proper hashes
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            var cashierUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "cashier");

            var created = new List<string>();

            // Create or update admin user
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@pos.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "System Administrator",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(adminUser);
                created.Add("admin");
            }
            else if (adminUser.PasswordHash == "TEMP_HASH_WILL_BE_REPLACED")
            {
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                created.Add("admin (updated)");
            }

            // Create or update cashier user
            if (cashierUser == null)
            {
                cashierUser = new User
                {
                    Username = "cashier",
                    Email = "cashier@pos.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "Default Cashier",
                    Role = UserRole.Cashier,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(cashierUser);
                created.Add("cashier");
            }
            else if (cashierUser.PasswordHash == "TEMP_HASH_WILL_BE_REPLACED")
            {
                cashierUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                created.Add("cashier (updated)");
            }

            await _context.SaveChangesAsync();

            if (created.Count > 0)
            {
                return Ok(new
                {
                    message = "Default users initialized successfully",
                    users = created,
                    credentials = new
                    {
                        admin = new { username = "admin", password = "admin123" },
                        cashier = new { username = "cashier", password = "admin123" }
                    }
                });
            }
            else
            {
                return Ok(new
                {
                    message = "Default users already exist with proper passwords",
                    credentials = new
                    {
                        admin = new { username = "admin", password = "admin123" },
                        cashier = new { username = "cashier", password = "admin123" }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during initialization", error = ex.Message });
        }
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { message = "Auth endpoint is working!" });
    }
}
