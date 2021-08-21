using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
// to use sessions
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
// for password hashing
using Microsoft.AspNetCore.Identity;
using WeddingPlanner.Models;

// Other using statements
namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;

        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            _context = context;
        }
        // Register Home page
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        // get the Login page
        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }

        // Get dashboard
        [HttpGet("Dashboard")]
        // user passed in from Log in
        public IActionResult Dashboard()
            {
                // get the user Id in session
                int?  userId = HttpContext.Session.GetInt32("userId"); 
             
                // grab the current user
               var UserInDb =  _context.Users.FirstOrDefault(user => user.UserId == userId );
               ViewBag.SpecificUser = UserInDb;

                // get all the wedding in the db
                ViewBag.AllWeddings = _context.Weddings;
                // weddings this user is going to 
                var test = _context.Weddings
                    .Include(wed => wed.UsersInThisWedding)
                    .Where(wed => wed.UsersInThisWedding.All(c => c.UserId == userId)).ToList();

                return View();
            }

        
        [HttpGet("AddWedding/{currentUserId}")]
        // current
        public IActionResult AddWedding(int currentUserId)
            
            {
                // var ThisUser = _context.Users.FirstOrDefault(b => b.UserId == currentUserId);
                ViewBag.CurrentUserId = currentUserId ;
                // ViewBag.ThisUser = ThisUser;
                return View();
            }

        // Get one wedding with all its guest attending 
        [HttpGet("OneWedding/{weddingId}")]
        public IActionResult OneWedding(int weddingId)
            {
                // get the wedding in the db with the weddingId passed in
                ViewBag.SpecificWedding = _context.Weddings
                    .FirstOrDefault(w => w.WeddingId == weddingId);
                // Give me all the guest in this wedding
                ViewBag.GuestInThisWedding = _context.Users
                    .Include(user => user.UsersWeddings)
                    .Where(user => user.UsersWeddings.All(u => u.WeddingId != weddingId));

                return View();
            }

        // Delete a wedding
        [HttpGet("Delete/{weddingId}")]
        public IActionResult DeleteWedding(int weddingId)
            {
                // find the wedding want to delete 
                Wedding RetrievedWedding = _context.Weddings
                    .SingleOrDefault(wed => wed.WeddingId == weddingId);
                // get the usersId in that wedding to put in dashboard redirect
                var LoggedInUser = _context.Users.FirstOrDefault(u => u.UserId == RetrievedWedding.UserId);

                    _context.Weddings.Remove(RetrievedWedding);
                    _context.SaveChanges();
                    return RedirectToAction("Dashboard", LoggedInUser);

            }
        
        // add user to a wedding aka RSVP
        [HttpPost("AddUserToWedding")]
        public IActionResult AddUserToWedding(Association newGuestRsvp)
            {
                // find a user in the users table to redirect to dash after
                var userInDb = _context.Users.FirstOrDefault(u => u.UserId == newGuestRsvp.UserId);
                _context.Associations.Add(newGuestRsvp);
                _context.SaveChanges();
                return RedirectToAction("Dashboard", userInDb);

            }

        
        // Add wedding post
        [HttpPost("NewWeddingPost")]
        // 
        public IActionResult NewWeddingPost(Wedding newWedding)
            {
                // Dashboard will need a user
                var userInDb = _context.Users.FirstOrDefault(u => u.UserId == newWedding.UserId);
                _context.Add(newWedding);
                _context.SaveChanges();
                return RedirectToAction("Dashboard", userInDb);
            }

        // Register logic
        [HttpPost("Register")]
        public IActionResult Register(User user)
        {
            // Check initial ModelState
            if (ModelState.IsValid)
            {
                // If a User exists with provided email
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");

                    // You may consider returning to the View at this point
                    return View("Index");
                }
                // if everything is okay save the user to the DB
                // Initializing a PasswordHasher object, providing our User class as its type
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                _context.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            // other code
            return View("Index");
        }

        // Login Post 
        [HttpPost("LoginPost")]
        public IActionResult LoginPost(LoginUser userSubmission)
        {
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("Password", "Not the right password cops are being called!");
                }
                // assign user ID to sessions
                HttpContext.Session.SetInt32("userId", userInDb.UserId);
                // If everything is good go to the Dashboard view page 
                // pass in the user we found in the db into it
                return RedirectToAction("Dashboard");
            }
            // go back to login if fails
            return View("Login");
        }

        // add user to attend wedding
        

    }
}