using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Task4.Data;
using Task4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

namespace Task4.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;

        private UsersContext db;
        public HomeController(ILogger<HomeController> logger, UsersContext context, SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            db = context;
        }

        public IActionResult Index()
        {
            ViewData["TwitterCount"] = CountInBase("Twitter");
            ViewData["MicrosoftCount"] = CountInBase("Microsoft");
            ViewData["FacebookCount"] = CountInBase("Facebook");
            if(CheckState() == true)
            {
                ViewData["BlockState"] = true;
            }
            else
            {
                ViewData["BlockState"] = false;
            }
            return View();
        }
        public int CountInBase(string SocialNetwork)
        {
            int CountTwitter = db.Users.Count(x => x.SocialNetwork == SocialNetwork);
            return CountTwitter;
        }

        public enum SortState
        {
            IdAsc,
            IdDesc,
            NameAsc,
            NameDesc,
            SocialNetworkAsc,
            SocialNetworkDesc,
            FirstEnterAsc,
            FirstEnterDesc,
            LastEnterAsc,
            LastEnterDesc,
            StateAsc,
            StateDesc
        }


        [Authorize]
        public IActionResult TablePage(string SocialNetwork, string Blocked, string Act, int[] Selected, SortState SortOrder = SortState.IdAsc)
        {
            if (CheckState() == true)
            {
                return Redirect("/Home/Index");
            }

            ViewData["SocialNetworkSave"] = SocialNetwork;
            ViewData["BlockedSave"] = Blocked;

            IQueryable<UserDataModel> UsersBuffer = db.Users;

            _logger.LogInformation(Selected.Length.ToString());

            if (Act == "UserBlock")
            {
                if (UnBlockUsers(Selected, false) == true)
                {
                    _signInManager.SignOutAsync();
                    return Redirect("/Home/Index");
                }
            }
            else if (Act == "UserUnblock")
            {
                UnBlockUsers(Selected, true);
            }
            else if (Act == "UserDelete")
            {
                if (DeleteUsers(Selected) == true)
                {
                    _signInManager.SignOutAsync();
                    return Redirect("/Home/Index");
                }
            }

            List<UserDataModel> SocialNetworks = PrepareSocialList(new List<UserDataModel>());
            List<UserDataModel> States = PrepareStateList(new List<UserDataModel>());
  
            if (SocialNetwork != null && SocialNetwork != "Все")
            {
                UsersBuffer = UsersBuffer.Where(p => p.SocialNetwork.Contains(SocialNetwork));
            }
            if (Blocked != null && Blocked != "Все")
            {
                UsersBuffer = UsersBuffer.Where(p => p.State.Contains(Blocked));
            }

            UsersBuffer = TableSort(UsersBuffer, SortOrder);

            TableViewModel ViewModel = new TableViewModel
            {
                Users = UsersBuffer,
                SocialNetworksList = new SelectList(SocialNetworks, "SocialNetwork", "SocialNetwork"),
                BlockedList = new SelectList(States, "State", "State")
            };

            return View(ViewModel);
        }

        List<UserDataModel> PrepareSocialList(List<UserDataModel> SocialNetworks)
        {
            List<string> NetworkBuffer = db.Users.Select(x => x.SocialNetwork).Distinct().ToList();
            foreach (string Network in NetworkBuffer)
            {
                SocialNetworks.Insert(0, new UserDataModel { SocialNetwork = Network });
            }
            SocialNetworks.Insert(0, new UserDataModel { SocialNetwork = "Все" });
            return SocialNetworks;
        }

        List<UserDataModel> PrepareStateList(List<UserDataModel> States)
        {
            List<string> StatesBuffer = db.Users.Select(x => x.State).Distinct().ToList();
            foreach (string State in StatesBuffer)
            {
                States.Insert(0, new UserDataModel { State = State });
            }
            States.Insert(0, new UserDataModel { State = "Все" });
            return States;
        }

        bool CheckState()
        {
            if(User.Identity.Name == null)
            {
                return false;
            }
            if (db.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault() != null)
            {
                if (db.Users.Where(x => x.Email == User.Identity.Name).FirstOrDefault().State == "Block")
                {
                    return true;
                }
            }
            return false;
        }

         bool UnBlockUsers(int[] Selected, bool Unblock)
         {
            foreach (int CurrentId in Selected)
            {
                UserDataModel CurrentUser = db.Users.Where(x=>x.Id == CurrentId).FirstOrDefault();
                if (Unblock == false)
                {
                    CurrentUser.State = "Block";
                    if (CurrentUser.Email == User.Identity.Name)
                    {
                        db.SaveChanges();
                        return true;
                    }
                }
                else if (Unblock == true)
                CurrentUser.State = "Active";
                db.SaveChanges();
            }
            _logger.LogInformation("UserBlock");
            return false;
        }

        bool DeleteUsers(int[] Selected)
        {
            foreach (int CurrentId in Selected)
            {
                UserDataModel CurrentUser = db.Users.Where(x => x.Id == CurrentId).FirstOrDefault();
                db.Users.Remove(CurrentUser);
                if (CurrentUser.Email == User.Identity.Name)
                {             
                    db.SaveChanges();
                    return true;
                }
                db.SaveChanges();
            }
            return false;
        }

        private IQueryable<UserDataModel> TableSort(IQueryable<UserDataModel> ForSort, SortState SortOrder)
        {
            ViewData["IdSort"] = SortOrder == SortState.IdAsc ? SortState.IdDesc : SortState.IdAsc;
            ViewData["NameSort"] = SortOrder == SortState.NameAsc ? SortState.NameDesc : SortState.NameAsc;
            ViewData["SocialNetworkSort"] = SortOrder == SortState.SocialNetworkAsc ? SortState.SocialNetworkDesc : SortState.SocialNetworkAsc;
            ViewData["FirstEnterSort"] = SortOrder == SortState.FirstEnterAsc ? SortState.FirstEnterDesc : SortState.FirstEnterAsc;
            ViewData["LastEnterSort"] = SortOrder == SortState.LastEnterAsc ? SortState.LastEnterDesc : SortState.LastEnterAsc;
            ViewData["StateSort"] = SortOrder == SortState.StateAsc ? SortState.StateDesc : SortState.StateAsc;


            ForSort = SortOrder switch
            {
                SortState.IdDesc => ForSort.OrderByDescending(s => s.Id),
                SortState.NameAsc => ForSort.OrderBy(s => s.SocialUserName),
                SortState.NameDesc => ForSort.OrderByDescending(s => s.SocialUserName),
                SortState.SocialNetworkAsc => ForSort.OrderBy(s => s.SocialNetwork),
                SortState.SocialNetworkDesc => ForSort.OrderByDescending(s => s.SocialNetwork),
                SortState.FirstEnterAsc => ForSort.OrderBy(s => s.FirstEnter),
                SortState.FirstEnterDesc => ForSort.OrderByDescending(s => s.FirstEnter),
                SortState.LastEnterAsc => ForSort.OrderBy(s => s.LastEnter),
                SortState.LastEnterDesc => ForSort.OrderByDescending(s => s.LastEnter),
                SortState.StateAsc => ForSort.OrderBy(s => s.State),
                SortState.StateDesc => ForSort.OrderByDescending(s => s.State),
                SortState.IdAsc => ForSort.OrderBy(s => s.Id),
            };
            return ForSort;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {        
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
