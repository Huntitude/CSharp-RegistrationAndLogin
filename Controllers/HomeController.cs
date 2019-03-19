using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
//Added
using LoginRegistration.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace LoginRegistration.Controllers
{
    public class HomeController : Controller
    {
        //Added Context
        private Context dbContext;
        // here we can "inject" our context service into the constructor
        public HomeController(Context context)
        {
            dbContext = context;
        }

        // ============================================
        //                    ROUTES
        // ============================================

        // GET: /Home/
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        // ============================================
        //               GET SUCCESS
        // ============================================
        // GET: /success redirect to Index if without session
        [HttpGet("success")]
        public IActionResult Home()
        {
            // Session Check
            if(HttpContext.Session.GetInt32("userID")  == null)
            {
                return RedirectToAction("Index");
            }
            // Set session to id = userID
            int? id = HttpContext.Session.GetInt32("userID") ?? default(int);
            // Set ViewBag CurrentUser to match the session id from above
            ViewBag.CurrentUser = dbContext.Users.FirstOrDefault(u => u.UserId == id);
            return View("Home");
        }

        // ============================================
        //               POST SUCCESS
        // ============================================
        // POST: /Register
        [HttpPost("register")]
        public IActionResult Register(ViewModel modelData)
        {
            User registerUser = modelData.User;
            //Check inital Modelstate
            if(ModelState.IsValid)
            {
                //If user exists with given email
                if(dbContext.Users.Any(use => use.Email == registerUser.Email))
                {
                    ModelState.AddModelError("User.Email", "Email already taken!");
                    return View("Index");
                }
                //Password hashing
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                registerUser.Password = Hasher.HashPassword(registerUser, registerUser.Password);
                //Add user to DB & save
                dbContext.Add(registerUser);
                dbContext.SaveChanges();
                //Set session
                HttpContext.Session.SetInt32("userID", (int)registerUser.UserId);
                int? userID = HttpContext.Session.GetInt32("userID");
                // System.Console.WriteLine($"\n\n\n{userID}");
                return RedirectToAction("Home");
            }
            // Validation failed: Return in Index
        return View("Index");

        }

        // ============================================
        //               POST LOGIN
        // ============================================
        // POST: /login
        [HttpPost("login")]
        public IActionResult Login(ViewModel modelData)
        {
            UserLogin loginUser = modelData.Userlogin;
            // If model is valid
            if(ModelState.IsValid)
            {
                // Check email in the DB
                var userlogin = dbContext.Users.FirstOrDefault( use => use.Email == loginUser.Email);
                if(userlogin == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                // Password hasher
                var hasher = new PasswordHasher<UserLogin>();
                var result = hasher.VerifyHashedPassword(loginUser, userlogin.Password, loginUser.Password);
                if(result == 0)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                // Store Session ID
                HttpContext.Session.SetInt32("userID", (int)userlogin.UserId);
                return RedirectToAction("Home");
            }
            // Validation failed: Return in Index
            return View("Index");
        }

        // ============================================
        //               GET Logout
        // ============================================
        // GET: /logout
        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            // Clear session - Redirect to Index
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }


    }
}
