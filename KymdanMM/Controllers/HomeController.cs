using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AutoMapper;
using KymdanMM.Data.Service;
using KymdanMM.Model.Models;
using KymdanMM.Models;
using Newtonsoft.Json;

namespace KymdanMM.Controllers
{
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
            return View();
        }

        public ActionResult GetDepartment()
        {
            var departments = _departmentService.GetDepartments();
            return Json(departments, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUser()
        {
            var users = usersContext.UserProfiles.ToList();
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMaterialProposal(int pageNumber, int pageSize)
        {
            var materialProposals = _materialProposalService.GetMaterialProposals(pageNumber, pageSize);
            return Json(new { data = materialProposals.Select(Mapper.Map<MaterialProposal, MaterialProposalViewModel>), total = materialProposals.TotalItemCount }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult AddOrUpdateMaterialProposal(int? id)
        {
            var materialProposal = id != null ? _materialProposalService.GetMaterialProposal((int)id) : new MaterialProposal();
            var materialProposalViewModel = materialProposal != null ? Mapper.Map<MaterialProposal, MaterialProposalViewModel>(materialProposal) : new MaterialProposalViewModel();
            materialProposalViewModel.ProposerUserName = materialProposalViewModel.ProposerUserName ?? Thread.CurrentPrincipal.Identity.Name;
            var users = usersContext.UserProfiles.ToList();
            var user = users.FirstOrDefault(a => a.UserName == Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                materialProposalViewModel.ProposerDepartmentId = user.DepartmentId;
                ViewBag.Departments = _departmentService.GetDepartments();
                ViewBag.ProgressStatuses = _progressStatusService.GetProgressStatuss();
                ViewBag.Users = users;
            }
            return View(materialProposalViewModel);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddOrUpdateMaterialProposal(MaterialProposalViewModel materialProposalViewModel, string materials)
        {
            //var materialProposalViewModel = JsonConvert.DeserializeObject<MaterialProposalViewModel>(materialProposalJson);
            var materialProposal = Mapper.Map<MaterialProposalViewModel, MaterialProposal>(materialProposalViewModel);
            materialProposal.ProposerUserName = materialProposal.ProposerUserName ?? Thread.CurrentPrincipal.Identity.Name;
            materialProposal.Finished = materialProposal.ProgressStatusId == 1;
            _materialProposalService.AddOrUpdateMaterialProposal(materialProposal);

            //var materialViewModels = JsonConvert.DeserializeObject<List<MaterialViewModel>>(materials);
            //foreach (var materialViewModel in materialViewModels)
            //{
            //    var material = Mapper.Map<MaterialViewModel, Material>(materialViewModel);
            //    material.MaterialProposal = materialProposal;
            //    material.MaterialProposalId = materialProposal.Id;
            //    _materialService.AddOrUpdateMaterial(material);
            //}

            return Json(materialProposal.Id, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetMaterial(int id, int pageNumber, int pageSize)
        {
            var materials = _materialService.GetMaterials(pageNumber, pageSize, id);
            return Json(new { data = materials.Select(Mapper.Map<Material, MaterialViewModel>), total = materials.TotalItemCount }, JsonRequestBehavior.AllowGet);
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
