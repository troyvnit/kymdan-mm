﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AutoMapper;
using Excel;
using KymdanMM.Data;
using KymdanMM.Data.Infrastructure;
using KymdanMM.Data.Service;
using KymdanMM.Filters;
using KymdanMM.Model.Models;
using KymdanMM.Models;
using Newtonsoft.Json;
using PagedList;

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
                return Redirect("/Home/DepartmentManagerPage#ApprovedAwaitingImplement");
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
            var currentUser = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (currentUser != null)
            {
                if (!Thread.CurrentPrincipal.IsInRole("Admin"))
                {
                    var assignInfoViewModel = material.AssignInfoes.FirstOrDefault(
                        assign =>
                            assign.DepartmentId == currentUser.DepartmentId);
                    if (assignInfoViewModel != null)
                    {
                        material.ProgressStatusId = assignInfoViewModel.ProgressStatusId;
                        material.Finished = assignInfoViewModel.Finished;
                    }
                    else
                    {
                        material.ProgressStatusId = 0;
                        material.Finished = false;
                    }
                }
                else
                {
                    material.Finished = material.AssignInfoes.Any() && material.AssignInfoes.Count(assign => !assign.Finished) == 0;
                }
            }
            var departments = _departmentService.GetDepartments();
            var proposerDepartment = _departmentService.GetDepartment(material.ProposerDepartmentId);
            if (proposerDepartment != null)
                material.ProposerDepartmentName = proposerDepartment.DepartmentName;
            if (!string.IsNullOrEmpty(material.ImplementerDepartmentIds))
            {
                var implementDepartmentIds = material.ImplementerDepartmentIds.Split(',');
                var implementDepartmentNames = (from implementDepartmentId in implementDepartmentIds select _departmentService.GetDepartment(Convert.ToInt32(implementDepartmentId)) into implementDepartment where implementDepartment != null select implementDepartment.DepartmentName).ToList();
                material.ImplementerDepartmentNames = string.Join(",", implementDepartmentNames); 
            }
            var progressStatus = _progressStatusService.GetProgressStatus(material.ProgressStatusId);
            if (progressStatus != null)
                material.Status = progressStatus.Status;
            var proposeUser = users.FirstOrDefault(a => a.UserName == material.ProposerUserName);
            if (!string.IsNullOrEmpty(material.ImplementerUserNames))
            {
                var implementUserNames = material.ImplementerUserNames.Split(',');
                var implementerDisplayNames = (from implementUserName in implementUserNames select users.FirstOrDefault(a => a.UserName == implementUserName) into implementUser where implementUser != null select implementUser.DisplayName).ToList();
                material.ImplementerDisplayNames = string.Join(",", implementerDisplayNames); 
            }
            if (proposeUser != null)
                material.ProposerDisplayName = proposeUser.DisplayName;
            ViewBag.Departments = departments;
            ViewBag.ProgressStatuses = _progressStatusService.GetProgressStatuses();
            ViewBag.Users = users;
            ViewBag.CurrentUser = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            return View(material);
        }

        public ActionResult CommentPartialView(int id, string relateIds)
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
            var relateMaterials = relateIds.Split(',').Select(relateId => _materialService.GetMaterial(Convert.ToInt32(relateId))).ToList();
            ViewBag.RelateMaterials = relateMaterials;
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
                             (a.ImplementerDepartmentId == user.DepartmentId || a.ImplementerDepartmentIds.StartsWith(user.DepartmentId + ",") || a.ImplementerDepartmentIds.Contains("," + user.DepartmentId + ",") || a.ImplementerDepartmentIds.EndsWith("," + user.DepartmentId) || a.ImplementerDepartmentIds == user.DepartmentId.ToString())) &&
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
            if (_departmentService.GetDepartments().Any(a => a.DepartmentName.ToUpper() == departmentName.ToUpper()))
            {
                return Json(new { success = false, message = "Đã tồn tại tên phòng ban \"" + departmentName + "\""});
            }
            _departmentService.AddOrUpdateDepartment(department);
            return Json(new { success = true, department}, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetUser(string departmentIds)
        {
            var users = usersContext.UserProfiles.ToList();
            if (Thread.CurrentPrincipal.IsInRole("Admin"))
            {
                return
                    Json(
                        users.Where(
                            a =>
                                (string.IsNullOrEmpty(departmentIds) || departmentIds.StartsWith(a.DepartmentId + ",") ||
                                 departmentIds.Contains("," + a.DepartmentId + ",") ||
                                 departmentIds.EndsWith("," + a.DepartmentId) ||
                                 departmentIds == a.DepartmentId.ToString()) &&
                                (Roles.IsUserInRole(a.UserName, "Member") ||
                                 Roles.IsUserInRole(a.UserName, "Department Manager"))), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(users.Where(a =>
                                        {
                                            var firstOrDefault = users.FirstOrDefault(u => u.UserName == Thread.CurrentPrincipal.Identity.Name);
                                            return firstOrDefault != null && ((a.DepartmentId == firstOrDefault.DepartmentId) && (Roles.IsUserInRole(a.UserName, "Member") || Roles.IsUserInRole(a.UserName, "Department Manager")));
                                        }), JsonRequestBehavior.AllowGet);
            }
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
            var users = usersContext.UserProfiles.ToList();
            var currentUser = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (currentUser != null)
            {
                var currentYear = DateTime.Now.Year.ToString().Substring(2, 2);
                var currentDepartmentName = _departmentService.GetDepartment(currentUser.DepartmentId).DepartmentName;
                var lastMaterialProposal = _materialProposalService.GetMaterialProposals().Where(a => a.Materials.Count(m => !m.Deleted) > 0).OrderBy(a => a.CreatedDate)
                    .LastOrDefault(a => a.ProposalCode.Contains(currentYear + "/" + currentDepartmentName));
                    var currentProposalCodeSplited = lastMaterialProposal != null ?
                        lastMaterialProposal.ProposalCode.Split('/') : new [] { currentYear , currentDepartmentName, "00000"};
                int num;
                var result = Int32.TryParse(currentProposalCodeSplited[2], out num);
                if (!result)
                {
                    num = 0;
                }
                var materialProposal = id != null ? _materialProposalService.GetMaterialProposal((int)id) : 
                    new MaterialProposal
                    {
                        ProposalCode = fromHardProposal != true || Thread.CurrentPrincipal.IsInRole("Admin") ? currentProposalCodeSplited[0] + "/" + currentProposalCodeSplited[1] + "/" + (num + 1) : ""
                    };
                var approved = materialProposal.Materials.Count(m => !m.Approved) == 0;
                var materialProposalViewModel = Mapper.Map<MaterialProposal, MaterialProposalViewModel>(materialProposal);
                materialProposalViewModel.FromHardProposal = materialProposalViewModel.FromHardProposal == true || fromHardProposal == true;
                materialProposalViewModel.ProposerUserName =
                    string.IsNullOrEmpty(materialProposalViewModel.ProposerUserName)
                        ? Thread.CurrentPrincipal.Identity.Name
                        : materialProposalViewModel.ProposerUserName;
                var proposer = users.FirstOrDefault(a => a.UserName == materialProposalViewModel.ProposerUserName);
                if (proposer != null)
                    materialProposalViewModel.ProposerDisplayName =
                        proposer.DisplayName;
                if (Thread.CurrentPrincipal.IsInRole("Admin") || ((Thread.CurrentPrincipal.IsInRole("Department Manager") && (materialProposalViewModel.ProposerDepartmentId == currentUser.DepartmentId || materialProposal.CreatedUserName == currentUser.UserName || materialProposalViewModel.ProposerDepartmentId == 0))))
                {
                    ViewBag.Departments = _departmentService.GetDepartments();
                    ViewBag.ProgressStatuses = _progressStatusService.GetProgressStatuses();
                    ViewBag.Users = users;
                    ViewBag.CurrentUser = currentUser;
                    ViewBag.Approved = approved;
                    return View(materialProposalViewModel);
                }
            }
            return RedirectToAction("AccessDenied");
        }

        [HttpPost]
        public ActionResult AddOrUpdateMaterialProposal(MaterialProposalViewModel materialProposalViewModel, string materials)
        {
            var existed =
                _materialProposalService.GetMaterialProposals().FirstOrDefault(a => a.ProposalCode == materialProposalViewModel.ProposalCode.ToUpper().Replace("_", "") && a.Materials.Count(m => !m.Deleted) > 0);
            if (existed != null)
            {
                if (existed.Id == materialProposalViewModel.Id)
                {
                    materialProposalViewModel.Id = existed.Id;
                    var src = Mapper.Map<MaterialProposalViewModel, MaterialProposal>(materialProposalViewModel);
                    foreach (PropertyDescriptor item in TypeDescriptor.GetProperties(src))
                    {
                        if (item.Name != "Comments" && item.Name != "Materials" && item.Name != "Sent")
                        {
                            item.SetValue(existed, item.GetValue(src));
                        }
                        if (item.Name == "Sent")
                        {
                            if (!existed.Sent)
                            {
                                item.SetValue(existed, item.GetValue(src));
                            }
                        }
                    }
                    existed.CreatedDate = DateTime.MinValue;
                    existed.CreatedUserName = null;
                    _materialProposalService.AddOrUpdateMaterialProposal(existed);
                    return Json(new {success = true, existed.Id}, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var materialProposal = Mapper.Map<MaterialProposalViewModel, MaterialProposal>(materialProposalViewModel);
                materialProposal.ProposerUserName = materialProposal.ProposerUserName ?? Thread.CurrentPrincipal.Identity.Name;
                var users = usersContext.UserProfiles.ToList();
                var currentUser = users.FirstOrDefault(a => a.UserName == materialProposal.ProposerUserName);
                if (currentUser != null) materialProposal.ProposerDepartmentId = currentUser.DepartmentId;
                _materialProposalService.AddOrUpdateMaterialProposal(materialProposal);
                return Json(new { success = true, materialProposal.Id }, JsonRequestBehavior.AllowGet);
            }
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
            var users = usersContext.UserProfiles.ToList();
            var user = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            var usersSameDepartment = users.Where(u => user != null && u.DepartmentId == user.DepartmentId).Select(u => u.UserName);
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
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
                        break;
                    case "ReceiveAndAwaitingApprove":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                !a.Approved &&
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
                        break;
                    case "ApprovedAwaitingReceive":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                a.MaterialProposal.ProposerDepartmentId == user.DepartmentId &&
                                (a.AssignInfoes.Count == 0 || a.AssignInfoes.Count(assign => !assign.Finished) != 0) &&
                                a.Approved &&
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
                        break;
                    case "ApprovedAwaitingImplement":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                (a.ImplementerDepartmentIds.StartsWith(user.DepartmentId + ",") || a.ImplementerDepartmentIds.Contains("," + user.DepartmentId + ",") || a.ImplementerDepartmentIds.EndsWith("," + user.DepartmentId) || a.ImplementerDepartmentIds == user.DepartmentId.ToString()) &&
                                (usersSameDepartment.Count(username => a.ImplementerUserNames.StartsWith(username + ",") || a.ImplementerUserNames.Contains("," + username + ",") || a.ImplementerUserNames.EndsWith("," + username) || a.ImplementerUserNames == username) <= 0) &&
                                (a.AssignInfoes.Count(assign => assign.DepartmentId == user.DepartmentId) == 0 || !a.AssignInfoes.FirstOrDefault(assign => assign.DepartmentId == user.DepartmentId).Finished) &&
                                a.Approved &&
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
                        break;
                    case "ApprovedImplemented":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                (a.ImplementerDepartmentIds.StartsWith(user.DepartmentId + ",") || a.ImplementerDepartmentIds.Contains("," + user.DepartmentId + ",") || a.ImplementerDepartmentIds.EndsWith("," + user.DepartmentId) || a.ImplementerDepartmentIds == user.DepartmentId.ToString()) &&
                                (usersSameDepartment.Count(username => a.ImplementerUserNames.StartsWith(username + ",") || a.ImplementerUserNames.Contains("," + username + ",") || a.ImplementerUserNames.EndsWith("," + username) || a.ImplementerUserNames == username) > 0) &&
                                (a.AssignInfoes.Count(assign => assign.DepartmentId == user.DepartmentId) == 0 || !a.AssignInfoes.FirstOrDefault(assign => assign.DepartmentId == user.DepartmentId).Finished) &&
                                a.Approved &&
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
                        break;
                    case "ApprovedAssigned":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                !string.IsNullOrEmpty(a.ImplementerDepartmentIds) &&
                                (a.AssignInfoes.Count == 0 || a.AssignInfoes.Count(assign => (a.ImplementerDepartmentIds.StartsWith(assign.DepartmentId + ",") || a.ImplementerDepartmentIds.Contains("," + assign.DepartmentId + ",") || a.ImplementerDepartmentIds.EndsWith("," + assign.DepartmentId) || a.ImplementerDepartmentIds == assign.DepartmentId.ToString()) && !assign.Finished) != 0) &&
                                a.Approved &&
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
                        break;
                    case "AssignedFinished":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                !string.IsNullOrEmpty(a.ImplementerUserNames) &&
                                a.AssignInfoes.Any() && a.AssignInfoes.Count(assign =>  (a.ImplementerDepartmentIds.StartsWith(assign.DepartmentId + ",") || a.ImplementerDepartmentIds.Contains("," + assign.DepartmentId + ",") || a.ImplementerDepartmentIds.EndsWith("," + assign.DepartmentId) || a.ImplementerDepartmentIds == assign.DepartmentId.ToString()) && !assign.Finished) == 0 &&
                                a.Approved &&
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
                        break;
                    case "Finished":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                (a.ImplementerDepartmentIds.StartsWith(user.DepartmentId + ",") || a.ImplementerDepartmentIds.Contains("," + user.DepartmentId + ",") || a.ImplementerDepartmentIds.EndsWith("," + user.DepartmentId) || a.ImplementerDepartmentIds == user.DepartmentId.ToString()) &&
                                !string.IsNullOrEmpty(a.ImplementerUserNames) &&
                                a.AssignInfoes.FirstOrDefault(assign => assign.DepartmentId == user.DepartmentId).Finished &&
                                a.Approved &&
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
                        break;
                    case "Received":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                a.MaterialProposal.ProposerDepartmentId == user.DepartmentId &&
                                !string.IsNullOrEmpty(a.ImplementerUserNames) &&
                                a.AssignInfoes.Any() && a.AssignInfoes.Count(assign =>  (a.ImplementerDepartmentIds.StartsWith(assign.DepartmentId + ",") || a.ImplementerDepartmentIds.Contains("," + assign.DepartmentId + ",") || a.ImplementerDepartmentIds.EndsWith("," + assign.DepartmentId) || a.ImplementerDepartmentIds == assign.DepartmentId.ToString()) && !assign.Finished) == 0 &&
                                a.Approved &&
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
                        break;
                    case "BeAssignedUnfinished":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                (a.ImplementerUserNames.StartsWith(Thread.CurrentPrincipal.Identity.Name + ",") || a.ImplementerUserNames.Contains("," + Thread.CurrentPrincipal.Identity.Name + ",") || a.ImplementerUserNames.EndsWith("," + Thread.CurrentPrincipal.Identity.Name) || a.ImplementerUserNames == Thread.CurrentPrincipal.Identity.Name) &&
                                (a.AssignInfoes.Count(assign => assign.DepartmentId == user.DepartmentId) == 0 || !a.AssignInfoes.FirstOrDefault(assign => assign.DepartmentId == user.DepartmentId).Finished) &&
                                a.Approved &&
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
                        break;
                    case "BeAssignedFinished":
                        materials = _materialService.GetMaterials(pageNumber ?? 1, pageSize ?? 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                (a.ImplementerUserNames.StartsWith(Thread.CurrentPrincipal.Identity.Name + ",") || a.ImplementerUserNames.Contains("," + Thread.CurrentPrincipal.Identity.Name + ",") || a.ImplementerUserNames.EndsWith("," + Thread.CurrentPrincipal.Identity.Name) || a.ImplementerUserNames == Thread.CurrentPrincipal.Identity.Name) &&
                                a.AssignInfoes.FirstOrDefault(assign => assign.DepartmentId == user.DepartmentId).Finished &&
                                a.Approved &&
                                a.MaterialProposal.Sent &&
                                !a.Deleted);
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
                    var proposalDeparmentComments = !(model.ImplementerDepartmentIds.StartsWith(model.ProposerDepartmentId + ",") || model.ImplementerDepartmentIds.Contains("," + model.ProposerDepartmentId + ",") || model.ImplementerDepartmentIds.EndsWith("," + model.ProposerDepartmentId) || model.ImplementerDepartmentIds == model.ProposerDepartmentId.ToString()) ? 
                        materialViewModel.Comments.Where(
                            a =>
                            {
                                var poster = usersContext.UserProfiles.ToList().FirstOrDefault(u => u.UserName == a.PosterUserName);
                                return poster != null && model.ProposerDepartmentId == poster.DepartmentId && !Roles.IsUserInRole(a.PosterUserName, "Admin");
                            }) : 
                            materialViewModel.Comments.Where(
                            a =>
                            {
                                var poster = usersContext.UserProfiles.ToList().FirstOrDefault(u => u.UserName == a.PosterUserName);
                                return poster != null && model.ProposerDepartmentId == poster.DepartmentId && !(model.ImplementerUserNames.StartsWith(a.PosterUserName + ",") || model.ImplementerUserNames.Contains("," + a.PosterUserName + ",") || model.ImplementerUserNames.EndsWith("," + a.PosterUserName) || model.ImplementerUserNames == a.PosterUserName) && !Roles.IsUserInRole(a.PosterUserName, "Admin");
                            });
                    materialViewModel.ProposalDeparmentComments = string.Join("<br/> ",
                        proposalDeparmentComments.Select(a => a.PosterDisplayName + " (" + a.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss") + "): " + "\"" + a.Content + "\""));
                    var lastProposalDeparmentComment = proposalDeparmentComments.LastOrDefault();
                    if (lastProposalDeparmentComment != null)
                    {
                        materialViewModel.LastProposalDeparmentComment = lastProposalDeparmentComment.Content.Length > 50 ? lastProposalDeparmentComment.Content.Substring(0, 50) + "..." : lastProposalDeparmentComment.Content;
                        materialViewModel.LastProposalDeparmentCommentReadClass = lastProposalDeparmentComment.ReadUserNames.Contains(user.UserName) ? "" : "unread";
                    }
                    else
                    {
                        materialViewModel.LastProposalDeparmentComment = "";
                    }
                    var implementDepartmentComments = !(model.ImplementerDepartmentIds.StartsWith(model.ProposerDepartmentId + ",") || model.ImplementerDepartmentIds.Contains("," + model.ProposerDepartmentId + ",") || model.ImplementerDepartmentIds.EndsWith("," + model.ProposerDepartmentId) || model.ImplementerDepartmentIds == model.ProposerDepartmentId.ToString()) ? 
                        materialViewModel.Comments.Where(
                            a =>
                            {
                                var poster = usersContext.UserProfiles.ToList().FirstOrDefault(u => u.UserName == a.PosterUserName);
                                return poster != null && (model.ImplementerDepartmentIds.StartsWith(poster.DepartmentId + ",") || model.ImplementerDepartmentIds.Contains("," + poster.DepartmentId + ",") || model.ImplementerDepartmentIds.EndsWith("," + poster.DepartmentId) || model.ImplementerDepartmentIds == poster.DepartmentId.ToString()) && !Roles.IsUserInRole(a.PosterUserName, "Admin");
                            }) :
                            materialViewModel.Comments.Where(
                            a =>
                            {
                                var poster = usersContext.UserProfiles.ToList().FirstOrDefault(u => u.UserName == a.PosterUserName);
                                return poster != null &&
                                    (model.ImplementerDepartmentIds.StartsWith(poster.DepartmentId + ",") || model.ImplementerDepartmentIds.Contains("," + poster.DepartmentId + ",") || model.ImplementerDepartmentIds.EndsWith("," + poster.DepartmentId) || model.ImplementerDepartmentIds == poster.DepartmentId.ToString()) &&
                                    (model.ImplementerUserNames.StartsWith(a.PosterUserName + ",") || model.ImplementerUserNames.Contains("," + a.PosterUserName + ",") || model.ImplementerUserNames.EndsWith("," + a.PosterUserName) || model.ImplementerUserNames == a.PosterUserName) && !Roles.IsUserInRole(a.PosterUserName, "Admin");
                            });
                    materialViewModel.ImplementDepartmentComments = string.Join("<br/> ",
                        implementDepartmentComments.Select(a => a.PosterDisplayName + " (" + a.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss") + "): " + "\"" + a.Content + "\""));
                    var lastImplementDepartmentComment = implementDepartmentComments.LastOrDefault();
                    if (lastImplementDepartmentComment != null)
                    {
                        materialViewModel.LastImplementDepartmentComment = lastImplementDepartmentComment.Content.Length > 50 ? lastImplementDepartmentComment.Content.Substring(0, 50) + "..." : lastImplementDepartmentComment.Content;
                        materialViewModel.LastImplementDepartmentCommentReadClass = lastImplementDepartmentComment.ReadUserNames.Contains(user.UserName) ? "" : "unread";
                    }
                    else
                    {
                        materialViewModel.LastImplementDepartmentComment = "";
                    }
                    var generalManagerComments = materialViewModel.Comments.Where(a => Roles.IsUserInRole(a.PosterUserName, "Admin"));
                    materialViewModel.GeneralManagerComments = string.Join("<br/> ",
                        generalManagerComments.Select(a => a.PosterDisplayName + " (" + a.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss") + "): " + "\"" + a.Content + "\""));
                    var lastGeneralManagerComment = generalManagerComments.LastOrDefault();
                    if (lastGeneralManagerComment != null)
                    {
                        materialViewModel.LastGeneralManagerComment = lastGeneralManagerComment.Content.Length > 50
                            ? lastGeneralManagerComment.Content.Substring(0, 50) + "..."
                            : lastGeneralManagerComment.Content;
                        materialViewModel.LastGeneralManagerCommentReadClass =
                            lastGeneralManagerComment.ReadUserNames.Contains(user.UserName) ? "" : "unread";
                    }
                    else
                    {
                        materialViewModel.LastGeneralManagerComment = "";
                    }

                    //materialViewModel.Description = materialViewModel.Description.Length > 50
                    //        ? materialViewModel.Description.Substring(0, 50) + "..."
                    //        : materialViewModel.Description;

                    if (!Thread.CurrentPrincipal.IsInRole("Admin") && materialViewModel.ImplementerDepartmentIds.Split(',').Contains(user.DepartmentId.ToString()))
                    {
                        var assignInfo =
                            materialViewModel.AssignInfoes.FirstOrDefault(a => a.DepartmentId == user.DepartmentId);
                        if (assignInfo != null)
                        {
                            materialViewModel.StartDate = assignInfo.StartDate;
                            materialViewModel.FinishDate = assignInfo.FinishDate;
                            materialViewModel.ProgressStatusId = assignInfo.ProgressStatusId;
                            materialViewModel.Finished = assignInfo.Finished;
                        }
                        else
                        {
                            materialViewModel.StartDate = null;
                            materialViewModel.FinishDate = null;
                            materialViewModel.ProgressStatusId = 0;
                            materialViewModel.Finished = false;
                        }
                    }
                }
                return Json(new { data = materialViewModels, total = materials.TotalItemCount },
                    JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddOrUpdateMaterial(string materials, int? materialProposalId)
        {
            var materialViewModels = JsonConvert.DeserializeObject<List<MaterialViewModel>>(materials);
            foreach (var materialViewModel in materialViewModels)
            {
                var material = Mapper.Map<MaterialViewModel, Material>(materialViewModel);
                material.MaterialProposalId = materialProposalId ?? material.MaterialProposalId;
                material.Deadline = material.Deadline != null
                    ? ((DateTime) material.Deadline).AddHours(7)
                    : material.Deadline;
                material.ApproveDate = material.ApproveDate != null
                    ? ((DateTime)material.ApproveDate).AddHours(7)
                    : material.ApproveDate;
                material.StartDate = material.StartDate != null
                    ? ((DateTime)material.StartDate).AddHours(7)
                    : material.StartDate;
                material.FinishDate = material.FinishDate != null
                    ? ((DateTime)material.FinishDate).AddHours(7)
                    : material.FinishDate;
                var users = usersContext.UserProfiles.ToList();
                var currentUser = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
                if (currentUser != null)
                {
                    var usersSameDepartment = Thread.CurrentPrincipal.IsInRole("Admin") ? users.Select(a => a.UserName).ToList() : users.Where(a => a.DepartmentId == currentUser.DepartmentId).Select(a => a.UserName).ToList();
                    var existedMaterial = _materialService.GetMaterials().FirstOrDefault(a => a.Id == material.Id);
                    if (existedMaterial != null)
                    {
                        var implementerUserNames = existedMaterial.ImplementerUserNames.Split(',').ToList();
                        foreach (var userSameDepartment in usersSameDepartment)
                        {
                            implementerUserNames.Remove(userSameDepartment);
                        }
                        implementerUserNames.AddRange(material.ImplementerUserNames.Split(','));
                        material.ImplementerUserNames = string.Join(",", implementerUserNames.Distinct().Where(a => !string.IsNullOrEmpty(a)));
                        material.Description = material.Description.Length >= 3 && material.Description.Substring(material.Description.Length - 3) == "..."
                            ? existedMaterial.Description
                            : material.Description;
                        if (material.Approved && !existedMaterial.Approved)
                        {
                            material.ApproveDate = DateTime.Now;
                        }
                        if (!Thread.CurrentPrincipal.IsInRole("Admin"))
                        {
                            using (var _dbContext = new DatabaseFactory().Get())
                            {
                                var _dbSet = _dbContext.Set<Material>();
                                _dbSet.Attach(existedMaterial);
                                var asignInfo =
                                    existedMaterial.AssignInfoes.FirstOrDefault(a => a.DepartmentId == currentUser.DepartmentId);
                                if (asignInfo != null)
                                {
                                    asignInfo.StartDate = material.StartDate;
                                    asignInfo.FinishDate = material.FinishDate;
                                    asignInfo.ProgressStatusId = material.ProgressStatusId;
                                    asignInfo.Finished = material.Finished;
                                }
                                else
                                {
                                    existedMaterial.AssignInfoes.Add(new AssignInfo
                                    {
                                        DepartmentId = currentUser.DepartmentId,
                                        StartDate = material.StartDate,
                                        FinishDate = material.FinishDate,
                                        ProgressStatusId = material.ProgressStatusId,
                                        Finished = material.Finished
                                    });
                                }
                                _dbContext.Entry(existedMaterial).State = EntityState.Modified;
                                _dbContext.Commit();
                            }
                        }
                    }
                }
                material.Finished = material.AssignInfoes.Count(a => !a.Finished) == 0;
                _materialService.AddOrUpdateMaterial(material);
            }
            return Json(materialViewModels, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult DeleteMaterial(string materials)
        {
            var materialViewModels = JsonConvert.DeserializeObject<List<MaterialViewModel>>(materials);
            foreach (var materialViewModel in materialViewModels)
            {
                var material = _materialService.GetMaterial(materialViewModel.Id);
                material.Deleted = true;
                _materialService.AddOrUpdateMaterial(material);
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
        public ActionResult AddComment(string content, string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                foreach (var id in ids.Replace("multiselect-all,", "").Split(','))
                {
                    var comment = new Comment { Content = content, PosterUserName = Thread.CurrentPrincipal.Identity.Name };
                    var material = _materialService.GetMaterial(Convert.ToInt32(id));
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
                }
                return Json(new CommentViewModel{Content = "OK"}, JsonRequestBehavior.AllowGet);
            }
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
                                 (a.ImplementerDepartmentId == user.DepartmentId || a.ImplementerDepartmentIds.StartsWith(user.DepartmentId + ",") || a.ImplementerDepartmentIds.Contains("," + user.DepartmentId + ",") || a.ImplementerDepartmentIds.EndsWith("," + user.DepartmentId) || a.ImplementerDepartmentIds == user.DepartmentId.ToString()) &&
                                 string.IsNullOrEmpty(a.ImplementerUserName) &&
                                 !a.Finished &&
                                 a.Approved &&
                                 a.MaterialProposal.Sent);
                        break;
                    case "ApprovedImplemented":
                        materials = _materialService.GetMaterials(1, 1,
                            a => (a.MaterialProposal.Id == id || id == null) &&
                                 (a.ImplementerDepartmentId == user.DepartmentId || a.ImplementerDepartmentIds.StartsWith(user.DepartmentId + ",") || a.ImplementerDepartmentIds.Contains("," + user.DepartmentId + ",") || a.ImplementerDepartmentIds.EndsWith("," + user.DepartmentId) || a.ImplementerDepartmentIds == user.DepartmentId.ToString()) &&
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
                                 (a.ImplementerDepartmentId == user.DepartmentId || a.ImplementerDepartmentIds.StartsWith(user.DepartmentId + ",") || a.ImplementerDepartmentIds.Contains("," + user.DepartmentId + ",") || a.ImplementerDepartmentIds.EndsWith("," + user.DepartmentId) || a.ImplementerDepartmentIds == user.DepartmentId.ToString()) &&
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

        [HttpPost]
        public ActionResult ImportExcel(IEnumerable<HttpPostedFileBase> files, int? materialProposalId)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.ContentLength > 0)
                    {
                        var fileExtension = Path.GetExtension(file.FileName);

                        if (fileExtension == ".xls" || fileExtension == ".xlsx")
                        {
                            var fileLocation = Server.MapPath("~/App_Data/") + file.FileName;
                            if (System.IO.File.Exists(fileLocation))
                            {

                                System.IO.File.Delete(fileLocation);
                            }
                            file.SaveAs(fileLocation);

                            FileStream stream = System.IO.File.Open(fileLocation, FileMode.Open, FileAccess.Read);

                            IExcelDataReader excelReader = fileExtension == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream);
                           
                            excelReader.IsFirstRowAsColumnNames = true;
                            DataSet result = excelReader.AsDataSet();

                            var readUserNames = new List<string>();
                            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
                            if (user != null)
                            {
                                readUserNames.Add(user.UserName);
                            }
                            var materials = result.Tables[0].AsEnumerable().Select(dataRow => user != null && dataRow[0] != DBNull.Value ? new Material
                                                                                                             {
                                                                                                                 MaterialName = dataRow[0] != DBNull.Value ? (string) dataRow[0] : "",
                                                                                                                 Description = dataRow[1] != DBNull.Value ? (string) dataRow[1] : "",
                                                                                                                 Quantity = dataRow[2] != DBNull.Value ? Convert.ToDecimal((double)dataRow[2]) : 0,
                                                                                                                 Unit = dataRow[3] != DBNull.Value ? (string) dataRow[3] : "",
                                                                                                                 InventoryQuantity = dataRow[4] != DBNull.Value ? Convert.ToDecimal((double) dataRow[4]) : 0,
                                                                                                                 Used = dataRow[5] != DBNull.Value && (double) dataRow[5] == 1,
                                                                                                                 Deadline = dataRow[6] != DBNull.Value ? DateTime.ParseExact((string) dataRow[6], "dd/MM/yyyy", new CultureInfo("en-GB")) : DateTime.MinValue,
                                                                                                                 UsingPurpose = dataRow[7] != DBNull.Value ? (string)dataRow[7] : "",
                                                                                                                 Comments = dataRow[8] != DBNull.Value ? new List<Comment> { new Comment { Approved = true, Content = (string)dataRow[8], PosterUserName = Thread.CurrentPrincipal.Identity.Name, PosterDisplayName = user.DisplayName, ReadUserNames = string.Join(",", readUserNames) } } : new List<Comment>()
                                                                                                             } : null).Where(a => a != null).ToList();

                            excelReader.Close();

                            foreach (var material in materials)
                            {
                                material.MaterialProposalId = materialProposalId ?? material.MaterialProposalId;
                                material.Deadline = material.Deadline != null
                                    ? ((DateTime)material.Deadline).AddHours(7)
                                    : material.Deadline;
                                material.ApproveDate = material.ApproveDate != null
                                    ? ((DateTime)material.ApproveDate).AddHours(7)
                                    : material.ApproveDate;
                                material.StartDate = material.StartDate != null
                                    ? ((DateTime)material.StartDate).AddHours(7)
                                    : material.StartDate;
                                material.FinishDate = material.FinishDate != null
                                    ? ((DateTime)material.FinishDate).AddHours(7)
                                    : material.FinishDate;
                                material.ImplementerDepartmentIds = material.ImplementerDepartmentIds ?? "";
                                material.ImplementerUserNames = material.ImplementerUserNames ?? "";
                                var users = usersContext.UserProfiles.ToList();
                                var currentUser = users.FirstOrDefault(a => a.UserName == material.ImplementerUserName);
                                if (currentUser != null) material.ImplementerDepartmentId = currentUser.DepartmentId;
                                _materialService.AddOrUpdateMaterial(material);
                            }

                            return Json(Mapper.Map<List<Material>, List<MaterialViewModel>>(materials));
                        }

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

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
    }
}
