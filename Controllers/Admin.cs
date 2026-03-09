using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EventPack.Models;
using EventPack.Models.Db;

namespace EventPack.Controllers;

public class Admin : Controller
{
    private readonly Csi402projectContext Db;

    public Admin(Csi402projectContext context)
    {
        Db = context;
    }

    public IActionResult userController()
    {
        var users = Db.Userinformations.ToList();
        return View(users);
    }
}