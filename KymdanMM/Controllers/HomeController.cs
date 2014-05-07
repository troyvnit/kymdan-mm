using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AutoMapper;
using KymdanMM.Data;
using KymdanMM.Data.Service;
using KymdanMM.Filters;
using KymdanMM.Model.Models;
using KymdanMM.Models;
using Newtonsoft.Json;
using PagedList;
using WebMatrix.WebData;

namespace KymdanMM.Controllers
{
    [InitializeSimpleMembership]
    [CustomAuthorizeAttribute]
    public class HomeController : Controller
    {
        private IMaterialProposalService _materialProposalService { get; set; }
        private IMaterialService _materialService { get; set; }
        private IDepartmentService _departmentService { get; set; }
        private IProgressStatusService _progressStatusService { get; set; }
        private UsersContext usersContext { get; set; }

        public HomeController(IMaterialProposalService _materialProposalService, IMaterialService _materialService, IDepartmentService _departmentService, IProgressStatusService _progressStatusService)
        {
            this._materialProposalService = _materialProposalService;
            this._departmentService = _departmentService;
            this._progressStatusService = _progressStatusService;
            this._materialService = _materialService;
            usersContext = new UsersContext();
        }

        public ActionResult Index()
        {
            if (Thread.CurrentPrincipal.IsInRole("ITAdmin"))
            {
                return RedirectToAction("Admin");
            }
            return View();
        }

        public ActionResult DepartmentManagerPage()
        {
            return View();
        }

        public ActionResult Material(int id)
        {
            var material = Mapper.Map<Material, MaterialViewModel>(_materialService.GetMaterial(id));
            var users = usersContext.UserProfiles.ToList();
            ViewBag.Departments = _departmentService.GetDepartments();
            ViewBag.ProgressStatuses = _progressStatusService.GetProgressStatuses();
            ViewBag.Users = users;
            ViewBag.CurrentUser = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            return View(material);
        }

