using M_UserLogin.Data;
using M_UserLogin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ✅ Adds MVC controller + view support
builder.Services.AddControllersWithViews();

// 🧩 Connects your project to SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔐 ASP.NET Core Identity setup
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 🚫 Redirect unauthorized users to AccessDenied page
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Shared/AccessDenied";
});

// 🔹 JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

// 🔐 Only JWT authentication (cookie disabled for now)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// 🧩 Add the custom JWT token generator service
builder.Services.AddScoped<M_UserLogin.Helpers.JwtTokenService>();

var app = builder.Build();

// 🧠 Seed Admin role and user + leave balances at startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Ensure Admin role exists
    string adminRole = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // Seed Admin user
    string adminEmail = "hmurtaza510@gmail.com"; // ✅ Replace with your main email
    string adminPassword = "murtaza123"; // ✅ You can change this

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new Users
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin User",
            CasualLeaveBalance = 12,
            SickLeaveBalance = 6,
            AnnualLeaveBalance = 14
        };
        await userManager.CreateAsync(adminUser, adminPassword);
        await userManager.AddToRoleAsync(adminUser, adminRole);
        Console.WriteLine("✅ Admin user created with leave balances.");
    }
    else
    {
        // Ensure admin has leave balances
        bool updated = false;
        if (adminUser.CasualLeaveBalance == 0) { adminUser.CasualLeaveBalance = 12; updated = true; }
        if (adminUser.SickLeaveBalance == 0) { adminUser.SickLeaveBalance = 6; updated = true; }
        if (adminUser.AnnualLeaveBalance == 0) { adminUser.AnnualLeaveBalance = 14; updated = true; }

        if (updated)
        {
            dbContext.Users.Update(adminUser);
            await dbContext.SaveChangesAsync();
            Console.WriteLine("✅ Admin leave balances updated.");
        }

        if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
            Console.WriteLine("✅ Admin role assigned successfully.");
        }
    }

    // Seed leave balances for all other users if not set
    var allUsers = dbContext.Users.Where(u => u.Id != adminUser.Id).ToList();
    foreach (var user in allUsers)
    {
        bool updated = false;
        if (user.CasualLeaveBalance == 0) { user.CasualLeaveBalance = 12; updated = true; }
        if (user.SickLeaveBalance == 0) { user.SickLeaveBalance = 6; updated = true; }
        if (user.AnnualLeaveBalance == 0) { user.AnnualLeaveBalance = 14; updated = true; }
        if (updated)
        {
            dbContext.Users.Update(user);
        }
    }
    await dbContext.SaveChangesAsync();
}

// ⚙️ Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 📍 Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

app.Run();
