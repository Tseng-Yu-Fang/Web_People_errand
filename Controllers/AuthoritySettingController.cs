﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AttendanceManagement.Models;

namespace AttendanceManagement.Controllers
{
    public class AuthoritySettingController : Controller
    {
        // GET: Authority
        public async Task<ActionResult> Index()
        {
            
            if (Session["company_hash"] == null)
            {
                return RedirectToAction("Index", "Account", null);
            }
            //輸入公司取得全部的管理員
            List<Manager> managers = await CompanyManagerModel.GetAllManager(Session["company_hash"].ToString());
            //輸入公司代碼取得部門資料
            List<Department> department = await DepartmentModel.Get_DepartmentAsync(Session["company_hash"].ToString());
            //輸入公司代碼取得職稱資料
            List<JobTitle> jobtitle = await JobtitleModel.Get_JobtitleAsync(Session["company_hash"].ToString());

            ViewBag.departments = department;//部門名稱
            ViewBag.jobtitles = jobtitle;//職稱
            ViewBag.managers = managers;//全部的管理員
            
            return View();
        }
    }
}