using App.Business.Abstract;
using App.DataAccess.Abstract;
using App.DataAccess.Concrete.EfEntityFramework;
using App.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Business.Concrete
{
    public class CategoryService : ICategoryService
    {
        private ICategoryDal _categoryDal;

        public CategoryService(ICategoryDal categoryDal)
        {
            _categoryDal = categoryDal;
        }
        public CategoryService()
        {
            _categoryDal = new EfCategoryDal();
        }
        public List<Category> GetList()
        {
            return _categoryDal.GetList();
        }
    }
}
