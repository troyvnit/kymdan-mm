using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using KymdanMM.Data.Service;
using KymdanMM.Model.Models;
using KymdanMM.Models;

namespace KymdanMM.Controllers
{
    public class HomeController : Controller
    {
        private IMaterialProposalService _materialProposalService { get; set; }

        public HomeController(IMaterialProposalService _materialProposalService)
        {
            this._materialProposalService = _materialProposalService;
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetMaterialProposal()
        {
            var materialProposals = _materialProposalService.GetMaterialProposals().Select(Mapper.Map<MaterialProposal, MaterialProposalViewModel>);
            return Json(materialProposals, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddOrUpdateMaterialProposal()
        {
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
