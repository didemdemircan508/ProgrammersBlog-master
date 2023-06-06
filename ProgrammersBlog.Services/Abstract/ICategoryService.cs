using ProgrammersBlog.Entities.Concrete;
using ProgrammersBlog.Entities.Dtos;
using ProgrammersBlog.Shared.Utilities.Results.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammersBlog.Services.Abstract
{
    public interface ICategoryService
    {

        Task<IDataResult<CategoryDto>> Get(int categoryId);

        Task<IDataResult<CategoryUpdateDto>> GetCategoryUpdateDeto(int categoryId);
        Task<IDataResult<CategoryListDto>> GetAll();
        Task<IDataResult<CategoryListDto>> GetAllByNoneDeleted();

        Task<IDataResult<CategoryListDto>> GetAllByNoneDeletedAndActive();

        Task<IDataResult<CategoryDto>> Add(CategoryAddDto categoryAddDto,string createdByName);

        Task<IDataResult<CategoryDto>> Update(CategoryUpdateDto categoryUpdateto,string modifiedByName);

        Task<IDataResult<CategoryDto>> Delete(int categoryId,string modifiedByName);//sadece flag kısmını 1 yapar

        Task<IResult> HardDelete(int categoryId);// tamamıyle databaseden siler



       

    }
}
