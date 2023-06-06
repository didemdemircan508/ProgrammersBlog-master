using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammersBlog.Data.Abstract
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IArticleRepository Articles { get; }

        ICategoryRepository Categories { get; }

        ICommentRepository Comments { get; }

        


        //_unifofwork.categories.AddAsync(user); unitOfWork.SaveAsync(); bunları veritabanın a kaydetmıs oluyoruz


        Task<int> SaveAsync();



    }
}