        public ActionResult GetProposeMaterialForDepartmentManagerPage(int pageNumber, int pageSize, string keyWord, int? departmentId, int? progressStatusId, ApproveStatus? approveStatus)
        {
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
               var materials = _materialService.GetMaterials(pageNumber, pageSize,
                       a => (a.ImplementerDepartmentId == departmentId ||
                            a.MaterialProposal.ProposerDepartmentId == user.DepartmentId ||
                            a.ImplementerDepartmentId == user.DepartmentId) &&
                           (a.ProgressStatusId == progressStatusId ||
                            progressStatusId == null) &&
                           (a.ApproveStatus == approveStatus || approveStatus == null) &&
                           (a.MaterialProposal.ProposalCode.Contains(keyWord) ||
                            a.MaterialProposal.Description.Contains(keyWord) || a.Description.Contains(keyWord) ||
                            a.MaterialName.Contains(keyWord) || string.IsNullOrEmpty(keyWord)));
                var materialViewModels = materials.Select(Mapper.Map<Material, MaterialViewModel>).ToList();
                return Json(new { data = materialViewModels, total = materials.TotalItemCount },
                    JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDepartment()
        {
            var departments = _departmentService.GetDepartments();
            return Json(departments, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddDepartment(string departmentName)
        {
            var department = new Department { DepartmentName = departmentName };
            _departmentService.AddOrUpdateDepartment(department);
            return Json(department, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteDepartment(int id)
        {
            var department = _departmentService.GetDepartment(id);
            _departmentService.DeleteDepartment(department);
            return Json(department, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProgressStatus()
        {
            var progressStatuses = _progressStatusService.GetProgressStatuses();
            return Json(progressStatuses, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddProgressStatus(string status)
        {
            var progressStatus = new ProgressStatus { Status = status };
            _progressStatusService.AddOrUpdateProgressStatus(progressStatus);
            return Json(progressStatus, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteProgressStatus(int id)
        {
            var progressStatus = _progressStatusService.GetProgressStatus(id);
            _progressStatusService.DeleteProgressStatus(progressStatus);
            return Json(progressStatus, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUser()
        {
            var users = usersContext.UserProfiles.ToList();
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMaterialProposal(int pageNumber, int pageSize, string keyWord, int? departmentId, int? progressStatusId, ApproveStatus? approveStatus)
        {
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {

                IPagedList<MaterialProposal> materialProposals;
                if (Thread.CurrentPrincipal.IsInRole("Admin"))
                {
                    materialProposals = _materialProposalService.GetMaterialProposals(pageNumber, pageSize,
                        a => (a.Materials.Count(m => m.ApproveStatus != ApproveStatus.Unapproved) > 0) &&
                            (a.ProposerDepartmentId == departmentId || a.Materials.Count(m => m.ImplementerDepartmentId == departmentId) > 0 || departmentId == null) &&
                            (a.Materials.Count(m => m.ProgressStatusId == progressStatusId) > 0 || progressStatusId == null) &&
                            (a.Materials.Count(m => m.ApproveStatus == approveStatus) > 0 || approveStatus == null) &&
                            (a.ProposalCode.Contains(keyWord) || a.Description.Contains(keyWord) || a.Materials.Count(m => m.MaterialName.Contains(keyWord)) > 0 || string.IsNullOrEmpty(keyWord)));
                }
                else if (Thread.CurrentPrincipal.IsInRole("Department Manager"))
                {
                    materialProposals = _materialProposalService.GetMaterialProposals(pageNumber, pageSize,
                        a => (a.Materials.Count(m => m.ImplementerDepartmentId == departmentId) > 0 || a.ProposerDepartmentId == user.DepartmentId || a.Materials.Count(b => b.ImplementerDepartmentId == user.DepartmentId) > 0) &&
                            (a.Materials.Count(m => m.ProgressStatusId == progressStatusId) > 0 || progressStatusId == null) &&
                            (a.Materials.Count(m => m.ApproveStatus == approveStatus) > 0 || approveStatus == null) &&
                            (a.ProposalCode.Contains(keyWord) || a.Description.Contains(keyWord) || a.Materials.Count(m => m.MaterialName.Contains(keyWord)) > 0 || string.IsNullOrEmpty(keyWord)));
                }
                else
                {
                    materialProposals = _materialProposalService.GetMaterialProposals(pageNumber, pageSize,
                        a => (a.Materials.Count(m => m.ImplementerUserName == user.UserName) > 0 || a.ProposerUserName == user.UserName || a.Materials.Count(m => m.ImplementerUserName == user.UserName) > 0) &&
                            (a.Materials.Count(m => m.ProgressStatusId == progressStatusId) > 0 || progressStatusId == null) &&
                            (a.Materials.Count(m => m.ApproveStatus == approveStatus) > 0 || approveStatus == null) &&
                            (a.ProposalCode.Contains(keyWord) || a.Description.Contains(keyWord) || a.Materials.Count(m => m.MaterialName.Contains(keyWord)) > 0 || string.IsNullOrEmpty(keyWord)));
                }
                var data = materialProposals.Select(Mapper.Map<MaterialProposal, MaterialProposalViewModel>).ToList();
                return Json(new { data, total = materialProposals.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMaterialProposals()
        {
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                var materialProposals =
                    _materialProposalService.GetMaterialProposals()
                        .Select(
                            a =>
                                new
                                {
                                    MaterialProposalId = a.Id,
                                    MaterialProposalCode = a.ProposalCode,
                                    Type = a.ProposerDepartmentId == user.DepartmentId ? "Đề xuất" : "Thực hiện"
                                });
                return Json(materialProposals, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddOrUpdateMaterialProposal(int? id)
        {
            if ((id == null || id == 0) && !(Thread.CurrentPrincipal.IsInRole("Member") || Thread.CurrentPrincipal.IsInRole("Department Manager")))
            {
                return RedirectToAction("AccessDenied");
            }
            var materialProposal = id != null ? _materialProposalService.GetMaterialProposal((int)id) : new MaterialProposal();
            var materialProposalViewModel = materialProposal != null ? Mapper.Map<MaterialProposal, MaterialProposalViewModel>(materialProposal) : new MaterialProposalViewModel();
            if (id == null && Thread.CurrentPrincipal.IsInRole("Department Manager"))
            {
                materialProposalViewModel.FromHardProposal = true;
            }
            materialProposalViewModel.ProposerUserName = materialProposalViewModel.ProposerUserName ?? Thread.CurrentPrincipal.Identity.Name;
            var users = usersContext.UserProfiles.ToList();
            var proposerUser = users.FirstOrDefault(a => a.UserName == materialProposalViewModel.ProposerUserName);
            if (proposerUser != null)
            {
                materialProposalViewModel.ProposerDisplayName = proposerUser.DisplayName;
                materialProposalViewModel.ProposerDepartmentId = materialProposalViewModel.ProposerDepartmentId != 0 ? materialProposalViewModel.ProposerDepartmentId : proposerUser.DepartmentId;
            }
            var proposerDepartment = _departmentService.GetDepartment(materialProposalViewModel.ProposerDepartmentId);
            if (proposerDepartment != null)
                materialProposalViewModel.ProposerDepartmentName = proposerDepartment.DepartmentName;
            ViewBag.Departments = _departmentService.GetDepartments();
            ViewBag.ProgressStatuses = _progressStatusService.GetProgressStatuses();
            ViewBag.Users = users;
            ViewBag.CurrentUser = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            return View(materialProposalViewModel);
        }

        [HttpPost]
        public ActionResult AddOrUpdateMaterialProposal(MaterialProposalViewModel materialProposalViewModel, string materials)
        {
            var materialProposal = Mapper.Map<MaterialProposalViewModel, MaterialProposal>(materialProposalViewModel);
            if (materialProposal.Id != 0)
            {
                materialProposal.CreatedDate = DateTime.MinValue;
                materialProposal.CreatedUserName = null;
            }
            _materialProposalService.AddOrUpdateMaterialProposal(materialProposal);
            return Json(materialProposal.Id, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ApproveMaterialProposal(string idString)
        {
            var ids = idString.Split(',');
            foreach (var id in ids)
            {
                var materialProposal = _materialProposalService.GetMaterialProposal(Convert.ToInt32(id));
                _materialProposalService.AddOrUpdateMaterialProposal(materialProposal);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMaterial(int? id, int pageNumber, int pageSize, string keyWord, int? departmentId, int? progressStatusId, ApproveStatus? approveStatus, bool? finished)
        {
            //var materials = _materialService.GetMaterials(pageNumber, pageSize, id);
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                IPagedList<Material> materials;
                if (Thread.CurrentPrincipal.IsInRole("Admin"))
                {
                    materials = _materialService.GetMaterials(pageNumber, pageSize,
                        a => (a.MaterialProposal.Id == id || id == null) && (a.ApproveStatus != ApproveStatus.Unapproved) &&
                             (a.MaterialProposal.ProposerDepartmentId == departmentId ||
                              a.ImplementerDepartmentId == departmentId || departmentId == null) &&
                             (a.ProgressStatusId == progressStatusId || progressStatusId == null) &&
                             (a.ApproveStatus == approveStatus || approveStatus == null) &&
                             (a.Finished != true || finished == null) &&
                             (a.MaterialProposal.ProposalCode.Contains(keyWord) ||
                              a.MaterialProposal.Description.Contains(keyWord) || a.Description.Contains(keyWord) ||
                              a.MaterialName.Contains(keyWord) || string.IsNullOrEmpty(keyWord)));
                }
                else if (Thread.CurrentPrincipal.IsInRole("Department Manager"))
                {
                    materials = _materialService.GetMaterials(pageNumber, pageSize,
                        a =>
                            (a.MaterialProposal.Id == id || id == null) &&
                            (a.MaterialProposal.ProposerDepartmentId == user.DepartmentId || a.MaterialProposal.CreatedUserName == user.UserName ||
                             (a.ImplementerDepartmentId == user.DepartmentId && a.ApproveStatus == ApproveStatus.GeneralManagerApproved)) &&
                            (a.ProgressStatusId == progressStatusId ||
                             progressStatusId == null) &&
                            (a.ApproveStatus == approveStatus || approveStatus == null) &&
                             (a.Finished != true || finished == null) &&
                            (a.MaterialProposal.ProposalCode.Contains(keyWord) ||
                             a.MaterialProposal.Description.Contains(keyWord) || a.Description.Contains(keyWord) ||
                             a.MaterialName.Contains(keyWord) || string.IsNullOrEmpty(keyWord)));
                }
                else
                {
                    materials = _materialService.GetMaterials(pageNumber, pageSize,
                        a => (a.MaterialProposal.Id == id || id == null) &&
                            (a.MaterialProposal.ProposerUserName == user.UserName || (a.ImplementerUserName == user.UserName && a.ApproveStatus == ApproveStatus.GeneralManagerApproved)) &&
                            (a.ProgressStatusId == progressStatusId ||
                             progressStatusId == null) &&
                            (a.ApproveStatus == approveStatus || approveStatus == null) &&
                             (a.Finished != true || finished == null) &&
                            (a.MaterialProposal.ProposalCode.Contains(keyWord) ||
                             a.MaterialProposal.Description.Contains(keyWord) || a.Description.Contains(keyWord) ||
                             a.MaterialName.Contains(keyWord) || string.IsNullOrEmpty(keyWord)));
                }
                var materialViewModels = materials.Select(Mapper.Map<Material, MaterialViewModel>).ToList();
                foreach (var materialProposalViewModel in materialViewModels)
                {
                    var materialProposal =
                        _materialProposalService.GetMaterialProposal(materialProposalViewModel.MaterialProposalId);
                    materialProposalViewModel.Type = materialProposal.CreatedUserName == user.UserName && materialProposal.FromHardProposal ? "Tạo từ giấy" : materialProposalViewModel.ImplementerDepartmentId ==
                                                     user.DepartmentId
                        ? "Phải thực hiện"
                        : "Đã đề xuất";
                }
                return Json(new {data = materialViewModels, total = materials.TotalItemCount},
                    JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddOrUpdateMaterial(string materials, int materialProposalId)
        {
            var materialViewModels = JsonConvert.DeserializeObject<List<MaterialViewModel>>(materials);
            foreach (var materialViewModel in materialViewModels)
            {
                var material = Mapper.Map<MaterialViewModel, Material>(materialViewModel);
                material.MaterialProposalId = materialProposalId;
                _materialService.AddOrUpdateMaterial(material);
            }
            return Json(materialViewModels, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteMaterial(string materials)
        {
            var materialViewModels = JsonConvert.DeserializeObject<List<MaterialViewModel>>(materials);
            foreach (var materialViewModel in materialViewModels)
            {
                var material = Mapper.Map<MaterialViewModel, Material>(materialViewModel);
                _materialService.DeleteMaterial(material);
            }
            return Json(materialViewModels, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ApproveMaterial(string idString)
        {
            var ids = idString.Split(',');
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user == null) return Json(false, JsonRequestBehavior.AllowGet);
            foreach (var material in ids.Select(id => _materialService.GetMaterial(Convert.ToInt32(id))))
            {
                if (Thread.CurrentPrincipal.IsInRole("Department Manager") &&
                    material.MaterialProposal.ProposerDepartmentId == user.DepartmentId &&
                    material.ApproveStatus < ApproveStatus.ManagerApproved)
                {
                    material.ApproveStatus = ApproveStatus.ManagerApproved;
                    material.StartDate = DateTime.Now;

                }
                if (Thread.CurrentPrincipal.IsInRole("Admin") &&
                    material.ApproveStatus < ApproveStatus.GeneralManagerApproved)
                {
                    material.ApproveStatus = ApproveStatus.GeneralManagerApproved;
                    material.ApproveDate = DateTime.Now;
                }
                _materialService.AddOrUpdateMaterial(material);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FinishMaterial(string idString)
        {
            var ids = idString.Split(',');
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user == null) return Json(false, JsonRequestBehavior.AllowGet);
            foreach (var material in ids.Select(id => _materialService.GetMaterial(Convert.ToInt32(id))))
            {
                if (Thread.CurrentPrincipal.IsInRole("Department Manager") &&
                    material.ImplementerDepartmentId == user.DepartmentId)
                {
                    material.Finished = true;

                }
                _materialService.AddOrUpdateMaterial(material);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddComment(string content, int id)
        {
            var comment = new Comment{ Content = content, PosterUserName = Thread.CurrentPrincipal.Identity.Name};
            var materialProposal = _materialProposalService.GetMaterialProposal(id);
            var users = usersContext.UserProfiles.ToList();
            var user = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                comment.PosterDisplayName = user.DisplayName;
            }
            materialProposal.Comments.Add(comment);
            _materialProposalService.AddOrUpdateMaterialProposal(materialProposal);
            return Json(Mapper.Map<Comment, CommentViewModel>(comment), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AccessDenied()
        {
            ViewBag.Message = "Quyền hạn của bạn không phù hợp xem trang này!";

            return View();
        }

        [Authorize(Roles = "ITAdmin")]
        public ActionResult Admin()
        {
            ViewBag.Departments = _departmentService.GetDepartments();
            ViewBag.ProgressStatuses = _progressStatusService.GetProgressStatuses();
            return View();
        }

        [Authorize(Roles = "ITAdmin")]
        public ActionResult BackupDatabase()
        {
            var fileName = "KymdanMMBackup_" + DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".bak";
            var dbPath = Server.MapPath("~/App_Data/" + fileName);
            using (var db = new KymdanMMEntities())
            {
                var cmd = String.Format("BACKUP DATABASE {0} TO DISK='{1}' WITH FORMAT, MEDIANAME='DbBackups', MEDIADESCRIPTION='Media set for {0} database';"
                    , "KymdanMM", dbPath);
                db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, cmd);
            }

            return File(new FileStream(dbPath, FileMode.Open), "application/octet-stream", fileName);
        }

        public ActionResult SaveFile(IEnumerable<HttpPostedFileBase> files)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                    file.SaveAs(physicalPath);
                }
                return Json(files.Select(a => new { name = a.FileName, size = a.ContentLength, extension = a.ContentType }), "text/plain");
            }
            return Content("");
        }

        public ActionResult RemoveFile(string[] fileNames)
        {
            if (fileNames != null)
            {
                foreach (var fullName in fileNames)
                {
                    var fileName = Path.GetFileName(fullName);
                    var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);

                    // TODO: Verify user permissions

                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }
            }
            return Content("");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
