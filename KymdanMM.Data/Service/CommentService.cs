using System.Collections.Generic;
using KymdanMM.Data.Repository;
using KymdanMM.Model.Models;
using KymdanMM.Data.Infrastructure;

namespace KymdanMM.Data.Service
{
    public interface ICommentService
    {
        Comment GetComment(int id);
        IEnumerable<Comment> GetComments();
        bool AddOrUpdateComment(Comment comment);
        bool DeleteComment(Comment comment);
    }
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CommentService(ICommentRepository commentRepository, IUnitOfWork unitOfWork)
        {
            this._commentRepository = commentRepository;
            this._unitOfWork = unitOfWork;
        }

        public Comment GetComment(int id)
        {
            return _commentRepository.GetById(id);
        }
        public IEnumerable<Comment> GetComments()
        {
            var comments = _commentRepository.GetAll();
            return comments;
        }

        public bool AddOrUpdateComment(Comment comment)
        {
            if (comment.Id != 0)
            {
                _commentRepository.Update(comment);
                _unitOfWork.Commit();
            }
            else
            {
                _commentRepository.Add(comment);
                _unitOfWork.Commit();
            }
            return true;
        }

        public bool DeleteComment(Comment comment)
        {
            _commentRepository.Delete(comment);
            _unitOfWork.Commit();
            return true;
        }
    }
}
