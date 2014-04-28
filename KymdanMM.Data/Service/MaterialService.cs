using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using KymdanMM.Data.Repository;
using KymdanMM.Model.Models;
using KymdanMM.Data.Infrastructure;
using PagedList;

namespace KymdanMM.Data.Service
{
    public interface IMaterialService
    {
        Material GetMaterial(int id);
        IEnumerable<Material> GetMaterials();
        IPagedList<Material> GetMaterials(int pageNumber, int pageSize, Expression<Func<Material, bool>> where);
        bool AddOrUpdateMaterial(Material material);
        bool DeleteMaterial(Material material);
    }
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MaterialService(IMaterialRepository materialRepository, IUnitOfWork unitOfWork)
        {
            this._materialRepository = materialRepository;
            this._unitOfWork = unitOfWork;
        }

        public Material GetMaterial(int id)
        {
            return _materialRepository.GetById(id);
        }

        public IEnumerable<Material> GetMaterials()
        {
            var materials = _materialRepository.GetAll();
            return materials;
        }

        public IPagedList<Material> GetMaterials(int pageNumber, int pageSize, Expression<Func<Material, bool>> where)
        {
            var materials = _materialRepository.GetPage(new Page { PageNumber = pageNumber, PageSize = pageSize }, where, a => a.CreatedDate);
            return materials;
        }

        public bool AddOrUpdateMaterial(Material material)
        {
            if (material.Id != 0)
            {
                _materialRepository.Update(material);
                _unitOfWork.Commit();
            }
            else
            {
                _materialRepository.Add(material);
                _unitOfWork.Commit();
            }
            return true;
        }

        public bool DeleteMaterial(Material material)
        {
            _materialRepository.Delete(material);
            _unitOfWork.Commit();
            return true;
        }
    }
}
