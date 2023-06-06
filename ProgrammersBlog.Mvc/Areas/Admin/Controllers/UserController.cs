using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProgrammersBlog.Entities.Concrete;
using ProgrammersBlog.Entities.Dtos;
using ProgrammersBlog.Mvc.Areas.Admin.Models;
using ProgrammersBlog.Shared.Utilities.Extensions;
using ProgrammersBlog.Shared.Utilities.Results.ComplexTypes;
using System.Text.Json.Serialization;

namespace ProgrammersBlog.Mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
  

    public class UserController : Controller
    {
       

        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _env;
        public readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;

        public UserController (UserManager<User> userManager, IWebHostEnvironment env,IMapper mapper,SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _env = env;
            _mapper = mapper;
            _signInManager = signInManager;
        }

        //[Authorize(Roles ="Admin")]
        public  async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(new UserListDto { 
              Users= users,
              ResultStatus = ResultStatus.Success

            
            });
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View("UserLogin");
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(userLoginDto.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, userLoginDto.Password, userLoginDto.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "E-posta adresiniz veya şifreniz yanlıştır");
                        return View("UserLogin");
                    }


                }

                else
                {
                    ModelState.AddModelError("", "E-posta adresiniz veya şifreniz yanlıştır");
                    return View("UserLogin");
                }
            }

            else
            {
                return View("UserLogin");
            }
            
        }

        //[Authorize]
        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { Area = "" });

        }

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userListDto=System.Text.Json.JsonSerializer.Serialize (new UserListDto
            {
                Users = users,
                ResultStatus = ResultStatus.Success


            },new System.Text.Json.JsonSerializerOptions { 
                ReferenceHandler = ReferenceHandler.Preserve
            
            }
            );

            return Json(userListDto);
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {

            return PartialView("_UserAddPartial");
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add(UserAddDto userAddDto)
        {
            
            if (ModelState.IsValid)
            {
                userAddDto.Picture = await ImageUploud(userAddDto.UserName,userAddDto.PictureFile);
                var user = _mapper.Map<User>(userAddDto);
                var result = await _userManager.CreateAsync(user, userAddDto.Password);
                if (result.Succeeded)
                {
                    var userAddAjaxModel = System.Text.Json.JsonSerializer.Serialize(new UserAddAjaxViewModel
                    {
                        UserDto = new UserDto
                        {
                            ResultStatus = ResultStatus.Success,
                            Message = $"{user.UserName} adlı kullanıcı adına sahip, kullanıcı başarıyla eklenmiştir.",
                            User = user
                        },
                        UserAddPartial = await this.RenderViewToStringAsync("_UserAddPartial", userAddDto)
                    });
                    return Json(userAddAjaxModel);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    var userAddAjaxErrorModel = System.Text.Json.JsonSerializer.Serialize(new UserAddAjaxViewModel
                    {
                        UserAddDto = userAddDto,
                        UserAddPartial = await this.RenderViewToStringAsync("_UserAddPartial", userAddDto)
                    });
                    return Json(userAddAjaxErrorModel);
                }

            }
            var userAddAjaxModelStateErrorModel = System.Text.Json.JsonSerializer.Serialize(new UserAddAjaxViewModel
            {
                UserAddDto = userAddDto,
                UserAddPartial = await this.RenderViewToStringAsync("_UserAddPartial", userAddDto)
            });
            return Json(userAddAjaxModelStateErrorModel);

        }

        [HttpGet]
        public ViewResult AccessDenied() {
            return View();
        
        }

        //[Authorize(Roles = "Admin")]
        public async Task<JsonResult> Delete(int userId)
        {

            var user = await _userManager.FindByIdAsync(userId.ToString());
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                var deletedUser = System.Text.Json.JsonSerializer.Serialize(new UserDto
                {
                    ResultStatus = ResultStatus.Success,
                    Message =$"{user.UserName} adlı kullanıcı adına sahip kullanıcı başarıyla silinmiştir",
                    User=user

                });

                return Json(deletedUser);
            }
            else
            {
                string errorMessages = string.Empty;
                foreach (var error in result.Errors) {
                    errorMessages=$"*{error.Description}\n";
                
                }
                var deletedUserErrorModel = System.Text.Json.JsonSerializer.Serialize(new UserDto
                {
                    ResultStatus= ResultStatus.Error,
                    Message = $"{user.UserName} adlı kullanıcı adına sahip kullanıcı silinirken hata oluştu\n{errorMessages}",
                    User=user


                });
                return Json(deletedUserErrorModel);
            }
        
        }


        //[Authorize(Roles = "Admin")]
        [HttpGet]

        public async Task<PartialViewResult> Update(int userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var userUpdateDto = _mapper.Map<UserUpdateDto>(user);
            return PartialView("_UserUpdatePartial", userUpdateDto);


        
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Update(UserUpdateDto userUpdateDto)
        {
            if (ModelState.IsValid)
            {
                bool isNewPictureUploaded = false;
                var oldUser = await _userManager.FindByIdAsync(userUpdateDto.Id.ToString());
                var oldUserPicture = oldUser.Picture;
                if (userUpdateDto.PictureFile != null)
                {
                    userUpdateDto.Picture = await ImageUploud(userUpdateDto.UserName, userUpdateDto.PictureFile);
                    isNewPictureUploaded = true;


                }
                var updatedUser = _mapper.Map<UserUpdateDto, User>(userUpdateDto, oldUser);
                var result = await _userManager.UpdateAsync(updatedUser);
                if (result.Succeeded)
                {

                    if (isNewPictureUploaded)
                    {
                        ImageDelete(oldUserPicture);


                    }
                    var userUpdateViewModel = System.Text.Json.JsonSerializer.Serialize(new UserUpdateAjaxViewModel
                    {
                        UserDto = new UserDto
                        {
                            ResultStatus = ResultStatus.Success,
                            Message = $"{updatedUser.UserName} adlı kullanıı başarıyla güncellenmiştir",
                            User = updatedUser


                        },
                        UserUpdatePartial = await this.RenderViewToStringAsync("_UserUpdatePartial", userUpdateDto)


                    });
                    return Json(userUpdateViewModel);

                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    var userUpdateErorViewModel = System.Text.Json.JsonSerializer.Serialize(new UserUpdateAjaxViewModel
                    {

                        UserUpdateDto = userUpdateDto,
                        UserUpdatePartial = await this.RenderViewToStringAsync("_UserUpdatePartial", userUpdateDto)


                    });
                    return Json(userUpdateErorViewModel);

                }


            }
            else
            {

                var userUpdateModelStateErrorViewModel = System.Text.Json.JsonSerializer.Serialize(new UserUpdateAjaxViewModel
                {

                    UserUpdateDto = userUpdateDto,
                    UserUpdatePartial = await this.RenderViewToStringAsync("_UserUpdatePartial", userUpdateDto)


                });
                return Json(userUpdateModelStateErrorViewModel);


            }
        
        }
        //[Authorize]
        [HttpGet]
        public async Task<ViewResult> ChangeDetails()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var updateDto = _mapper.Map<UserUpdateDto>(user);
            return View(updateDto);


        
        }

        //[Authorize]
        [HttpPost]
        public async Task<ViewResult> ChangeDetails(UserUpdateDto userUpdateDto)
        {

            if (ModelState.IsValid)
            {
                bool isNewPictureUploaded = false;
                var oldUser = await _userManager.GetUserAsync(HttpContext.User);
                var oldUserPicture = oldUser.Picture;
                if (userUpdateDto.PictureFile != null)
                {
                    userUpdateDto.Picture = await ImageUploud(userUpdateDto.UserName, userUpdateDto.PictureFile);
                    if (oldUserPicture != "defaultUser.png")
                    {
                        isNewPictureUploaded = true;
                    }
  

                }
                var updatedUser = _mapper.Map<UserUpdateDto, User>(userUpdateDto, oldUser);
                var result = await _userManager.UpdateAsync(updatedUser);
                if (result.Succeeded)
                {

                    if (isNewPictureUploaded)
                    {
                        ImageDelete(oldUserPicture);
                    }
                    TempData.Add("SuccessMessage", $"{updatedUser.UserName} adlı kullanıı başarıyla güncellenmiştir");
                    return View(userUpdateDto);

                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }


                    return View(userUpdateDto);

                }


            }
            else
            {
                return View(userUpdateDto);
           

            }





        }


        //[Authorize(Roles = "Admin,Editor")]
        public async Task<string> ImageUploud(string userName,IFormFile pictureFile)
        {
            string wwwroot = _env.WebRootPath;
            //didemdemircan
            //string fileName = Path.GetFileNameWithoutExtension(userAddDto.Picture.FileName);
            //png
            string fileExtension = Path.GetExtension(pictureFile.FileName);
            DateTime dateTime = DateTime.Now;
            //didemdemircan_587_5_38_12_3_10_2020.png

            string fileName = $"{userName}_{dateTime.FullDateAndTimeStringWithUnderScore()}{fileExtension}";
            var path = Path.Combine($"{wwwroot}/img", fileName);
            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await pictureFile.CopyToAsync(stream);
            }

            return fileName;
           



        }
        //[Authorize]
        [HttpGet]
        public ViewResult PasswordChange()
        {
            return View();
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> PasswordChange(UserPasswordChangeDto userPasswordChangeDto)
        {
            if (ModelState.IsValid) {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var isVerified = await _userManager.CheckPasswordAsync(user, userPasswordChangeDto.CurrentPassword);
                if (isVerified)
                {
                    var result = await _userManager.ChangePasswordAsync(user, userPasswordChangeDto.CurrentPassword, userPasswordChangeDto.NewPassword);
                    if (result.Succeeded)
                    {
                        await _userManager.UpdateSecurityStampAsync(user);
                        await _signInManager.SignOutAsync();
                        await _signInManager.PasswordSignInAsync(user, userPasswordChangeDto.NewPassword, true, false);
                        TempData.Add("SuccessMessage", "Şifreniz başarıyla değiştirilmiştir");
                        return View(result);

                    }

                }

                else
                {
                    ModelState.AddModelError("", "Lütfen Girmiş olduğunuz şu anki şifrenizi kontrol ediniz");
                return View(userPasswordChangeDto);
                }
            }
            else
            {
                return View(userPasswordChangeDto);

            }
            return View();
        }

        //[Authorize(Roles = "Admin,Editor")]
        public  bool ImageDelete(string pictureName)
        {
            string wwwroot = _env.WebRootPath;
            var fileToDelete = Path.Combine($"{wwwroot}/img", pictureName);
            if (System.IO.File.Exists(fileToDelete))
            {
                System.IO.File.Delete(fileToDelete);
                return true;


            }
            else
            {
                return false;
            
            }
        
        }

    }
}
