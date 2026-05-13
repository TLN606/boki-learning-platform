using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using Boki_ELearning.Helpers;
using Boki_ELearning.Models;
using Boki_ELearning.Models.ViewModels;

namespace Boki_ELearning.Controllers
{
    public class AccountController : Controller
    {
        private readonly BokiDBEntities1 _db = new BokiDBEntities1();

        public ActionResult Login(string returnUrl = null)
        {
            if (Session[SessionKeys.AccountId] != null)
            {
                return RedirectToLocal(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginIdentity = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
            var account = _db.TAIKHOANs.FirstOrDefault(x =>
                ((x.EMAIL ?? string.Empty).ToLower() == loginIdentity) ||
                ((x.TENDANGNHAP ?? string.Empty).ToLower() == loginIdentity));

            if (account == null || !PasswordHelper.Verify(model.Password, account.MATKHAU))
            {
                ModelState.AddModelError(string.Empty, "Email hoac mat khau khong dung.");
                return View(model);
            }

            var normalizedRole = RoleHelper.Normalize(account.VAITRO);
            var student = _db.HOCSINHs.AsNoTracking().FirstOrDefault(x => x.MATAIKHOAN == account.MATAIKHOAN);

            SignIn(account, student, normalizedRole);

            TempData["Message"] = "Dang nhap thanh cong.";
            return RedirectToLocal(returnUrl);
        }

        public ActionResult Register()
        {
            if (Session[SessionKeys.AccountId] != null)
            {
                return RedirectToAction("Index", "Learning");
            }

            return View(new RegisterViewModel
            {
                ClassOptions = LoadClassOptions()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ClassOptions = LoadClassOptions();
                return View(model);
            }

            var email = (model.Email ?? string.Empty).Trim().ToLowerInvariant();
            var existed = _db.TAIKHOANs.Any(x =>
                ((x.EMAIL ?? string.Empty).ToLower() == email) ||
                ((x.TENDANGNHAP ?? string.Empty).ToLower() == email));

            if (existed)
            {
                ModelState.AddModelError("Email", "Email da duoc su dung.");
                model.ClassOptions = LoadClassOptions();
                return View(model);
            }

            if (!_db.LOPs.AsNoTracking().Any(x => x.MALOP == model.ClassId))
            {
                ModelState.AddModelError("ClassId", "Vui long chon lop hop le.");
                model.ClassOptions = LoadClassOptions();
                return View(model);
            }

            TAIKHOAN account = null;
            try
            {
                using (var scope = new TransactionScope())
                {
                    account = new TAIKHOAN
                    {
                        EMAIL = email,
                        TENDANGNHAP = email,
                        MATKHAU = PasswordHelper.Hash(model.Password),
                        VAITRO = RoleHelper.Student
                    };

                    _db.TAIKHOANs.Add(account);
                    _db.SaveChanges();

                    var student = new HOCSINH
                    {
                        HOTEN = (model.FullName ?? string.Empty).Trim(),
                        MALOP = model.ClassId,
                        MATAIKHOAN = account.MATAIKHOAN
                    };

                    _db.HOCSINHs.Add(student);
                    _db.SaveChanges();

                    scope.Complete();
                    SignIn(account, student, RoleHelper.Student);

                    TempData["Message"] = "Dang ky thanh cong. Chao mung ban den voi Boki.";
                    return RedirectToAction("Index", "Learning");
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Khong the dang ky luc nay. Vui long thu lai.");
                model.ClassOptions = LoadClassOptions();
                return View(model);
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            TempData["Message"] = "Ban da dang xuat.";
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SignIn(TAIKHOAN account, HOCSINH student, string normalizedRole)
        {
            Session[SessionKeys.AccountId] = account.MATAIKHOAN;
            Session[SessionKeys.Role] = normalizedRole;
            Session[SessionKeys.StudentName] = student != null && !string.IsNullOrWhiteSpace(student.HOTEN)
                ? student.HOTEN
                : (account.TENDANGNHAP ?? account.EMAIL);

            if (student != null)
            {
                Session[SessionKeys.StudentId] = student.MAHOCSINH;
                Session[SessionKeys.ClassId] = student.MALOP;
            }
            else
            {
                Session.Remove(SessionKeys.StudentId);
                Session.Remove(SessionKeys.ClassId);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var role = Session[SessionKeys.Role] as string;
            if (RoleHelper.IsAdmin(role))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Index", "Learning");
        }

        private List<SelectOptionViewModel> LoadClassOptions()
        {
            return _db.LOPs.AsNoTracking()
                .OrderBy(x => x.TENLOP)
                .Select(x => new SelectOptionViewModel
                {
                    Id = x.MALOP,
                    Name = x.TENLOP
                })
                .ToList();
        }
    }
}
