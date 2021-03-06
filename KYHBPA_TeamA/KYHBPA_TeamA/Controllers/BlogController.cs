﻿using KYHBPA_TeamA.Models;
using KYHBPA_TeamA.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Web.UI;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Imaging;

namespace KYHBPA_TeamA.Controllers
{
    public class BlogController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        private BlogRepository _blogRepository = new BlogRepository();

        // GET: Blog
        public ActionResult Index()
        {
            var allPosts = _db.Posts.Select(post => new DisplayPostsViewModel()
            {
                Id = post.Id,
                Title = post.Title,
                ShortDescription = post.ShortDescription,
                Description = post.Description,
                Published = post.Published,
                PostedOn = post.PostedOn,
                Category = post.Category,
                Comments = post.Comments
            }).Where(x => x.Published == true).OrderByDescending(x => x.PostedOn);

            

            // When it is time to build a visitor-facing view,
            // you will pass this view all posts that have been published!
            return View(allPosts); 

        }

        // GET: Blog/Manage
        [Authorize(Roles = "Admin")]
        public ActionResult Admin()
        {
            var allPosts = _db.Posts.Select(post => new CreateBlogPostViewModel()
            {
                Id = post.Id,
                Title = post.Title,
                ShortDescription = post.ShortDescription,
                Description = post.Description,
                Published = post.Published,
                PostedOn = post.PostedOn,
                Category = post.Category,
                Comments = post.Comments,
                FrontPageFeature = post.FrontPageFeature
            });

            return View(allPosts);
        }

