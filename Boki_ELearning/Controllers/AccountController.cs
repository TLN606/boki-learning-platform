using System;
using System.Data.Entity;
using System.Linq;
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

            var loginIdentity = model.Email.Trim().ToLowerInvariant();
            var account = _db.TAIKHOANs.FirstOrDefault(x =>
                ((x.EMAIL ?? string.Empty).ToLower() == loginIdentity) ||
                ((x.TENDANGNHAP ?? string.Empty).ToLower() == loginIdentity));

            if (account == null || !PasswordHelper.Verify(model.Password, account.MATKHAU))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            var student = _db.HOCSINHs.AsNoTracking().FirstOrDefault(x => x.MATAIKHOAN == account.MATAIKHOAN);
            SignIn(account, student);

            TempData["Message"] = "Đăng nhập thành công.";
            return RedirectToLocal(returnUrl);
        }

        public ActionResult Register()
        {
            if (Session[SessionKeys.AccountId] != null)
            {
                return RedirectToAction("Index", "Learning");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim().ToLowerInvariant();
            var existed = _db.TAIKHOANs.Any(x =>
                ((x.EMAIL ?? string.Empty).ToLower() == email) ||
                ((x.TENDANGNHAP ?? string.Empty).ToLower() == email));

            if (existed)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            int defaultClassId;
            try
            {
                defaultClassId = EnsureDefaultClass();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Không thể khởi tạo lớp mặc định. Vui lòng thử lại.");
                return View(model);
            }

            if (defaultClassId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Không thể khởi tạo lớp mặc định. Vui lòng thử lại.");
                return View(model);
            }

            TAIKHOAN account = null;
            try
            {
                account = new TAIKHOAN
                {
                    EMAIL = email,
                    TENDANGNHAP = email,
                    MATKHAU = PasswordHelper.Hash(model.Password),
                    VAITRO = "HOCSINH"
                };

                _db.TAIKHOANs.Add(account);
                _db.SaveChanges();

                var student = new HOCSINH
                {
                    HOTEN = model.FullName.Trim(),
                    MALOP = defaultClassId,
                    MATAIKHOAN = account.MATAIKHOAN
                };

                _db.HOCSINHs.Add(student);
                _db.SaveChanges();

                SignIn(account, student);

                TempData["Message"] = "Đăng ký thành công. Chào mừng bạn đến với Boki.";
                return RedirectToAction("Index", "Learning");
            }
            catch (Exception)
            {
                if (account != null && account.MATAIKHOAN > 0)
                {
                    var createdAccount = _db.TAIKHOANs.FirstOrDefault(x => x.MATAIKHOAN == account.MATAIKHOAN);
                    if (createdAccount != null)
                    {
                        _db.TAIKHOANs.Remove(createdAccount);
                        _db.SaveChanges();
                    }
                }

                ModelState.AddModelError(string.Empty, "Không thể đăng ký lúc này. Vui lòng thử lại.");
                return View(model);
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            TempData["Message"] = "Bạn đã đăng xuất.";
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

        private void SignIn(TAIKHOAN account, HOCSINH student)
        {
            Session[SessionKeys.AccountId] = account.MATAIKHOAN;
            Session[SessionKeys.Role] = account.VAITRO ?? "HOCSINH";
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

            return RedirectToAction("Index", "Learning");
        }

        private int EnsureDefaultClass()
        {
            var firstClass = _db.LOPs.OrderBy(x => x.MALOP).FirstOrDefault();
            if (firstClass != null)
            {
                return firstClass.MALOP;
            }

            var defaultClass = new LOP
            {
                TENLOP = "Lớp 2"
            };

            _db.LOPs.Add(defaultClass);
            _db.SaveChanges();

            return defaultClass.MALOP;
        }
    }
}
