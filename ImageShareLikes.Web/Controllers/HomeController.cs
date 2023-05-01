using ImageShareLikes.Data;
using ImageShareLikes.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using Image = ImageShareLikes.Data.Image;

namespace ImageShareLikes.Web.Controllers
{
    public class HomeController : Controller
    {
        private IWebHostEnvironment _environment;
        private string _connectionString;

        public HomeController(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _connectionString = configuration.GetConnectionString("ConStr");
        }
        public IActionResult Index()
        {

            var repo = new ImageRepo(_connectionString);
            return View(new HomePageViewModel
            {
                Images = repo.GetImages().OrderByDescending(I => I.Date).ToList()
            });
        }
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Upload(Image image, IFormFile imageFile)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            string fullPath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
            using var stream = new FileStream(fullPath, FileMode.CreateNew);
            imageFile.CopyTo(stream);
            image.FileName = fileName;
            
            var repo = new ImageRepo(_connectionString);
            
            repo.AddImage(image);
            return Redirect("/");
        }
        public IActionResult ViewImage(int id)
        {
            var repo = new ImageRepo(_connectionString);
            var vm = new ViewImageViewModel
            {
                Image = repo.GetById(id)
            };
            var ids = HttpContext.Session.Get<List<int>>("ids");
            if (ids != null && ids.Contains(vm.Image.Id))
            {
                vm.Liked = true;
            }
            return View(vm);
        }
        [HttpPost]
        public void AddLike(int id)
        {

            var repo = new ImageRepo(_connectionString);
            repo.UpdateLikes(id);
            var ids = HttpContext.Session.Get<List<int>>("ids");

            if (ids == null)
            {
                ids = new List<int>();
            }
            ids.Add(id);
            HttpContext.Session.Set<List<int>>("ids", ids);
        }
        public IActionResult GetLikes(int id)
        {
            var repo = new ImageRepo(_connectionString);
            var image = repo.GetById(id);
            return Json(image.Likes);
        }
       
    }
      
}