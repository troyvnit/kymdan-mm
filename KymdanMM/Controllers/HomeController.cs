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

namespace KymdanMM.Controllers
{
    public class HomeController : Controller
    {
        private IMaterialProposalService _materialProposalService { get; set; }
        private IDepartmentService _departmentService { get; set; }
        private UsersContext usersContext { get; set; }

        public HomeController(IMaterialProposalService _materialProposalService, IDepartmentService _departmentService)
        {
            this._materialProposalService = _materialProposalService;
            this._departmentService = _departmentService;
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

        public ActionResult GetMaterialProposal()
        {
            var materialProposals = _materialProposalService.GetMaterialProposals().Select(Mapper.Map<MaterialProposal, MaterialProposalViewModel>);
            return Json(materialProposals, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddOrUpdateMaterialProposal(int? id)
        {
            var materialProposal = id != null ? _materialProposalService.GetMaterialProposal((int)id) : new MaterialProposal();
            var materialProposalViewModel = materialProposal != null ? Mapper.Map<MaterialProposal, MaterialProposalViewModel>(materialProposal) : new MaterialProposalViewModel();
            materialProposalViewModel.ProposerUserName = Thread.CurrentPrincipal.Identity.Name;
            return View(materialProposalViewModel);
        }

        [HttpPost]
        public ActionResult AddOrUpdateMaterialProposal(MaterialProposalViewModel materialProposalViewModel)
        {
            var materialProposal = Mapper.Map<MaterialProposalViewModel, MaterialProposal>(materialProposalViewModel);
            materialProposal.ProposerUserName  = Thread.CurrentPrincipal.Identity.Name;
            _materialProposalService.AddOrUpdateMaterialProposal(materialProposal);
            return RedirectToAction("AddOrUpdateMaterialProposal", new { id = materialProposal.Id });
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
