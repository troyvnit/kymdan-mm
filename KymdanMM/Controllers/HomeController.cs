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
        private ICommentService _commentService { get; set; }
        private UsersContext usersContext { get; set; }

        public HomeController(IMaterialProposalService _materialProposalService, IMaterialService _materialService, IDepartmentService _departmentService, IProgressStatusService _progressStatusService, ICommentService _commentService)
        {
            this._materialProposalService = _materialProposalService;
            this._departmentService = _departmentService;
            this._progressStatusService = _progressStatusService;
            this._materialService = _materialService;
            this._commentService = _commentService;
            usersContext = new UsersContext();
        }

        public ActionResult Index()
        {
            if (Thread.CurrentPrincipal.IsInRole("ITAdmin"))
            {
                return RedirectToAction("Admin");
            }
            if (Thread.CurrentPrincipal.IsInRole("Admin"))
            {
                return RedirectToAction("GeneralManagerPage");
            }
            if (Thread.CurrentPrincipal.IsInRole("Department Manager"))
            {
                return RedirectToAction("DepartmentManagerPage");
            }
            if (Thread.CurrentPrincipal.IsInRole("Member"))
            {
                return RedirectToAction("MemberPage");
            }
            return RedirectToAction("AccessDenied");
        }

        public ActionResult DepartmentManagerPage()
        {
            return View();
        }

        public ActionResult GeneralManagerPage()
        {
            return View();
        }

        public ActionResult MemberPage()
        {
            return View();
        }

        public ActionResult Material(int id)
        {
            var material = Mapper.Map<Material, MaterialViewModel>(_materialService.GetMaterial(id));
            var users = usersContext.UserProfiles.ToList();
            var departments = _departmentService.GetDepartments();
            var proposerDepartment = _departmentService.GetDepartment(material.ProposerDepartmentId);
            if (proposerDepartment != null)
                material.ProposerDepartmentName = proposerDepartment.DepartmentName;
            var implementDepartment = _departmentService.GetDepartment(material.ImplementerDepartmentId);
            if (implementDepartment != null)
                material.ImplementerDepartmentName = implementDepartment.DepartmentName;
            var progressStatus = _progressStatusService.GetProgressStatus(material.ProgressStatusId);
            if (progressStatus != null)
                material.Status = progressStatus.Status;
            var implementUser = users.FirstOrDefault(a => a.UserName == material.ImplementerUserName);
            if (implementUser != null)
                material.ImplementerDisplayName = implementUser.DisplayName;
            ViewBag.Departments = departments;
            ViewBag.ProgressStatuses = _progressStatusService.GetProgressStatuses();
            ViewBag.Users = users;
            ViewBag.CurrentUser = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            return View(material);
        }

        public ActionResult CommentPartialView(int id)
        {
            var material = _materialService.GetMaterial(id);
                var materialViewModel = Mapper.Map<Material, MaterialViewModel>(material);
            var users = usersContext.UserProfiles.ToList();
            var departments = _departmentService.GetDepartments();
            var proposerDepartment = _departmentService.GetDepartment(materialViewModel.ProposerDepartmentId);
            if (proposerDepartment != null)
                materialViewModel.ProposerDepartmentName = proposerDepartment.DepartmentName;
            var implementDepartment = _departmentService.GetDepartment(materialViewModel.ImplementerDepartmentId);
            if (implementDepartment != null)
                materialViewModel.ImplementerDepartmentName = implementDepartment.DepartmentName;
            var progressStatus = _progressStatusService.GetProgressStatus(materialViewModel.ProgressStatusId);
            if (progressStatus != null)
                materialViewModel.Status = progressStatus.Status;
            var implementUser = users.FirstOrDefault(a => a.UserName == materialViewModel.ImplementerUserName);
            if (implementUser != null)
                materialViewModel.ImplementerDisplayName = implementUser.DisplayName;
            ViewBag.Departments = departments;
            ViewBag.ProgressStatuses = _progressStatusService.GetProgressStatuses();
            ViewBag.Users = users;
            ViewBag.CurrentUser = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            foreach (var comment in material.Comments)
            {
                var readUserNames = comment.ReadUserNames.Split(',').ToList();
                readUserNames.Add(Thread.CurrentPrincipal.Identity.Name);
                comment.ReadUserNames = string.Join(",", readUserNames);
            }
            _materialService.AddOrUpdateMaterial(material);
            return View(materialViewModel);
        }

        public ActionResult GetProposeMaterialForDepartmentManagerPage(int pageNumber, int pageSize, string keyWord, int? departmentId, int? progressStatusId, bool? approveStatus)
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
                            (a.Approved == approveStatus || approveStatus == null) &&
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

        public ActionResult GetUser(int? departmentId)
        {
            var users = usersContext.UserProfiles.ToList();
            var currentUser = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            return Json(users.Where(a => currentUser != null && a.DepartmentId == (departmentId ?? currentUser.DepartmentId) && Roles.IsUserInRole(a.UserName, "Member")), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllUser()
        {
            var users = usersContext.UserProfiles.ToList();
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMaterialProposal(int? pageNumber, int? pageSize, string keyWord, int? departmentId, int? progressStatusId, bool? approveStatus)
        {
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {

                IPagedList<MaterialProposal> materialProposals;
                if (Thread.CurrentPrincipal.IsInRole("Admin"))
                {
                    materialProposals = _materialProposalService.GetMaterialProposals(pageNumber ?? 1, pageSize ?? 1,
                        a => (a.Materials.Count(m => m.Approved != false) > 0) &&
                            (a.ProposerDepartmentId == departmentId || a.Materials.Count(m => m.ImplementerDepartmentId == departmentId) > 0 || departmentId == null) &&
                            (a.Materials.Count(m => m.ProgressStatusId == progressStatusId) > 0 || progressStatusId == null) &&
                            (a.Materials.Count(m => m.Approved == approveStatus) > 0 || approveStatus == null) &&
                            (a.ProposalCode.Contains(keyWord) || a.Description.Contains(keyWord) || a.Materials.Count(m => m.MaterialName.Contains(keyWord)) > 0 || string.IsNullOrEmpty(keyWord)));
                }
                else if (Thread.CurrentPrincipal.IsInRole("Department Manager"))
                {
                    materialProposals = _materialProposalService.GetMaterialProposals(pageNumber ?? 1, pageSize ?? 1,
                        a => (a.Materials.Count(m => m.ImplementerDepartmentId == departmentId) > 0 || a.ProposerDepartmentId == user.DepartmentId || a.Materials.Count(b => b.ImplementerDepartmentId == user.DepartmentId) > 0) &&
                            (a.Materials.Count(m => m.ProgressStatusId == progressStatusId) > 0 || progressStatusId == null) &&
                            (a.Materials.Count(m => m.Approved == approveStatus) > 0 || approveStatus == null) &&
                            (a.ProposalCode.Contains(keyWord) || a.Description.Contains(keyWord) || a.Materials.Count(m => m.MaterialName.Contains(keyWord)) > 0 || string.IsNullOrEmpty(keyWord)));
                }
                else
                {
                    materialProposals = _materialProposalService.GetMaterialProposals(pageNumber ?? 1, pageSize ?? 1,
                        a => (a.Materials.Count(m => m.ImplementerUserName == user.UserName) > 0 || a.ProposerUserName == user.UserName || a.Materials.Count(m => m.ImplementerUserName == user.UserName) > 0) &&
                            (a.Materials.Count(m => m.ProgressStatusId == progressStatusId) > 0 || progressStatusId == null) &&
                            (a.Materials.Count(m => m.Approved == approveStatus) > 0 || approveStatus == null) &&
                            (a.ProposalCode.Contains(keyWord) || a.Description.Contains(keyWord) || a.Materials.Count(m => m.MaterialName.Contains(keyWord)) > 0 || string.IsNullOrEmpty(keyWord)));
                }
                var data = materialProposals.Select(Mapper.Map<MaterialProposal, MaterialProposalViewModel>).ToList();
                return Json(new { data, total = materialProposals.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMaterialProposals(int? pageNumber, int? pageSize, string command)
        {
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                IPagedList<MaterialProposal> materialProposals;
                switch (command)
                {
                    case "SentAndAwaitingApprove":
                        materialProposals = _materialProposalService.GetMaterialProposals(pageNumber ?? 1, pageSize ?? 1,
                            a =>
                                a.ProposerDepartmentId == user.DepartmentId &&
                                a.Sent &&
                                a.Materials.Count(m => !m.Approved) > 0);
                        break;
                    case "Temp":
                        materialProposals = _materialProposalService.GetMaterialProposals(pageNumber ?? 1, pageSize ?? 1,
                            a =>
                                (a.ProposerDepartmentId == user.DepartmentId || (a.FromHardProposal && a.CreatedUserName == user.UserName)) &&
                                !a.Sent);
                        break;
                    case "ReceiveAndAwaitingApprove":
                        materialProposals = _materialProposalService.GetMaterialProposals(pageNumber ?? 1, pageSize ?? 1,
                            a =>
                                a.Sent &&
                                a.Materials.Count(m => !m.Approved) > 0);
                        break;
                    default:
                        materialProposals = _materialProposalService.GetMaterialProposals(pageNumber ?? 1, pageSize ?? 1, a => true);
                        break;
                }
                var data = materialProposals.Select(Mapper.Map<MaterialProposal, MaterialProposalViewModel>).ToList();
                return Json(new { data, total = materialProposals.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddOrUpdateMaterialProposal(int? id, bool? fromHardProposal)
        {
            var materialProposal = id != null ? _materialProposalService.GetMaterialProposal((int)id) : new MaterialProposal();
            var approved = materialProposal.Materials.Count(m => !m.Approved) == 0;
            var materialProposalViewModel = Mapper.Map<MaterialProposal, MaterialProposalViewModel>(materialProposal);
            materialProposalViewModel.FromHardProposal = materialProposalViewModel.FromHardProposal == true || fromHardProposal == true;
            var users = usersContext.UserProfiles.ToList();
            var currentUser = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            var proposer = users.FirstOrDefault(a => a.UserName == materialProposalViewModel.ProposerUserName);
            if (proposer != null)
                materialProposalViewModel.ProposerDisplayName =
                    proposer.DisplayName;
            if (Thread.CurrentPrincipal.IsInRole("Admin") || ((Thread.CurrentPrincipal.IsInRole("Department Manager") && currentUser != null && (materialProposalViewModel.ProposerDepartmentId == currentUser.DepartmentId || materialProposal.CreatedUserName == currentUser.UserName || materialProposalViewModel.ProposerDepartmentId == 0))))
            {
                ViewBag.Departments = _departmentService.GetDepartments();
                ViewBag.ProgressStatuses = _progressStatusService.GetProgressStatuses();
                ViewBag.Users = users;
                ViewBag.CurrentUser = currentUser;
                ViewBag.Approved = approved;
                return View(materialProposalViewModel);
            }
            return RedirectToAction("AccessDenied");
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
            else
            {
                materialProposal.ProposerUserName = materialProposal.ProposerUserName ?? Thread.CurrentPrincipal.Identity.Name;
                var users = usersContext.UserProfiles.ToList();
                var currentUser = users.FirstOrDefault(a => a.UserName == materialProposal.ProposerUserName);
                if (currentUser != null) materialProposal.ProposerDepartmentId = currentUser.DepartmentId;
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

        public ActionResult GetMaterials(string command, int? pageNumber, int? pageSize, int? id, bool? approved, string keyWord, int? departmentId, int? progressStatusId, bool? approveStatus, bool? finished)
        {
            //var materials = _materialService.GetMaterials(pageNumber, pageSize, id);
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                IPagedList<Material> materials;
                switch (command)
                {
                    case "SentAndAwaitingApprove":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                a.MaterialProposal.ProposerDepartmentId == user.DepartmentId &&
                                !a.Approved &&
                                a.MaterialProposal.Sent);
                        break;
                    case "ReceiveAndAwaitingApprove":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                !a.Approved &&
                                a.MaterialProposal.Sent);
                        break;
                    case "ApprovedAwaitingReceive":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                a.MaterialProposal.ProposerDepartmentId == user.DepartmentId &&
                                !a.Finished &&
                                a.Approved &&
                                a.MaterialProposal.Sent);
                        break;
                    case "ApprovedAwaitingImplement":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                a.ImplementerDepartmentId == user.DepartmentId &&
                                string.IsNullOrEmpty(a.ImplementerUserName) &&
                                !a.Finished &&
                                a.Approved &&
                                a.MaterialProposal.Sent);
                        break;
                    case "ApprovedImplemented":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                a.ImplementerDepartmentId == user.DepartmentId &&
                                !string.IsNullOrEmpty(a.ImplementerUserName) &&
                                !a.Finished &&
                                a.Approved &&
                                a.MaterialProposal.Sent);
                        break;
                    case "ApprovedAssigned":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                a.ImplementerDepartmentId != 0 &&
                                !a.Finished &&
                                a.Approved &&
                                a.MaterialProposal.Sent);
                        break;
                    case "AssignedFinished":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                !string.IsNullOrEmpty(a.ImplementerUserName) &&
                                a.Finished &&
                                a.Approved &&
                                a.MaterialProposal.Sent);
                        break;
                    case "Finished":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                a.ImplementerDepartmentId == user.DepartmentId &&
                                !string.IsNullOrEmpty(a.ImplementerUserName) &&
                                a.Finished &&
                                a.Approved &&
                                a.MaterialProposal.Sent);
                        break;
                    case "BeAssignedUnfinished":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                a.ImplementerUserName == Thread.CurrentPrincipal.Identity.Name &&
                                !a.Finished &&
                                a.Approved &&
                                a.MaterialProposal.Sent);
                        break;
                    case "BeAssignedFinished":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                a.ImplementerUserName == Thread.CurrentPrincipal.Identity.Name &&
                                a.Finished &&
                                a.Approved &&
                                a.MaterialProposal.Sent);
                        break;
                    default:
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1, a => (a.MaterialProposal.Id == id || id == null) && (a.Approved == approved || approved == null));
                        break;
                }
                var materialViewModels = command == "SentAndAwaitingApprove" || command == "ReceiveAndAwaitingApprove" ?
                    materials.Select(Mapper.Map<Material, MaterialViewModel>).ToList().OrderBy(a => a.Deadline) :
                    materials.Select(Mapper.Map<Material, MaterialViewModel>).ToList().OrderBy(a => a.FinishDate);
                foreach (var materialViewModel in materialViewModels)
                {
                    var model = materialViewModel;
                    var proposalDeparmentComments = model.ImplementerDepartmentId != model.ProposerDepartmentId ? 
                        materialViewModel.Comments.Where(
                            a =>
                            {
                                var poster = usersContext.UserProfiles.ToList().FirstOrDefault(u => u.UserName == a.PosterUserName);
                                return poster != null && model.ProposerDepartmentId == poster.DepartmentId;
                            }) : 
                            materialViewModel.Comments.Where(
                            a =>
                            {
                                var poster = usersContext.UserProfiles.ToList().FirstOrDefault(u => u.UserName == a.PosterUserName);
                                return poster != null && model.ProposerDepartmentId == poster.DepartmentId && model.ImplementerUserName != a.PosterUserName;
                            });
                    var lastProposalDeparmentComment = proposalDeparmentComments.LastOrDefault();
                    if (lastProposalDeparmentComment != null)
                        materialViewModel.LastProposalDeparmentComment = lastProposalDeparmentComment.Content;
                    var implementDepartmentComments = model.ImplementerDepartmentId != model.ProposerDepartmentId ? materialViewModel.Comments.Where(
                            a =>
                            {
                                var poster = usersContext.UserProfiles.ToList().FirstOrDefault(u => u.UserName == a.PosterUserName);
                                return poster != null && model.ImplementerDepartmentId == poster.DepartmentId;
                            }) :
                            materialViewModel.Comments.Where(
                            a =>
                            {
                                var poster = usersContext.UserProfiles.ToList().FirstOrDefault(u => u.UserName == a.PosterUserName);
                                return poster != null && model.ProposerDepartmentId == poster.DepartmentId && model.ImplementerUserName == a.PosterUserName;
                            });
                    var lastImplementDepartmentComment = implementDepartmentComments.LastOrDefault();
                    if (lastImplementDepartmentComment != null)
                        materialViewModel.LastImplementDepartmentComment = lastImplementDepartmentComment.Content;
                    var generalManagerComments = materialViewModel.Comments.Where(a => Roles.IsUserInRole(a.PosterUserName, "Admin"));
                    var lastGeneralManagerComment = generalManagerComments.LastOrDefault();
                    if (lastGeneralManagerComment != null)
                        materialViewModel.LastGeneralManagerComment = lastGeneralManagerComment.Content;
                }
                return Json(new { data = materialViewModels, total = materials.TotalItemCount },
                    JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddOrUpdateMaterial(string materials, int? materialProposalId)
        {
            var materialViewModels = JsonConvert.DeserializeObject<List<MaterialViewModel>>(materials);
            foreach (var materialViewModel in materialViewModels)
            {
                var material = Mapper.Map<MaterialViewModel, Material>(materialViewModel);
                material.MaterialProposalId = materialProposalId ?? material.MaterialProposalId;
                var users = usersContext.UserProfiles.ToList();
                var currentUser = users.FirstOrDefault(a => a.UserName == material.ImplementerUserName);
                if (currentUser != null) material.ImplementerDepartmentId = currentUser.DepartmentId;
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
                var material = _materialService.GetMaterial(materialViewModel.Id);
                _materialService.DeleteMaterial(material);
            }
            return Json(materialViewModels, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteMaterialProposal(string materialProposals)
        {
            var materialProposalViewModels = JsonConvert.DeserializeObject<List<MaterialProposalViewModel>>(materialProposals);
            foreach (var materialProposalViewModel in materialProposalViewModels)
            {
                var materialProposal = _materialProposalService.GetMaterialProposal(materialProposalViewModel.Id);
                _materialProposalService.DeleteMaterialProposal(materialProposal);
            }
            return Json(materialProposalViewModels, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ApproveMaterial(string idString)
        {
            var ids = idString.Split(',');
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user == null) return Json(false, JsonRequestBehavior.AllowGet);
            foreach (var material in ids.Select(id => _materialService.GetMaterial(Convert.ToInt32(id))))
            {
                if (Thread.CurrentPrincipal.IsInRole("Admin") &&
                    material.Approved != true)
                {
                    material.Approved = true;
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
            var comment = new Comment { Content = content, PosterUserName = Thread.CurrentPrincipal.Identity.Name };
            var material = _materialService.GetMaterial(id);
            var readUserNames = new List<string>();
            var users = usersContext.UserProfiles.ToList();
            var user = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                comment.PosterDisplayName = user.DisplayName;
                readUserNames.Add(comment.PosterUserName);
                comment.ReadUserNames = string.Join(",", readUserNames);
            }
            comment.Approved = true;
            material.Comments.Add(comment);
            _materialService.AddOrUpdateMaterial(material);
            return Json(Mapper.Map<Comment, CommentViewModel>(comment), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ApproveComment(int id)
        {
            var comment = _commentService.GetComment(id);
            comment.Approved = true;
            _commentService.AddOrUpdateComment(comment);
            return Json(Mapper.Map<Comment, CommentViewModel>(comment), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteComment(int id)
        {
            var comment = _commentService.GetComment(id);
            _commentService.DeleteComment(comment);
            return Json(Mapper.Map<Comment, CommentViewModel>(comment), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AccessDenied()
        {
            ViewBag.Message = "Quyền hạn của bạn không phù hợp xem trang này!";

            return View();
        }

        public ActionResult GetImplementerDepartment(string command, int? id)
        {
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                IPagedList<Material> materials;
                switch (command)
                {
                    case "SentAndAwaitingApprove":
                        materials = _materialService.GetMaterials(1, 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                 a.MaterialProposal.ProposerDepartmentId == user.DepartmentId &&
                                 !a.Approved &&
                                 a.MaterialProposal.Sent);
                        break;
                    case "ReceiveAndAwaitingApprove":
                        materials = _materialService.GetMaterials(1, 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                 !a.Approved &&
                                 a.MaterialProposal.Sent);
                        break;
                    case "ApprovedAwaitingReceive":
                        materials = _materialService.GetMaterials(1, 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                 a.MaterialProposal.ProposerDepartmentId == user.DepartmentId &&
                                 !a.Finished &&
                                 a.Approved &&
                                 a.MaterialProposal.Sent);
                        break;
                    case "ApprovedAwaitingImplement":
                        materials = _materialService.GetMaterials(1, 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                 a.ImplementerDepartmentId == user.DepartmentId &&
                                 string.IsNullOrEmpty(a.ImplementerUserName) &&
                                 !a.Finished &&
                                 a.Approved &&
                                 a.MaterialProposal.Sent);
                        break;
                    case "ApprovedImplemented":
                        materials = _materialService.GetMaterials(1, 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                 a.ImplementerDepartmentId == user.DepartmentId &&
                                 !string.IsNullOrEmpty(a.ImplementerUserName) &&
                                 !a.Finished &&
                                 a.Approved &&
                                 a.MaterialProposal.Sent);
                        break;
                    case "ApprovedAssigned":
                        materials = _materialService.GetMaterials(1, 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                 a.ImplementerDepartmentId != 0 &&
                                 a.Approved &&
                                 a.MaterialProposal.Sent);
                        break;
                    case "Finished":
                        materials = _materialService.GetMaterials(1, 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                 a.ImplementerDepartmentId == user.DepartmentId &&
                                 !string.IsNullOrEmpty(a.ImplementerUserName) &&
                                 a.Finished &&
                                 a.Approved &&
                                 a.MaterialProposal.Sent);
                        break;
                    case "BeAssignedUnfinished":
                        materials = _materialService.GetMaterials(1, 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                 a.ImplementerUserName == Thread.CurrentPrincipal.Identity.Name &&
                                 !a.Finished &&
                                 a.Approved &&
                                 a.MaterialProposal.Sent);
                        break;
                    case "BeAssignedFinished":
                        materials = _materialService.GetMaterials(1, 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                 a.ImplementerUserName == Thread.CurrentPrincipal.Identity.Name &&
                                 a.Finished &&
                                 a.Approved &&
                                 a.MaterialProposal.Sent);
                        break;
                    default:
                        materials = _materialService.GetMaterials(1, 1,
                            a =>
                                (a.MaterialProposal.Id == id || id == null));
                        break;
                }
                return Json(materials.Select(a => new { Text = _departmentService.GetDepartment(a.ImplementerDepartmentId) != null ? _departmentService.GetDepartment(a.ImplementerDepartmentId).DepartmentName : "", Value = a.ImplementerDepartmentId }).ToList(), JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
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
                    var physicalPath = Path.Combine(Server.MapPath("~/Images/Upload"), fileName);
                    file.SaveAs(physicalPath);
                }
                return Json(files.Select(a => new FileViewModel { name = a.FileName, size = a.ContentLength, extension = "." + a.FileName.Split('.')[1] }), "text/plain");
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
                    var physicalPath = Path.Combine(Server.MapPath("~/Images/Upload"), fileName);

                    // TODO: Verify user permissions

                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }
            }
            return Json(fileNames, "text/plain");
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
