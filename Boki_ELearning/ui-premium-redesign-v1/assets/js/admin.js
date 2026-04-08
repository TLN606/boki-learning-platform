(() => {
  const body = document.body;
  const page = body.dataset.adminPage;
  if (!page) {
    return;
  }

  async function loadJson(path, fallback) {
    try {
      const r = await fetch(path);
      if (!r.ok) {
        return fallback;
      }
      return await r.json();
    } catch {
      return fallback;
    }
  }

  const fallback = {
    students: [
      { id: "HS001", name: "Nguyen Minh Chau", class: "Lop 2A", completed: 14, avg: 8.7, status: "On track" },
      { id: "HS002", name: "Tran Gia Han", class: "Lop 2A", completed: 10, avg: 7.9, status: "On track" },
      { id: "HS003", name: "Le Quoc Bao", class: "Lop 2B", completed: 8, avg: 7.1, status: "Need support" },
      { id: "HS004", name: "Pham Ha My", class: "Lop 2B", completed: 15, avg: 9.2, status: "Excellent" }
    ],
    lessons: [
      { id: 201, subject: "Toan", title: "Cong tru trong pham vi 100", duration: "22 phut", progress: 100, status: "Hoan thanh", teacher: "Co Thu" },
      { id: 202, subject: "Toan", title: "So sanh phan so co ban", duration: "26 phut", progress: 55, status: "Dang hoc", teacher: "Co Thu" },
      { id: 203, subject: "Tieng Viet", title: "Doc hieu: Khu vuon mua he", duration: "18 phut", progress: 100, status: "Hoan thanh", teacher: "Thay Vinh" },
      { id: 204, subject: "Tieng Viet", title: "Luyen viet doan van ngan", duration: "28 phut", progress: 20, status: "Dang hoc", teacher: "Thay Vinh" }
    ],
    results: [
      { id: "R-24001", student: "Nguyen Minh Chau", lesson: "Cong tru trong pham vi 100", score: 9.5, correct: "19/20", time: "13:42", date: "2026-04-08" },
      { id: "R-24002", student: "Tran Gia Han", lesson: "So sanh phan so co ban", score: 8.0, correct: "16/20", time: "15:10", date: "2026-04-08" },
      { id: "R-24003", student: "Le Quoc Bao", lesson: "Doc hieu: Khu vuon mua he", score: 7.0, correct: "14/20", time: "18:00", date: "2026-04-07" }
    ],
    questions: [
      { id: 1, question: "9 + 14 bang bao nhieu?", answer: "23", lesson: "Cong tru trong pham vi 100", level: "De" },
      { id: 2, question: "15 - 8 = ?", answer: "7", lesson: "Cong tru trong pham vi 100", level: "Trung binh" },
      { id: 3, question: "So nao lon hon, 35 hay 53?", answer: "53", lesson: "So sanh phan so co ban", level: "De" }
    ]
  };

  const tableBody = document.querySelector("#adminTableBody");
  const search = document.querySelector("#adminSearch");
  const status = document.querySelector("#adminStatus");
  const pager = document.querySelector("#adminPager");
  const sortButtons = Array.from(document.querySelectorAll("th[data-sort]"));

  let items = [];
  let filtered = [];
  let pageIndex = 1;
  const pageSize = 6;
  let sortKey = "";
  let sortDir = 1;

  function badgeStatus(value) {
    const lower = String(value).toLowerCase();
    if (lower.includes("excellent") || lower.includes("hoan")) {
      return '<span class="badge success">' + value + "</span>";
    }
    if (lower.includes("need") || lower.includes("chua")) {
      return '<span class="badge warn">' + value + "</span>";
    }
    return '<span class="badge">' + value + "</span>";
  }

  function rowTemplate(entry) {
    if (page === "students") {
      return `<tr><td>${entry.id}</td><td>${entry.name}</td><td>${entry.class}</td><td>${entry.completed}</td><td>${entry.avg}</td><td>${badgeStatus(entry.status)}</td></tr>`;
    }
    if (page === "lessons") {
      return `<tr><td>${entry.id}</td><td>${entry.subject}</td><td>${entry.title}</td><td>${entry.teacher}</td><td>${entry.progress}%</td><td>${badgeStatus(entry.status)}</td></tr>`;
    }
    if (page === "questions") {
      return `<tr><td>${entry.id}</td><td>${entry.lesson}</td><td>${entry.question}</td><td>${entry.answer}</td><td>${badgeStatus(entry.level)}</td></tr>`;
    }
    return `<tr><td>${entry.id}</td><td>${entry.student}</td><td>${entry.lesson}</td><td>${entry.score}</td><td>${entry.correct}</td><td>${entry.date}</td></tr>`;
  }

  function applyFilters() {
    const q = (search?.value || "").trim().toLowerCase();
    const s = (status?.value || "").trim().toLowerCase();

    filtered = items.filter((item) => {
      const text = Object.values(item).join(" ").toLowerCase();
      const matchText = !q || text.includes(q);
      const matchStatus = !s || text.includes(s);
      return matchText && matchStatus;
    });

    if (sortKey) {
      filtered.sort((a, b) => {
        const va = a[sortKey];
        const vb = b[sortKey];
        if (typeof va === "number" && typeof vb === "number") {
          return (va - vb) * sortDir;
        }
        return String(va).localeCompare(String(vb)) * sortDir;
      });
    }

    const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
    pageIndex = Math.min(pageIndex, totalPages);
    const start = (pageIndex - 1) * pageSize;
    const pageRows = filtered.slice(start, start + pageSize);

    tableBody.innerHTML = pageRows.map(rowTemplate).join("") || '<tr><td colspan="6">Khong co du lieu phu hop.</td></tr>';

    pager.innerHTML = "";
    const prev = document.createElement("button");
    prev.className = "btn btn-secondary";
    prev.textContent = "Truoc";
    prev.disabled = pageIndex === 1;
    prev.onclick = () => {
      pageIndex -= 1;
      applyFilters();
    };

    const next = document.createElement("button");
    next.className = "btn btn-secondary";
    next.textContent = "Sau";
    next.disabled = pageIndex >= totalPages;
    next.onclick = () => {
      pageIndex += 1;
      applyFilters();
    };

    const info = document.createElement("span");
    info.textContent = `Trang ${pageIndex}/${totalPages}`;

    pager.append(prev, info, next);
  }

  sortButtons.forEach((th) => {
    th.style.cursor = "pointer";
    th.addEventListener("click", () => {
      const key = th.dataset.sort;
      if (sortKey === key) {
        sortDir *= -1;
      } else {
        sortKey = key;
        sortDir = 1;
      }
      applyFilters();
    });
  });

  search?.addEventListener("input", () => {
    pageIndex = 1;
    applyFilters();
  });

  status?.addEventListener("change", () => {
    pageIndex = 1;
    applyFilters();
  });

  (async () => {
    if (page === "students") {
      items = await loadJson("assets/data/students.json", fallback.students);
    } else if (page === "lessons") {
      items = await loadJson("assets/data/lessons.json", fallback.lessons);
    } else if (page === "questions") {
      const q = await loadJson("assets/data/questions.json", []);
      items = q.length ? q.map((x) => ({
        id: x.id,
        lesson: "Cong tru trong pham vi 100",
        question: x.question,
        answer: x.choices?.[x.answer] || "",
        level: x.id % 2 === 0 ? "Trung binh" : "De"
      })) : fallback.questions;
    } else {
      items = await loadJson("assets/data/results.json", fallback.results);
    }
    filtered = [...items];
    applyFilters();
  })();
})();