        // GET: Blog/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new CreateBlogPostViewModel() { PostedOn = DateTime.Now });
        }

        // GET: Blog/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create(CreateBlogPostViewModel viewModel, HttpPostedFileBase image = null)
        {

            if (ModelState.IsValid)
            {

                var category = _db.Categories.FirstOrDefault(c => c.Id == viewModel.SelectedCategoryId);
                var post = new Post();

                //Incase the selected category isn't in the database yet..
                if (category != null)
                    post.Category = category;
                else
                {
                    var selectedCategoryName = viewModel.CategoryList.ToList().FirstOrDefault(item => int.Parse(item.Value) == viewModel.SelectedCategoryId).Text;
                    _db.Categories.Add(new Category() { Name = selectedCategoryName });
                    _db.SaveChanges();

                    var categoryRecord = _db.Categories.FirstOrDefault(c => c.Id == viewModel.SelectedCategoryId);
                    post.Category = categoryRecord;
                }

                // if user uploaded image, process it
                if(image != null)
                {
                    post.MimeType = image.ContentType;
                    post.PhotoContent = new byte[image.ContentLength];

                    image.InputStream.Read(post.PhotoContent, 0, image.ContentLength);

                    var imageToResize = Image.FromStream(image.InputStream);
                    post.ThumbnailPhotoContent = GetImageThumbnail(imageToResize);

                }


                post.Description = viewModel.Description;
                post.ShortDescription = viewModel.ShortDescription;
                post.Modified = viewModel.Modified;
                post.Published = viewModel.Published;
                post.PostedOn = viewModel.PostedOn;
                post.Tags = viewModel.Tags;
                post.Title = viewModel.Title;
                post.FrontPageFeature = viewModel.FrontPageFeature;

                _db.Posts.Add(post);
                _db.SaveChanges();
                return RedirectToAction("Admin");
            }
            else
            {
                return View(viewModel);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var post = _db.Posts.FirstOrDefault(p => p.Id == id);

            var viewModel = new CreateBlogPostViewModel()
            {
                Id = post.Id,
                Description = post.Description,
                ShortDescription = post.ShortDescription,
                Category = post.Category,
                Modified = post.Modified,
                PostedOn = post.PostedOn,
                Published = post.Published,
                Title = post.Title,
                SelectedCategoryId = post.Category.Id,
                FrontPageFeature = post.FrontPageFeature
            };

            return View(viewModel);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CreateBlogPostViewModel editedPost, FormCollection collection, HttpPostedFileBase image = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var postToUpdate = _db.Posts.FirstOrDefault(x => x.Id == editedPost.Id);
                    if (postToUpdate != null)
                    {
                        postToUpdate.Title = editedPost.Title;
                        postToUpdate.ShortDescription = editedPost.ShortDescription;
                        postToUpdate.Description = editedPost.Description;
                        postToUpdate.Published = editedPost.Published;
                        postToUpdate.Modified = DateTime.Today.Date;
                        postToUpdate.FrontPageFeature = editedPost.FrontPageFeature;

                        var newCategory = _db.Categories.FirstOrDefault(c => c.Id == editedPost.SelectedCategoryId);

                        if (postToUpdate.Category != newCategory)
                        {
                            postToUpdate.Category = newCategory;
                        }               

                        if (image != null)
                        {
                            postToUpdate.PhotoContent = new byte[image.ContentLength];
                            image.InputStream.Read(postToUpdate.PhotoContent, 0, image.ContentLength);

                            postToUpdate.MimeType = image.ContentType;


                            var imageToResize = Image.FromStream(image.InputStream);
                            postToUpdate.ThumbnailPhotoContent = GetImageThumbnail(imageToResize);

                        }
                    }


                    _db.Entry(postToUpdate).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    TempData["message"] = string.Format($"Article with title: {editedPost.Title} has been updated!");
                    return RedirectToAction("Admin");
                }

                return RedirectToAction("Admin");
            }
            catch
            {
                return View();
            }
        }

        // GET: Blog/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var post = _db.Posts.Find(id);
            var vm = new DisplayPostsViewModel()
            {
                Id = post.Id,
                Description = post.Description,
                ShortDescription = post.ShortDescription,
                Category = post.Category,
                PostedOn = post.PostedOn,
                Published = post.Published,
                Title = post.Title,
            };

            if (post == null)
                return HttpNotFound();

            return View(vm);
        }
        // POST: Blog/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                var post = _db.Posts.Find(id);

                // remove comment dependency by getting all comments associated
                // with the post, nulling the post Id, and then deleting them
                var comments = _db.Comments.Where(x => x.Post.Id == post.Id);

                foreach(var comment in comments)
                {
                    comment.Post = null;
                }

                _db.Posts.Remove(post);
                _db.Comments.RemoveRange(comments);
                _db.SaveChanges();
                return RedirectToAction("Admin");
            }
            catch
            {
                return View();
            }
        }


        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpNotFoundResult();
            }
            else
            {
                var post = _db.Posts.FirstOrDefault(p => p.Id == id);

                var viewModel = new BlogPostDetailsViewModel()
                {
                    Id = post.Id,
                    Description = post.Description,
                    ShortDescription = post.ShortDescription,
                    Category = post.Category,
                    Modified = post.Modified,
                    PostedOn = post.PostedOn,
                    Published = post.Published,
                    Title = post.Title,
                    Comments = post.Comments,
                    //Could lead to possible bug if seed order doesn't stay the same.
                    SelectedCategoryId = post.Category.Id
                };

                return View(viewModel);
            }
        }

        public ActionResult ViewByCategory(int Id)
        {
            // pick latest 10 posts by category
            var posts = _blogRepository.Posts(0, 10).Where(p => p.Category.Id == Id).ToList();

            var totalPosts = _blogRepository.TotalPosts();

            var listViewModel = new BlogListViewModel()
            {
                Posts = posts,
                TotalPosts = totalPosts
            };

            ViewBag.Title = "Latest Posts by Category";

            return View("List", listViewModel);
        }

        [Authorize(Roles = "Admin,Employee,Member,User")]
        [HttpGet]
        public ActionResult CreateComment(int Id)
        {
            var post = _db.Posts.FirstOrDefault(p => p.Id == Id);

            var viewModel = new CreateCommentViewModel()
            {
                Post = post,
                PostNumber = post.Id
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Employee,Member,User")]
        [HttpPost]
        public ActionResult CreateComment(CreateCommentViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var post = _db.Posts.FirstOrDefault(p => p.Id == viewModel.PostNumber);

                var comment = new Comment()
                {
                    Name = viewModel.Name,
                    Text = viewModel.Comment,
                    Post = post
                };

                _db.Comments.Add(comment);
                _db.SaveChanges();

                post.Comments.Add(comment);
                _db.SaveChanges();
            }

            return RedirectToAction("Read",new { postId = viewModel.PostNumber});
        }


        [OutputCache(Duration = 86400, Location = OutputCacheLocation.ServerAndClient)]
        public ActionResult GetBlogOrNewsImage(int id)
        {
            var post = _db.Posts.Find(id);

            if (post.MimeType != null && post.PhotoContent != null)
                return File(post.PhotoContent, post.MimeType);
            else
            {
                // if there is no image, is returning null best practice?
                // makes server handle error for 500 status code...
                HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                return null; 
            }
        }

        public ActionResult Read(int? postId)
        {
            var postToRead = _db.Posts.Find(postId);

            if (postId != null && postToRead != null)
            {
                var postViewModel = new DisplayPostsViewModel()
                {
                    Id = postToRead.Id,
                    Title = postToRead.Title,
                    ShortDescription = postToRead.ShortDescription,
                    Description = postToRead.Description,
                    PostedOn = postToRead.PostedOn,
                    Category = postToRead.Category,
                    Comments = postToRead.Comments
                };

                return View(postViewModel);
            }
            else
            {
                return HttpNotFound();
            }
        }

        /// <summary>
        /// Deletes comment
        /// </summary>
        /// <param name="id">Comment id</param>
        /// <returns>View from blog index</returns>
        public ActionResult DeleteComment(int? id, int postId)
        {
            if (id == null)
                return new HttpNotFoundResult();
            else
            {
                var commentToDelete = _db.Comments.Find(id);

                if (commentToDelete == null)
                    return new HttpNotFoundResult();
                else
                {
                    _db.Comments.Remove(commentToDelete);
                    _db.SaveChanges();
                    return RedirectToAction("Read", new { postId });
                }
            }
        }

        public byte[] GetImageThumbnail(Image image)
        {
            byte[] returnArray;

            var thumbSize = new Size()
            {
                Width = 500,
                Height = 500
            };

            int sourceWidth = image.Width;
            int sourceHeight = image.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)thumbSize.Width / (float)sourceWidth);
            nPercentH = ((float)thumbSize.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.CompositingMode = CompositingMode.SourceCopy;

            g.DrawImage(image, 0, 0, destWidth, destHeight);
            g.Dispose();

            using (MemoryStream ms = new MemoryStream())
            {
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 80L);
                b.Save(ms, getEncoderInfo("image/jpeg"), encoderParameters);

                //save the stream as byte array
                returnArray = ms.ToArray();
            }

            return returnArray;
        }

        private static ImageCodecInfo getEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            for (int j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType.ToLower() == mimeType.ToLower())
                    return encoders[j];
            }
            return null;
        }

        /// <summary>
        /// Returns thumbnail of photo
        /// </summary>
        /// <param name="id">Id of the photo</param>
        /// <returns>Returns file</returns>
        [OutputCache(Duration = 86400, Location = OutputCacheLocation.ServerAndClient)]
        public ActionResult GetBlogThumbnail(int? id)
        {
            if (id == null)
                return new HttpNotFoundResult();

            var blogToGet = _db.Posts.Find(id);

            if (blogToGet != null && blogToGet.ThumbnailPhotoContent != null)
                return File(blogToGet.ThumbnailPhotoContent, blogToGet.MimeType);
            else
                return new HttpNotFoundResult();

        }

        
        //public ActionResult UpdateAllImagesWithoutThumbnails()
        //{
        //    var blogsToUpdate = _db.Posts.Where(x => x.ThumbnailPhotoContent == null);

        //    foreach (var blog in blogsToUpdate)
        //    {
        //        if (blog.PhotoContent != null)
        //        {
        //            Image newImage = (Image)new ImageConverter().ConvertFrom(blog.PhotoContent);
        //            var resizedImage = GetImageThumbnail(newImage);
        //            blog.ThumbnailPhotoContent = resizedImage;
        //        }
        //    }

        //    if (blogsToUpdate != null)
        //    {
        //        _db.SaveChanges();

        //    }

        //    TempData["message"] = "Thumbnails successfully updated";
        //    return RedirectToAction("Admin");
        //}

    }
}