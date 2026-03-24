using AthensWorkspace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AthensWorkspace.Data;

public class MyIdentityDbContext(DbContextOptions<MyIdentityDbContext> options)
    : IdentityDbContext<OAuthUser, IdentityRole<int>, int>(options);