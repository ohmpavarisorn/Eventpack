using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EventPack.Models;
using EventPack.Models.Db;

namespace EventPack.Controllers;

public class HomeController : Controller
{
    private readonly Csi402projectContext _Db;
    public HomeController(Csi402projectContext Db)
    {
        _Db = Db;
    }
    public IActionResult Index()
    {
        var user = (from u in _Db.Userinformations select u).ToList();
                   
        return View(user);
    }
   
    public IActionResult Privacy()
    {
        return View();
    }
  public IActionResult Test()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
