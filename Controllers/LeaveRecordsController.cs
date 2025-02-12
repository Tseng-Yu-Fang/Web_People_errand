﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AttendanceManagement.Models;

namespace AttendanceManagement.Controllers
{
    public class LeaveRecordsController : Controller
    {
        // GET: LeaveRecords
        public async Task<ActionResult> Index()
        {
            if (Session["company_hash"] == null)
            {
                return RedirectToAction("Index", "Account", null);
            }
            //輸入公司代碼取得待審核請假申請紀錄
            List<LeaveRecord> review_leaverecord = await ReviewLeaveRecordModel.Get_ReviewLeaveRecord(Session["company_hash"].ToString());
            //輸入公司代碼取得已審核請假申請紀錄
            List<LeaveRecord> pass_leaverecord = await PassLeaveRecordModel.Get_PassLeaveRecord(Session["company_hash"].ToString());

            ViewBag.review_leaverecord = review_leaverecord;//待審核請假申請紀錄
            ViewBag.pass_leaverecord = pass_leaverecord;//待審核請假申請紀錄
            return View();
        }

        public async Task<ActionResult> Edit(int id)
        {
            if (Session["company_hash"] == null)
            {
                return RedirectToAction("Index", "Account", null);
            }
            //輸入公司代碼取得已審核請假申請紀錄
            List<LeaveRecord> pass_leaverecord = await PassLeaveRecordModel.Get_PassLeaveRecord(Session["company_hash"].ToString());
            int num = pass_leaverecord.FindIndex(item => item.LeaveRecordId == id);//員工索引值
            //輸入公司代碼取得假別
            List<LeaveType> leaveTypes = await LeaveTypeModel.Get_LeaveType(Session["company_hash"].ToString());

            ViewBag.pass_leaverecord = pass_leaverecord;//待審核請假申請紀錄
            ViewBag.leaveTypes = leaveTypes;//假別
            ViewBag.Num = num;//員工索引值
            return View("Edit");
        }

