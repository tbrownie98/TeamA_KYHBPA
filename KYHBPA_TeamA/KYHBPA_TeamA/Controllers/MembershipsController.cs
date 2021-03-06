﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KYHBPA_TeamA.Models;
using Microsoft.AspNet.Identity;
using System.Net.Mail;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;

namespace KYHBPA_TeamA.Controllers
{
    public class MembershipsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        // GET: Memberships
        [Authorize]
        public async Task<ActionResult> Index()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user.AppliedForMembership)
            {

                var membership = user.Membership;


                return View(membership);
            }
            else
            {
                return View("Create");
            }

        }

        [Authorize(Roles = "Admin")]
        public ActionResult Admin()
        {
            return View(db.Memberships.ToList());
        }

        // GET: Memberships/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            Membership membership = new Membership();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (User.IsInRole("Admin"))
            {
                membership = db.Memberships.Find(id);
            }
            else
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                membership = db.Memberships.Find(user.Membership.ID);
            }

            if (membership == null)
            {
                return HttpNotFound();
            }
            else
            {
               return View(membership);
            }
        }

        // GET: Memberships/Create
        [Authorize]
        public ActionResult Create()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (user.AppliedForMembership)
            {
                return View("Index",user.Membership);
            }
            else
            {
                return View();
            }
                
        }

        // POST: Memberships/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MembershipsViewModels.CreateMembershipViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // retreives user 
                var user = db.Users.Find(User.Identity.GetUserId());

                if (user.AppliedForMembership == false)
                {

                    var membership = new Membership()
                    {
                        DateofBirth = viewModel.DateofBirth,
                        PhoneNumber = viewModel.PhoneNumber,
                        Address = viewModel.Address,
                        City = viewModel.City,
                        State = viewModel.State,
                        ZipCode = viewModel.ZipCode,
                        LicenseNumber = viewModel.LicenseNumber,
                        IsOwner = viewModel.IsOwner,
                        IsTrainer = viewModel.IsTrainer,
                        Affiliation = viewModel.Affiliation,
                        ManagingPartner = viewModel.ManagingPartner,
                        AgreedToTerms = viewModel.AgreedToTerms,
                        Signature = viewModel.Signature,
                        MembershipEnrollment = DateTime.Now
                    };

                    if (membership.IsOwner == true && membership.IsTrainer == true)
                    {
                        membership.IsOwnerAndTrainer = true;
                    }

                    user.AppliedForMembership = true;
                    user.Membership = membership;
                    db.Memberships.Add(membership);
                    db.SaveChanges();

                    // Send email after creating membership.
                    MailDefinition md = new MailDefinition()
                    {
                        IsBodyHtml = true,
                        Subject = "User created Membership at KentuckyHbpa.org",
                        BodyFileName = "~/Content/EmailTemplates/AdminNotificationOfMembership.html",
                        From = "no-reply@KentuckyHbpa.org"
                    };

                    string nullReplacement = string.Empty;
                    ListDictionary replacements = new ListDictionary();
                    replacements.Add("{firstName}", user.FirstName);
                    replacements.Add("{lastName}", user.LastName);
                    replacements.Add("{dateOfBirth}", membership.DateofBirth.ToString());
                    replacements.Add("{phoneNumber}", membership.PhoneNumber);
                    replacements.Add("{address}", membership.Address);
                    replacements.Add("{city}", membership.City);
                    replacements.Add("{state}", membership.State);
                    replacements.Add("{zipCode}", membership.ZipCode);
                    replacements.Add("{licenseNumber}", membership.LicenseNumber);
                    replacements.Add("{owner}", membership.IsOwner.ToString());
                    replacements.Add("{trainer}", membership.IsTrainer.ToString());
                    replacements.Add("{affiliation}", membership.Affiliation ?? nullReplacement);
                    replacements.Add("{managingParter}", membership.ManagingPartner ?? nullReplacement);
                    replacements.Add("{agreedToTerms}", membership.AgreedToTerms.ToString());
                    replacements.Add("{signature}", membership.Signature);

                    // Test using personal email
                    MailMessage email = md.CreateMailMessage("kentuckyhbpa@gmail.com", replacements, new System.Web.UI.Control());
                    MailMessage personalEmail = md.CreateMailMessage("pmgreg3@gmail.com", replacements, new System.Web.UI.Control());
                    MailMessage testEmail = md.CreateMailMessage("tracksidejennie@gmail.com", replacements, new System.Web.UI.Control());

                    using (SmtpClient emailClient = new SmtpClient("relay-hosting.secureserver.net",25)
                    {
                        UseDefaultCredentials = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        EnableSsl = false
                    })
                    {
                        await emailClient.SendMailAsync(email);
                        await emailClient.SendMailAsync(personalEmail);
                        await emailClient.SendMailAsync(testEmail);
                    }

                    return RedirectToAction("Index");

                }
                else
                {
                    return RedirectToAction("MembershipError");
                }
            }
            else
                return View();
        }

        public ActionResult MembershipError()
        {
            return View();
        }
    

        // GET: Memberships/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Membership membership = db.Memberships.Find(id);

            if (User.IsInRole("Admin"))
            {
                if (membership == null)
                {
                    return HttpNotFound();
                }
                return View(membership);
            }
            else
            {
                var currentUser = db.Users.Find(User.Identity.GetUserId());

                // is the requested membership not their own?
                if(currentUser.Membership.ID != id)
                {
                    TempData["bad-message"] = "Access Denied.";
                    return RedirectToAction("Index");
                }
                else //this is their membership
                {
                    if (membership == null)
                    {
                        return HttpNotFound();
                    }
                    return View(membership);
                }
            }

        }

        // POST: Memberships/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,DateofBirth,MembershipEnrollment,PhoneNumber,Address,City,State,ZipCode,LicenseNumber,IsOwner,IsTrainer,IsOwnerAndTrainer,AgreedToTerms,Signature,Affiliation,ManagingPartner")] Membership membership)
        {
            if (ModelState.IsValid)
            {
                db.Entry(membership).State = EntityState.Modified;
                db.SaveChanges();

                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Admin");
                }
                else
                {
                    return RedirectToAction("Index");
                }         
            }
            return View(membership);
        }

        // GET: Memberships/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            Membership membership = new Membership();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (User.IsInRole("Admin"))
            {
                membership = db.Memberships.Find(id);
            }
            else
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                membership = user.Membership;

                if(membership.ID != id)
                {
                    // if the membership id doesn't match the original request
                    // and the member isn't an admin, tell them they tried to access something they shouldn't have
                    TempData["message"] = "Access to requested membership information has been denied.";
                }
            }
            
            if (membership == null)
            {
                return HttpNotFound();
            }

            return View(membership);
        }

        // POST: Memberships/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Membership membership = db.Memberships.Find(id);
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    // gets user that the membership is associated with
                    var user = db.Users
                        .Include(x => x.Membership)
                        .SingleOrDefault(x => x.Membership.ID == membership.ID);

                    // nulls the membership and resets the flag
                    user.Membership = null;
                    user.AppliedForMembership = false;

                    db.Memberships.Remove(membership);
                    db.SaveChanges();
                    return RedirectToAction("Admin");
                }
                else
                {
                    var currentUser = db.Users.Find(User.Identity.GetUserId());

                    if (currentUser.Membership.ID != id)
                    {
                        TempData["bad-message"] = "Access Denied.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // nulls the membership and resets the flag
                        currentUser.Membership = null;
                        currentUser.AppliedForMembership = false;

                        db.Memberships.Remove(membership);
                        db.SaveChanges();

                        TempData["success-message"] = "Membership has been successfully deleted";
                        return RedirectToAction("Index");
                    }
                }
            }
            else
            {
                return HttpNotFound();
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
