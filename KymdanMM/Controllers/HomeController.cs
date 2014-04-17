﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AutoMapper;
using KymdanMM.Data.Service;
using KymdanMM.Filters;
using KymdanMM.Model.Models;
using KymdanMM.Models;
using Newtonsoft.Json;
using PagedList;

namespace KymdanMM.Controllers
{
    [InitializeSimpleMembership]
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
            ViewBag.Departments = _departmentService.GetDepartments();
            return View();
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

        public ActionResult GetProgressStatus()
        {
            var progressStatuses = _progressStatusService.GetProgressStatuses();
            return Json(progressStatuses, JsonRequestBehavior.AllowGet);
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
                        a => (a.ApproveStatus != ApproveStatus.Unapproved) &&
                            (a.ImplementerDepartmentId == departmentId || a.ProposerDepartmentId == departmentId || departmentId == null) &&
                            (a.ProgressStatusId == progressStatusId || progressStatusId == null) &&
                            (a.ApproveStatus == approveStatus || approveStatus == null) &&
                            (a.ProposalCode.Contains(keyWord) || a.Description.Contains(keyWord) || a.Materials.Count(m => m.MaterialName.Contains(keyWord)) > 0 || string.IsNullOrEmpty(keyWord)));
                }
                else if (Thread.CurrentPrincipal.IsInRole("Department Manager"))
                {
                    materialProposals = _materialProposalService.GetMaterialProposals(pageNumber, pageSize,
                        a => (a.ImplementerDepartmentId == user.DepartmentId || a.ProposerDepartmentId == user.DepartmentId) &&
                            (a.ProgressStatusId == progressStatusId || progressStatusId == null) &&
                            (a.ApproveStatus == approveStatus || approveStatus == null) &&
                            (a.ProposalCode.Contains(keyWord) || a.Description.Contains(keyWord) || a.Materials.Count(m => m.MaterialName.Contains(keyWord)) > 0 || string.IsNullOrEmpty(keyWord)));
                }
                else
                {
                    materialProposals = _materialProposalService.GetMaterialProposals(pageNumber, pageSize,
                        a => (a.ImplementerUserName == user.UserName || a.ProposerUserName == user.UserName) &&
                            (a.ProgressStatusId == progressStatusId || progressStatusId == null) &&
                            (a.ApproveStatus == approveStatus || approveStatus == null) &&
                            (a.ProposalCode.Contains(keyWord) || a.Description.Contains(keyWord) || a.Materials.Count(m => m.MaterialName.Contains(keyWord)) > 0 || string.IsNullOrEmpty(keyWord)));
                }
                var data = materialProposals.Select(Mapper.Map<MaterialProposal, MaterialProposalViewModel>).ToList();
                foreach (var materialProposalViewModel in data)
                {
                    var implementerUser =
                        usersContext.UserProfiles.ToList()
                            .FirstOrDefault(a => a.UserName == materialProposalViewModel.ImplementerUserName);
                    if (implementerUser != null)
                    {
                        materialProposalViewModel.ImplementerDisplayName = implementerUser.DisplayName;
                    }

                    var proposerUser =
                        usersContext.UserProfiles.ToList()
                            .FirstOrDefault(a => a.UserName == materialProposalViewModel.ProposerUserName);
                    if (proposerUser != null)
                    {
                        materialProposalViewModel.ProposerDisplayName = proposerUser.DisplayName;
                    }

                    var progressStatus = _progressStatusService.GetProgressStatus(materialProposalViewModel.ProgressStatusId);
                    materialProposalViewModel.Status = progressStatus.Status;
                }
                return Json(new { data, total = materialProposals.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult AddOrUpdateMaterialProposal(int? id)
        {
            if ((id == null || id == 0) && !Thread.CurrentPrincipal.IsInRole("Member"))
            {
                return RedirectToAction("AccessDenied");
            }
            var materialProposal = id != null ? _materialProposalService.GetMaterialProposal((int)id) : new MaterialProposal();
            var materialProposalViewModel = materialProposal != null ? Mapper.Map<MaterialProposal, MaterialProposalViewModel>(materialProposal) : new MaterialProposalViewModel();
            materialProposalViewModel.ProposerUserName = materialProposalViewModel.ProposerUserName ?? Thread.CurrentPrincipal.Identity.Name;
            var users = usersContext.UserProfiles.ToList();
            var user = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                materialProposalViewModel.ProposerDepartmentId = user.DepartmentId;
                ViewBag.Departments = _departmentService.GetDepartments();
                ViewBag.ProgressStatuses = _progressStatusService.GetProgressStatuses();
                ViewBag.Users = users;
            }
            return View(materialProposalViewModel);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddOrUpdateMaterialProposal(MaterialProposalViewModel materialProposalViewModel, string materials)
        {
            if (!Thread.CurrentPrincipal.IsInRole("Admin"))
            {
                materialProposalViewModel.ImplementerDepartmentId = 0;
            }
            if (!Thread.CurrentPrincipal.IsInRole("Department Manager"))
            {
                materialProposalViewModel.ImplementerUserName = null;
            }
            var materialProposal = Mapper.Map<MaterialProposalViewModel, MaterialProposal>(materialProposalViewModel);
            materialProposal.ProposerUserName = materialProposal.ProposerUserName ?? Thread.CurrentPrincipal.Identity.Name;
            var user = usersContext.UserProfiles.ToList().FirstOrDefault(a => a.UserName == materialProposal.ProposerUserName);
            if (user != null)
            {
                materialProposal.ProposerDepartmentId = user.DepartmentId;
            }
            materialProposal.ImplementerUserName = materialProposalViewModel.ImplementerUserName ??
                                                   materialProposal.ImplementerUserName;
            materialProposal.ImplementerDepartmentId = materialProposalViewModel.ImplementerDepartmentId != 0
                ? materialProposalViewModel.ImplementerDepartmentId
                : materialProposal.ImplementerDepartmentId;
            _materialProposalService.AddOrUpdateMaterialProposal(materialProposal);
            return Json(materialProposal.Id, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult ApproveMaterialProposal(string idString)
        {
            var ids = idString.Split(',');
            foreach (var id in ids)
            {
                var materialProposal = _materialProposalService.GetMaterialProposal(Convert.ToInt32(id));
                if (materialProposal.ApproveStatus == ApproveStatus.Unapproved)
                {
                    materialProposal.ApproveStatus = Thread.CurrentPrincipal.IsInRole("Department Manager") ? ApproveStatus.ManagerApproved 
                        : Thread.CurrentPrincipal.IsInRole("Admin") ? ApproveStatus.AdminApproved : materialProposal.ApproveStatus;
                }
                if (materialProposal.ApproveStatus == ApproveStatus.ManagerApproved &&
                    Thread.CurrentPrincipal.IsInRole("Admin"))
                {
                    materialProposal.ApproveStatus = ApproveStatus.AdminApproved;
                }
                _materialProposalService.AddOrUpdateMaterialProposal(materialProposal);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMaterial(int id, int pageNumber, int pageSize)
        {
            var materials = _materialService.GetMaterials(pageNumber, pageSize, id);
            return Json(new { data = materials.Select(Mapper.Map<Material, MaterialViewModel>), total = materials.TotalItemCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
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
        [Authorize]
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

        public ActionResult AccessDenied()
        {
            ViewBag.Message = "Quyền hạn của bạn không phù hợp xem trang này!";

            return View();
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
