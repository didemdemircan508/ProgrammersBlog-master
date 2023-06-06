using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProgrammersBlog.Entities.Dtos;
using ProgrammersBlog.Mvc.Areas.Admin.Models;
using ProgrammersBlog.Services.Abstract;
using ProgrammersBlog.Shared.Utilities.Extensions;
using ProgrammersBlog.Shared.Utilities.Results.ComplexTypes;
using ProgrammersBlog.Shared.Utilities.Results.Concrete;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgrammersBlog.Mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
  
    public class CategoryController : Controller
    {
       

        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _categoryService.GetAllByNoneDeleted();
             return View(result.Data);
            
            
        }
        [HttpGet]
        public IActionResult Add()
        {
            return PartialView("_CategoryAddPartial");

        
        }

        [HttpGet]
        public async Task<IActionResult> Update(int categoryId)
        {

            var result = await _categoryService.GetCategoryUpdateDeto(categoryId);
            if (result.ResultStatus == ResultStatus.Success)
            {
                return PartialView("_CategoryUpdatePartial", result.Data);
            }
            else
            {
                return NotFound();
            
            }
           



        }

        [HttpPost]
        public async Task<IActionResult> Update(CategoryUpdateDto categoryUpdateDto)
        {

            if (ModelState.IsValid)
            {
                var result = await _categoryService.Update(categoryUpdateDto, "Didem Demircan");
                if (result.ResultStatus == ResultStatus.Success)
                {
                    var categoryUpdateAjaxModel = System.Text.Json.JsonSerializer.Serialize(new CategoryUpdateAjaxViewModel
                    {
                        CategoryDto = result.Data,
                        CategoryUpdatePartial = await this.RenderViewToStringAsync("_CategoryUpdatePartial", categoryUpdateDto)

                    });
                    return Json(categoryUpdateAjaxModel);

                }
            }

            var categoryUpdateAjaxErrorModel = System.Text.Json.JsonSerializer.Serialize(new CategoryUpdateAjaxViewModel
            {
                CategoryUpdatePartial = await this.RenderViewToStringAsync("_CategoryUpdatePartial", categoryUpdateDto)

            });

            return Json(categoryUpdateAjaxErrorModel);


        }


        [HttpPost]
        public async Task<IActionResult> Add(CategoryAddDto categoryAddDto)
        {

            if (ModelState.IsValid)
            
            {
                var result = await _categoryService.Add(categoryAddDto, "Didem Demircan");
                if (result.ResultStatus == ResultStatus.Success)
                {
                    var categoryAddAjaxModel = System.Text.Json.JsonSerializer.Serialize(new CategoryAddAjaxViewModel
                    {
                        CategoryDto = result.Data,
                        CategoryAddPartial = await this.RenderViewToStringAsync("_CategoryAddPartial", categoryAddDto)

                    }) ;
                    return Json(categoryAddAjaxModel);
                
                }
            }

            var categoryAddAjaxErrorModel = System.Text.Json.JsonSerializer.Serialize(new CategoryAddAjaxViewModel
            {
               CategoryAddPartial = await this.RenderViewToStringAsync("_CategoryAddPartial", categoryAddDto)

            });

            return Json(categoryAddAjaxErrorModel);


        }

        public async Task<JsonResult> GetAllCategories()
        {

            var result = await _categoryService.GetAllByNoneDeleted();
            var categories = System.Text.Json.JsonSerializer.Serialize(result.Data, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            
            });

            return Json(categories);        
        }

        [HttpPost]
        public async Task<JsonResult> Delete(int categoryId)
        {
            var result = await _categoryService.Delete(categoryId, "Didem Demircan");
            var deletedCategory = System.Text.Json.JsonSerializer.Serialize(result.Data);
            return Json(deletedCategory);
       
        }
    }
}
