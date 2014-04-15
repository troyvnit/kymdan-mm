using System.Collections.Generic;
using KymdanMM.Data.Repository;
using KymdanMM.Model.Models;
using KymdanMM.Data.Infrastructure;

namespace KymdanMM.Data.Service
{
    public interface IProgressStatusService
    {
        ProgressStatus GetProgressStatus(int id);
        IEnumerable<ProgressStatus> GetProgressStatuss();
        bool AddOrUpdateProgressStatus(ProgressStatus progressStatus);
        bool DeleteProgressStatus(ProgressStatus progressStatus);
    }
    public class ProgressStatusService : IProgressStatusService
    {
        private readonly IProgressStatusRepository _progressStatusRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProgressStatusService(IProgressStatusRepository progressStatusRepository, IUnitOfWork unitOfWork)
        {
            this._progressStatusRepository = progressStatusRepository;
            this._unitOfWork = unitOfWork;
        }

        public ProgressStatus GetProgressStatus(int id)
        {
            return _progressStatusRepository.GetById(id);
        }
        public IEnumerable<ProgressStatus> GetProgressStatuss()
        {
            var progressStatuss = _progressStatusRepository.GetAll();
            return progressStatuss;
        }

        public bool AddOrUpdateProgressStatus(ProgressStatus progressStatus)
        {
            if (progressStatus.Id != 0)
            {
                _progressStatusRepository.Update(progressStatus);
                _unitOfWork.Commit();
            }
            else
            {
                _progressStatusRepository.Add(progressStatus);
                _unitOfWork.Commit();
            }
            return true;
        }

        public bool DeleteProgressStatus(ProgressStatus progressStatus)
        {
            _progressStatusRepository.Delete(progressStatus);
            _unitOfWork.Commit();
            return true;
        }
    }
}
