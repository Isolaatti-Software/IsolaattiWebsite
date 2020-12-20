/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace isolaatti_API.Pages.admin
{
    public class AllElementsOnQueue : PageModel
    {
        private readonly DbContextApp _db;
        public IQueryable<SongQueue> GeneralQueue;
        
        // true if it's showing only pending elements,
        // false if showing also past elements
        public bool IsDefaultViewMode = true;

        public AllElementsOnQueue(DbContextApp db)
        {
            _db = db;
        }
        public IActionResult OnGet(string view = null, string delete = null, int id = -1, string from = null)
        {
            var username = Request.Cookies["name"];
            var password = Request.Cookies["password"];

            if (username == null || password == null) return RedirectToPage("LogIn");
            if (!_db.AdminAccounts.Any(ac => ac.name.Equals(username))) return RedirectToPage("LogIn");
            var account = _db.AdminAccounts.Single(ac => ac.name.Equals(username));
            if (!account.password.Equals(password)) return RedirectToPage("LogIn");

            // here is safe
            ViewData["username"] = account.name;
            // should 
            if (view == "alsoPast")
            {
                GeneralQueue = _db.SongsQueue.AsQueryable();
                ViewData["from"] = "alsoPast";
                IsDefaultViewMode = false;
                return Page();
            }
            if (delete != null)
            {
                switch (delete)
                {
                    case "all":
                        
                        var allElements = _db.SongsQueue.AsQueryable();
                        foreach (var element in allElements)
                        {
                            _db.SongsQueue.Remove(element);
                        }

                        _db.SaveChanges();
                        
                        ViewData["alert"] = "All elements were deleted";
                        switch (from)
                        {
                            case "pending":
                                GeneralQueue = _db.SongsQueue.Where(element => !element.Reserved);
                                break;
                            case "alsoPast": 
                                GeneralQueue = _db.SongsQueue.AsQueryable();
                                IsDefaultViewMode = false;
                                break;
                        }
                        GeneralQueue = _db.SongsQueue.AsQueryable();
                        break;
                    case "single":
                        var singleElement = 
                            _db.SongsQueue.Single(element => element.Id.Equals(id));
                        _db.SongsQueue.Remove(singleElement);
                        _db.SaveChanges();
                        ViewData["alert"] = $"Element with id {id} was deleted";
                        switch (from)
                        {
                            case "pending":
                                GeneralQueue = _db.SongsQueue.Where(element => !element.Reserved);
                                break;
                            case "alsoPast": 
                                GeneralQueue = _db.SongsQueue.AsQueryable();
                                IsDefaultViewMode = false;
                                break;
                        }
                        break;
                    case "onlyPast":
                        var pastElements =
                            _db.SongsQueue.Where(element => element.Reserved);
                        foreach (var element in pastElements)
                        {
                            _db.SongsQueue.Remove(element);
                        }

                        _db.SaveChanges();
                        ViewData["alert"] = "All past elements were deleted";
                        switch (from)
                        {
                            case "pending":
                                GeneralQueue = _db.SongsQueue.Where(element => !element.Reserved);
                                break;
                            case "alsoPast": 
                                GeneralQueue = _db.SongsQueue.AsQueryable();
                                IsDefaultViewMode = false;
                                break;
                        }
                        break;
                    case "onlyPending": 
                        var pendingElements =
                            _db.SongsQueue.Where(element => !element.Reserved);
                        foreach (var element in pendingElements)
                        {
                            _db.SongsQueue.Remove(element);
                        }

                        _db.SaveChanges();
                        ViewData["alert"] = "All pending elements were deleted";
                        switch (from)
                        {
                            case "pending":
                                GeneralQueue = _db.SongsQueue.Where(element => !element.Reserved);
                                break;
                            case "alsoPast": 
                                GeneralQueue = _db.SongsQueue.AsQueryable();
                                IsDefaultViewMode = false;
                                break;
                        }
                        break;
                }

                ViewData["from"] = from;
                return Page();
            }


            // when no arguments were passed
            GeneralQueue = _db.SongsQueue.Where(element => !element.Reserved);
            ViewData["from"] = "pending";
            return Page();
        }
    }
}