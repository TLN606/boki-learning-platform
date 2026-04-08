(() => {
  const nav = document.querySelector("#siteNav");
  const menuBtn = document.querySelector(".js-menu-btn");
  const toastStack = document.querySelector("#toastStack");

  if (menuBtn && nav) {
    menuBtn.addEventListener("click", () => {
      nav.classList.toggle("open");
    });
  }

  document.querySelectorAll("[data-progress]").forEach((el) => {
    const val = Number(el.dataset.progress || 0);
    el.style.width = `${Math.max(0, Math.min(100, val))}%`;
  });

  document.querySelectorAll("[data-tab-target]").forEach((tabBtn) => {
    tabBtn.addEventListener("click", () => {
      const target = tabBtn.dataset.tabTarget;
      const wrap = tabBtn.closest("[data-tabs]");
      if (!wrap) {
        return;
      }
      wrap.querySelectorAll("[data-tab-target]").forEach((btn) => btn.classList.remove("active"));
      wrap.querySelectorAll("[data-tab]").forEach((pane) => pane.hidden = pane.dataset.tab !== target);
      tabBtn.classList.add("active");
    });
  });

  function showToast(message) {
    if (!toastStack) {
      return;
    }
    const node = document.createElement("div");
    node.className = "toast";
    node.textContent = message;
    toastStack.appendChild(node);
    setTimeout(() => {
      node.remove();
    }, 2300);
  }

  window.BokiUI = {
    toast: showToast
  };

  document.querySelectorAll("[data-demo-submit]").forEach((btn) => {
    btn.addEventListener("click", (event) => {
      event.preventDefault();
      const msg = btn.dataset.demoSubmit || "Đã lưu dữ liệu.";
      showToast(msg);
      const href = btn.getAttribute("href");
      if (href) {
        setTimeout(() => {
          window.location.href = href;
        }, 420);
      }
    });
  });

  document.querySelectorAll("[data-modal-open]").forEach((btn) => {
    btn.addEventListener("click", () => {
      const id = btn.dataset.modalOpen;
      const modal = document.getElementById(id);
      if (modal) {
        modal.hidden = false;
      }
    });
  });

  document.querySelectorAll("[data-modal-close]").forEach((btn) => {
    btn.addEventListener("click", () => {
      const modal = btn.closest("[data-modal]");
      if (modal) {
        modal.hidden = true;
      }
    });
  });

  document.querySelectorAll("[data-link-disabled]").forEach((el) => {
    el.addEventListener("click", (event) => {
      event.preventDefault();
      showToast("Prototype chưa mở logic backend cho thao tác này.");
    });
  });
})();




