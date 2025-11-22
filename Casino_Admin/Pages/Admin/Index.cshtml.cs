using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Casino_Admin.Data;
using Casino_Admin.Models;
using System.Collections.Generic;
using System.Linq;

[Authorize(Roles = "Admin")]
public class AdminIndexModel : PageModel
{
    private readonly AppDbContext _db;
    public AdminIndexModel(AppDbContext db) { _db = db; }
    public List<User> Users { get; set; } = new();
    public void OnGet()
    {
        Users = _db.Users.OrderByDescending(u => u.Balance).ToList();
    }
}