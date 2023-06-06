using AutoMapper;
using ProgrammersBlog.Data.Abstract;
using ProgrammersBlog.Entities.Concrete;
using ProgrammersBlog.Entities.Dtos;
using ProgrammersBlog.Services.Abstract;
using ProgrammersBlog.Shared.Utilities.Results.Abstract;
using ProgrammersBlog.Shared.Utilities.Results.ComplexTypes;
using ProgrammersBlog.Shared.Utilities.Results.Concrete;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammersBlog.Services.Concrete
{
    public class CategoryManager : ICategoryService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryManager(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            
        }
        public async Task<IDataResult<CategoryDto>> Add(CategoryAddDto categoryAddDto, string createdByName)
        {
            //await _unitOfWork.Categories.AddAsync(new Category
            //{
            //    Name = categoryAddDto.Name,
            //    Description = categoryAddDto.Description,
            //    Note = categoryAddDto.Note,
            //    IsActive = categoryAddDto.IsActive,
            //    CreatedByName = createdByName,
            //    CreatedDate = DateTime.Now,
            //    ModifiedByName = createdByName,
            //    ModifiedDate = DateTime.Now,
            //    IsDeleted = false


            //}).ContinueWith(t=>_unitOfWork.SaveAsync()); automapper kullandığımız için   burayı sılıyoruz

           

            var category = _mapper.Map<Category>(categoryAddDto);
            category.CreatedByName = createdByName;
            category.ModifiedByName = createdByName;
            //await _unitOfWork.Categories.AddAsync(category).ContinueWith(t => _unitOfWork.SaveAsync());
            var addedCatogry=await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveAsync();
            return new DataResult<CategoryDto>(ResultStatus.Success, $"{categoryAddDto.Name} adlı kategori Başarıyla Eklenmiştir",new CategoryDto
            {
                Category = addedCatogry,
                ResultStatus = ResultStatus.Success,
                Message = $"{categoryAddDto.Name} adlı kategori Başarıyla Eklenmiştir"


            });
        }

        public async Task<IDataResult<CategoryDto>> Delete(int categoryId,string modifiedByName)
        {
            var category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId);
            if (category != null)
            {
                category.IsDeleted = true;
                category.ModifiedByName = modifiedByName;
                category.ModifiedDate = DateTime.Now;

                var deletedCategory = await _unitOfWork.Categories.UpdateAsync(category);
                await _unitOfWork.SaveAsync();
                return new DataResult<CategoryDto>(ResultStatus.Success, $"{deletedCategory.Name} Başarıyla Silnmiştir.", new CategoryDto
                {
                    Category = deletedCategory,
                    ResultStatus = ResultStatus.Success,
                    Message = $"{deletedCategory.Name} Başarıyla Silinmiştir"
                });
            }
            return new DataResult<CategoryDto>(ResultStatus.Error, "Böyle Bir Kategori Bulunamadı", new CategoryDto
            {
                Category = null,
                ResultStatus = ResultStatus.Error,
                Message = "Böyle Bir Kategori Bulunamadı"
            });
        }

        public async Task<IDataResult<CategoryDto>> Get(int categoryId)
        {
          var category =  await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId, c => c.Articles);
            if (category != null)
            {
                return new DataResult<CategoryDto>(ResultStatus.Success, new CategoryDto
                {
                    Category=category,
                    ResultStatus =ResultStatus.Success


                });
            
            }
            return new DataResult<CategoryDto>(ResultStatus.Error, "Böyle Bir Kategori Bulunamadı",new CategoryDto
            {
                Category = null,
                ResultStatus =ResultStatus.Error,
                Message = "Böyle Bir Kategori Bulunamadı"

            });
        }



        public async Task<IDataResult<CategoryListDto>> GetAll()
        {
          var categories =  await _unitOfWork.Categories.GetAllAsync(null,c=>c.Articles);
          
            var x = categories.Count();
            if (categories.Count > -1)
            {
                return new DataResult<CategoryListDto>(ResultStatus.Success, new CategoryListDto
                {
                    
                    Categories = categories,
                    ResultStatus = ResultStatus.Success
                });
               
            
            }

            return new DataResult<CategoryListDto>(ResultStatus.Error, "Hiç Bir Kategori Bulunamadı",new CategoryListDto{ 
                Categories=null,
                ResultStatus = ResultStatus.Error,
                Message = "Hiç Bir Kategori Bulunamadı"
            });

            
        }

        public async Task<IDataResult<CategoryListDto>> GetAllByNoneDeleted()
        {
            //var categories = await _unitOfWork.Categoies.GetAllAsync(c => c.IsDeleted == false);
            var categories = await _unitOfWork.Categories.GetAllAsync(c => !c.IsDeleted,c=>c.Articles);

            if (categories.Count > -1)
            {
                return new DataResult<CategoryListDto>(ResultStatus.Success, new CategoryListDto
                {

                    Categories = categories,
                    ResultStatus = ResultStatus.Success
                });


            }

            return new DataResult<CategoryListDto>(ResultStatus.Error, "Hiç Bir Kategori Bulunamadı", new CategoryListDto
            {
                Categories = null,
                ResultStatus = ResultStatus.Error,
                Message = "Hiç Bir Kategori Bulunamadı"
            });
        }

        public async Task<IDataResult<CategoryListDto>> GetAllByNoneDeletedAndActive()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(c => !c.IsDeleted && c.IsActive, c => c.Articles);
            if (categories.Count > -1) {
                return new DataResult<CategoryListDto>(ResultStatus.Success, new CategoryListDto
               { 
                    Categories =categories,
                    ResultStatus =ResultStatus.Success
                });
            
            }
            return new  DataResult<CategoryListDto>(ResultStatus.Error, "Hiç Bir Kategori Bulunamadı",null);
        }

        public async Task<IDataResult<CategoryUpdateDto>> GetCategoryUpdateDeto(int categoryId)
        {
            var result = await _unitOfWork.Categories.AnyAsync(c => c.Id == categoryId);
            if (result)
            {
                var category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId);
                var categoryUpdateDto = _mapper.Map<CategoryUpdateDto>(category);

                return new DataResult<CategoryUpdateDto>(ResultStatus.Success, categoryUpdateDto);

            }
            else {
                return new DataResult<CategoryUpdateDto>(ResultStatus.Error, "Böyle Bir kategori Bulunmadı",null);

            }

        }

        public  async Task<IResult> HardDelete(int categoryId)
        {
            var category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryId);
            if (category != null)
            {
                await _unitOfWork.Categories.DeleteAsync(category);
                await _unitOfWork.SaveAsync();
                return new Result(ResultStatus.Success, $"{category.Name} adlı Kategori Başarıyla veri tabanından silindi");
            
            }

            return new Result(ResultStatus.Error, "Böyle bir kategori Bulunamadı");
        }

        public async Task<IDataResult<CategoryDto>> Update(CategoryUpdateDto categoryUpdateDto, string modifiedByName)
        {
            //var category = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryUpdateDto.Id);
            //if (category != null)
            //{
            //    category.Name = categoryUpdateDto.Name;
            //    category.Description = categoryUpdateDto.Description;
            //    category.Note = categoryUpdateDto.Note;
            //    category.IsActive = categoryUpdateDto.IsActive;
            //    category.IsDeleted = categoryUpdateDto.IsDeleted;
            //    category.ModifiedByName = modifiedByName;
            //    category.ModifiedDate = DateTime.Now;
            //    await _unitOfWork.Categories.UpdateAsync(category).ContinueWith(t => _unitOfWork.SaveAsync());

            //    return new Result(ResultStatus.Success, $"{categoryUpdateDto.Name} Başarıyla Güncellenmiştir");


            // 

            var oldCategory = await _unitOfWork.Categories.GetAsync(c => c.Id == categoryUpdateDto.Id);

            var category = _mapper.Map<CategoryUpdateDto, Category>(categoryUpdateDto,oldCategory);
            category.ModifiedByName = modifiedByName;
            
            var updatedCategory=  await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveAsync();
            return new DataResult<CategoryDto>(ResultStatus.Success, $"{categoryUpdateDto.Name} Başarıyla Güncellenmiştir", new CategoryDto
            {
                Category = updatedCategory,
                ResultStatus = ResultStatus.Success,
                Message = $"{categoryUpdateDto.Name} Başarıyla Güncellenmiştir"
            }) ;
        }

       

    }
}
