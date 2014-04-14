using System.Collections.Generic;
using KymdanMM.Data.Repository;
using KymdanMM.Model.Models;
using KymdanMM.Data.Infrastructure;

namespace KymdanMM.Data.Service
{
    public interface IDepartmentService
    {
        Department GetDepartment(int id);
        IEnumerable<Department> GetDepartments();
        bool AddOrUpdateDepartment(Department department);
        bool DeleteDepartment(Department department);
    }
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork)
        {
            this._departmentRepository = departmentRepository;
            this._unitOfWork = unitOfWork;
        }

        public Department GetDepartment(int id)
        {
            return _departmentRepository.GetById(id);
        }
        public IEnumerable<Department> GetDepartments()
        {
            var departments = _departmentRepository.GetAll();
            return departments;
        }

        public bool AddOrUpdateDepartment(Department department)
        {
            if (department.Id != 0)
            {
                _departmentRepository.Update(department);
                _unitOfWork.Commit();
            }
            else
            {
                _departmentRepository.Add(department);
                _unitOfWork.Commit();
            }
            return true;
        }

        public bool DeleteDepartment(Department department)
        {
            _departmentRepository.Delete(department);
            _unitOfWork.Commit();
            return true;
        }
    }
}