        public async Task<ActionResult> Check(int id)
        {
            if (Session["company_hash"] == null)
            {
                return RedirectToAction("Index", "Account", null);
            }
            //輸入公司代碼取得未審核請假申請紀錄
            List<LeaveRecord> review_leaverecord = await ReviewLeaveRecordModel.Get_ReviewLeaveRecord(Session["company_hash"].ToString());
            //輸入公司代碼取得假別
            List<LeaveType> leaveTypes = await LeaveTypeModel.Get_LeaveType(Session["company_hash"].ToString());
            int num = review_leaverecord.FindIndex(item => item.LeaveRecordId == id);//員工索引值

            ViewBag.review_leaverecord = review_leaverecord;//待審核請假申請紀錄
            ViewBag.leaveTypes = leaveTypes;//假別
            ViewBag.Num = num;//員工索引值
            return View("Check");
        }
        [HttpPost]//公差審核頁面，審核按鈕
        public async Task<ActionResult> ReviewLeaveRecord(int leave_id,string id, string Button)
        {
            if (Session["company_hash"] == null)
            {
                return RedirectToAction("Index", "Account", null);
            }
            //輸入公司代碼取得已審核員工資料
            List<PassEmployee> passEmployees = await PassEmployeeModel.PassEmployees(Session["company_hash"].ToString());
            int Index = passEmployees.FindIndex(item => item.HashAccount.Equals(id)); ;//員工索引值，用來找出員工EMAIL

            bool result = false;

            if (Button.Equals("SaveButton"))
            {
                result = await Review_LeaveRecordModel.ReviewLeaveRecord(leave_id, true);//PUT更新審核狀態

                if (result)
                {
                    AttendanceManagement.Models.HttpResponse.sendGmail(passEmployees[Index].Email, "差勤打卡公差審核通知", "<h1>差勤打卡公差審核成功</h1><p>請至差勤打卡APP公差紀錄進行確認，如有問題請連繫後台。</p>");
                    return RedirectToAction("index");
                }
                else
                    return Content("<script>alert('審核失敗！如有問題請連繫後台');history.go(-1);</script>");
            }
            else
            {
                result = await Review_LeaveRecordModel.ReviewLeaveRecord(leave_id, false);//PUT更新審核狀態

                if (result)
                {
                    AttendanceManagement.Models.HttpResponse.sendGmail(passEmployees[Index].Email, "差勤打卡公差審核通知", "<h1>差勤打卡公差審核失敗</h1><p>請至差勤打卡APP公差紀錄進行確認，如有問題請連繫後台。</p>");
                    return RedirectToAction("index");
                }
                else
                    return Content("<script>alert('審核失敗！如有問題請連繫後台');history.go(-1);</script>");
            }

        }
        [HttpPost]//公差修改頁面，修改按鈕
        public async Task<ActionResult> EditLeaveRecord(int num,int id,string Button, bool? review)
        {

            //輸入公司代碼取得已審核請假申請紀錄
            List<LeaveRecord> pass_leaverecord = await PassLeaveRecordModel.Get_PassLeaveRecord(Session["company_hash"].ToString());
            string hashaccount = pass_leaverecord[num].HashAccount;//員工編號

            //輸入公司代碼取得已審核員工資料
            List<PassEmployee> passEmployees = await PassEmployeeModel.PassEmployees(Session["company_hash"].ToString());
            int Index = passEmployees.FindIndex(item => item.HashAccount.Equals(hashaccount)); ;//員工索引值，用來找出員工EMAIL
            bool result = false;

            if (Button.Equals("RenewButton"))
            {

                result = await Review_LeaveRecordModel.ReviewLeaveRecord(id,(bool)review);//PUT更新審核狀態

                if (result)
                {
                    AttendanceManagement.Models.HttpResponse.sendGmail(passEmployees[Index].Email, "差勤打卡請假紀錄變更通知", "<h1>您的請假狀態已變更</h1><p>請至差勤打卡APP公差紀錄確認變更內容，如有問題請連繫後台。</p>");
                    return RedirectToAction("index");
                }
                else
                    return Content("<script>alert('變更失敗！如有問題請連繫後台');history.go(-1);</script>");
            }
            //string url = "/TripRecords/Index?id=" + id + "&startdate=" + startdate + "&enddate=" + enddate + "location=" + location + "reason=" + reason;
            return RedirectToAction("index");
        }

        [HttpGet]//請假紀錄篩選
        public async Task<ActionResult> SearchLeaveRecord(string employee_name, DateTime? date)
        {
            if (Session["company_hash"] == null)
            {
                return RedirectToAction("Index", "Account", null);
            }

            //輸入公司代碼取得待審核請假申請紀錄
            List<LeaveRecord> review_leaverecord = await ReviewLeaveRecordModel.Get_ReviewLeaveRecord(Session["company_hash"].ToString());
            //輸入公司代碼取得已審核請假申請紀錄
            List<LeaveRecord> pass_leaverecord = await PassLeaveRecordModel.Get_PassLeaveRecord(Session["company_hash"].ToString());
            //放入篩選後的已審核資料
            List<LeaveRecord> search_leaverecord = new List<LeaveRecord>();

            if (date.Equals(null) && employee_name.Equals(""))//沒有輸入篩選條件就按搜尋，顯示全部資料
                return RedirectToAction("index");
            else if (!date.Equals(null) && !employee_name.Equals(""))//兩個篩選條件都輸入
                search_leaverecord = await PassLeaveRecordModel.Search_LeaveRecord2(Session["company_hash"].ToString(), date, employee_name);
            else search_leaverecord = await PassLeaveRecordModel.Search_LeaveRecord1(Session["company_hash"].ToString(), date, employee_name);//只輸入一個篩選條件

            ViewBag.review_leaverecord = review_leaverecord;//待審核請假申請紀錄
            ViewBag.pass_leaverecord = search_leaverecord;//待審核請假申請紀錄
            return View("Index");
        }
    }
}