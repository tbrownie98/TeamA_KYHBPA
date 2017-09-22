﻿using KYHBPA_TeamA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace KYHBPA_TeamA.Controllers
{
    public class PhotoController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Photo
        public ActionResult Index()
        {
            var photos = db.Photos.Select(p => new DisplayPhotosViewModel()
            {
                Data = p.PhotoData,
                Description = p.PhotoDesc,
                Title = p.PhotoTitle,
                Date = p.TimeStamp
            });

            return View(photos);
        }

        // GET: Photo/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }


        // GET
        public ActionResult Create()
        {
            return View();
        }

        // POST: Photo/Create
        [HttpPost]
        public ActionResult Create(AddPhotoViewModel addViewModel, HttpPostedFileBase image = null)
        {
            if (ModelState.IsValid)
            {
                var photo = new Photo()
                {
                    TimeStamp = DateTime.Now,
                    PhotoDesc = addViewModel.Description,
                    PhotoData = new byte[image.ContentLength],
                    PhotoTitle = addViewModel.Title,
                };
                db.Photos.Add(photo);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        // POST: Photo/Create
        //[HttpPost]
        //public ActionResult Create(FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        // GET: Photo/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Photo/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Photo/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Photo/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

    }
}