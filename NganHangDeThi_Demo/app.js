const STORAGE_KEY = "question-bank-demo-v1";

const difficultyLabel = {
  de: "Dễ",
  trungbinh: "Trung bình",
  kho: "Khó",
};

const statusLabel = {
  active: "Đang dùng",
  draft: "Nháp",
};

let questions = [];

const els = {
  questionForm: document.getElementById("questionForm"),
  editingId: document.getElementById("editingId"),
  subjectInput: document.getElementById("subjectInput"),
  lessonInput: document.getElementById("lessonInput"),
  difficultyInput: document.getElementById("difficultyInput"),
  statusInput: document.getElementById("statusInput"),
  questionTextInput: document.getElementById("questionTextInput"),
  answerTextInputs: Array.from(document.querySelectorAll(".answer-text")),
  questionTableBody: document.getElementById("questionTableBody"),
  emptyState: document.getElementById("emptyState"),
  searchInput: document.getElementById("searchInput"),
  subjectFilter: document.getElementById("subjectFilter"),
  lessonFilter: document.getElementById("lessonFilter"),
  difficultyFilter: document.getElementById("difficultyFilter"),
  statsStrip: document.getElementById("statsStrip"),
  statTemplate: document.getElementById("statItemTemplate"),
  saveBtn: document.getElementById("saveBtn"),
  resetBtn: document.getElementById("resetBtn"),
  exportBtn: document.getElementById("exportBtn"),
  importBtn: document.getElementById("importBtn"),
  importFileInput: document.getElementById("importFileInput"),
  seedDataBtn: document.getElementById("seedDataBtn"),
  examCountInput: document.getElementById("examCountInput"),
  buildExamBtn: document.getElementById("buildExamBtn"),
  examResult: document.getElementById("examResult"),
};

init();

function init() {
  questions = loadQuestions();

  els.questionForm.addEventListener("submit", handleSaveQuestion);
  els.resetBtn.addEventListener("click", () => resetForm(false));
  els.searchInput.addEventListener("input", renderAll);
  els.subjectFilter.addEventListener("change", renderAll);
  els.lessonFilter.addEventListener("change", renderAll);
  els.difficultyFilter.addEventListener("change", renderAll);
  els.questionTableBody.addEventListener("click", handleTableAction);
  els.exportBtn.addEventListener("click", exportQuestions);
  els.importBtn.addEventListener("click", () => els.importFileInput.click());
  els.importFileInput.addEventListener("change", handleImportQuestions);
  els.seedDataBtn.addEventListener("click", seedData);
  els.buildExamBtn.addEventListener("click", buildExamFromFilters);

  renderAll();
}

function loadQuestions() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return [];
    }
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

function saveQuestions() {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(questions));
}

function handleSaveQuestion(event) {
  event.preventDefault();

  const subject = els.subjectInput.value.trim();
  const lesson = els.lessonInput.value.trim();
  const difficulty = els.difficultyInput.value;
  const status = els.statusInput.value;
  const questionText = els.questionTextInput.value.trim();
  const answerTexts = els.answerTextInputs.map((item) => item.value.trim());
  const correctInput = document.querySelector("input[name='correctAnswer']:checked");
  const correctIndex = correctInput ? Number(correctInput.value) : -1;

  if (!subject || !lesson || !difficulty || !status || !questionText) {
    alert("Vui lòng nhập đầy đủ thông tin câu hỏi.");
    return;
  }

  if (answerTexts.some((text) => !text)) {
    alert("Vui lòng nhập đầy đủ 4 đáp án.");
    return;
  }

  if (correctIndex < 0 || correctIndex > 3) {
    alert("Vui lòng chọn đáp án đúng.");
    return;
  }

  const now = new Date().toISOString();
  const answers = answerTexts.map((text, index) => ({
    id: index + 1,
    text,
    isCorrect: index === correctIndex,
  }));

  const editingId = els.editingId.value;

  if (editingId) {
    const index = questions.findIndex((q) => q.id === editingId);
    if (index >= 0) {
      questions[index] = {
        ...questions[index],
        subject,
        lesson,
        difficulty,
        status,
        questionText,
        answers,
        updatedAt: now,
      };
    }
  } else {
    questions.unshift({
      id: createId(),
      subject,
      lesson,
      difficulty,
      status,
      questionText,
      answers,
      createdAt: now,
      updatedAt: now,
    });
  }

  saveQuestions();
  resetForm(true);
  renderAll();
}

function handleTableAction(event) {
  const target = event.target.closest("button[data-action]");
  if (!target) {
    return;
  }

  const { action, id } = target.dataset;
  if (!id) {
    return;
  }

  const index = questions.findIndex((q) => q.id === id);
  if (index < 0) {
    return;
  }

  if (action === "edit") {
    loadFormForEdit(questions[index]);
    return;
  }

  if (action === "delete") {
    const ok = confirm("Bạn có chắc muốn xóa câu hỏi này?");
    if (!ok) {
      return;
    }
    questions.splice(index, 1);
    saveQuestions();
    renderAll();
    return;
  }

  if (action === "toggle") {
    questions[index].status = questions[index].status === "active" ? "draft" : "active";
    questions[index].updatedAt = new Date().toISOString();
    saveQuestions();
    renderAll();
  }
}

function loadFormForEdit(question) {
  els.editingId.value = question.id;
  els.subjectInput.value = question.subject;
  els.lessonInput.value = question.lesson;
  els.difficultyInput.value = question.difficulty;
  els.statusInput.value = question.status;
  els.questionTextInput.value = question.questionText;

  question.answers.forEach((answer, idx) => {
    if (els.answerTextInputs[idx]) {
      els.answerTextInputs[idx].value = answer.text;
    }
    const radio = document.querySelector(`input[name='correctAnswer'][value='${idx}']`);
    if (radio) {
      radio.checked = !!answer.isCorrect;
    }
  });

  els.saveBtn.textContent = "Cập nhật câu hỏi";
  els.subjectInput.focus();
}

function resetForm(keepVisualState) {
  if (!keepVisualState) {
    els.questionForm.reset();
  }

  els.editingId.value = "";
  els.statusInput.value = "active";
  const firstRadio = document.querySelector("input[name='correctAnswer'][value='0']");
  if (firstRadio) {
    firstRadio.checked = true;
  }
  els.saveBtn.textContent = "Lưu câu hỏi";
}

function renderAll() {
  populateFilters();
  renderTable();
  renderStats();
}

function getFilteredQuestions() {
  const keyword = els.searchInput.value.trim().toLowerCase();
  const subject = els.subjectFilter.value;
  const lesson = els.lessonFilter.value;
  const difficulty = els.difficultyFilter.value;

  return questions.filter((q) => {
    const matchesKeyword =
      !keyword ||
      q.questionText.toLowerCase().includes(keyword) ||
      q.subject.toLowerCase().includes(keyword) ||
      q.lesson.toLowerCase().includes(keyword);
    const matchesSubject = !subject || q.subject === subject;
    const matchesLesson = !lesson || q.lesson === lesson;
    const matchesDifficulty = !difficulty || q.difficulty === difficulty;

    return matchesKeyword && matchesSubject && matchesLesson && matchesDifficulty;
  });
}

function populateFilters() {
  const selectedSubject = els.subjectFilter.value;
  const selectedLesson = els.lessonFilter.value;

  const subjects = [...new Set(questions.map((q) => q.subject))].sort((a, b) => a.localeCompare(b, "vi"));
  const lessons = [...new Set(questions.map((q) => q.lesson))].sort((a, b) => a.localeCompare(b, "vi"));

  replaceSelectOptions(els.subjectFilter, "Tất cả môn học", subjects, selectedSubject);
  replaceSelectOptions(els.lessonFilter, "Tất cả bài học", lessons, selectedLesson);
}

function replaceSelectOptions(select, firstLabel, values, selectedValue) {
  select.innerHTML = "";

  const firstOption = document.createElement("option");
  firstOption.value = "";
  firstOption.textContent = firstLabel;
  select.appendChild(firstOption);

  values.forEach((value) => {
    const option = document.createElement("option");
    option.value = value;
    option.textContent = value;
    select.appendChild(option);
  });

  if (values.includes(selectedValue)) {
    select.value = selectedValue;
  }
}

function renderTable() {
  const filtered = getFilteredQuestions();

  els.questionTableBody.innerHTML = filtered
    .map((q, idx) => {
      const difficultyText = difficultyLabel[q.difficulty] || q.difficulty;
      const statusText = statusLabel[q.status] || q.status;

      return `
        <tr>
          <td>${idx + 1}</td>
          <td>${escapeHtml(q.subject)}</td>
          <td>${escapeHtml(q.lesson)}</td>
          <td>
            <div class="question-snippet">${escapeHtml(q.questionText)}</div>
          </td>
          <td><span class="badge badge-${q.difficulty}">${difficultyText}</span></td>
          <td><span class="badge badge-${q.status}">${statusText}</span></td>
          <td>
            <div class="row-actions">
              <button class="btn-mini" type="button" data-action="edit" data-id="${q.id}">Sửa</button>
              <button class="btn-mini" type="button" data-action="toggle" data-id="${q.id}">Đổi trạng thái</button>
              <button class="btn-mini btn-danger" type="button" data-action="delete" data-id="${q.id}">Xóa</button>
            </div>
          </td>
        </tr>
      `;
    })
    .join("");

  els.emptyState.style.display = filtered.length ? "none" : "block";
}

function renderStats() {
  const total = questions.length;
  const active = questions.filter((q) => q.status === "active").length;
  const draft = questions.filter((q) => q.status === "draft").length;
  const hard = questions.filter((q) => q.difficulty === "kho").length;

  const stats = [
    { label: "Tổng câu hỏi", value: total },
    { label: "Đang dùng", value: active },
    { label: "Nháp", value: draft },
    { label: "Mức khó", value: hard },
  ];

  els.statsStrip.innerHTML = "";

  stats.forEach((item) => {
    const clone = els.statTemplate.content.cloneNode(true);
    clone.querySelector(".stat-label").textContent = item.label;
    clone.querySelector(".stat-value").textContent = String(item.value);
    els.statsStrip.appendChild(clone);
  });
}

function exportQuestions() {
  if (!questions.length) {
    alert("Chưa có câu hỏi để xuất.");
    return;
  }

  const content = JSON.stringify(questions, null, 2);
  const blob = new Blob([content], { type: "application/json;charset=utf-8" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");

  const stamp = new Date().toISOString().slice(0, 10);
  link.href = url;
  link.download = `ngan-hang-de-thi-${stamp}.json`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

async function handleImportQuestions(event) {
  const file = event.target.files?.[0];
  if (!file) {
    return;
  }

  try {
    const content = await file.text();
    const imported = JSON.parse(content);

    if (!Array.isArray(imported)) {
      alert("File JSON không đúng định dạng mảng câu hỏi.");
      return;
    }

    const normalized = imported
      .map(normalizeImportedQuestion)
      .filter((item) => item !== null);

    if (!normalized.length) {
      alert("Không tìm thấy câu hỏi hợp lệ trong file.");
      return;
    }

    questions = [...normalized, ...questions];
    saveQuestions();
    renderAll();
    alert(`Đã nhập ${normalized.length} câu hỏi.`);
  } catch {
    alert("Không thể đọc file JSON. Vui lòng kiểm tra lại.");
  } finally {
    event.target.value = "";
  }
}

function normalizeImportedQuestion(raw) {
  if (!raw || typeof raw !== "object") {
    return null;
  }

  const subject = String(raw.subject || "").trim();
  const lesson = String(raw.lesson || "").trim();
  const difficulty = ["de", "trungbinh", "kho"].includes(raw.difficulty) ? raw.difficulty : "trungbinh";
  const status = ["active", "draft"].includes(raw.status) ? raw.status : "draft";
  const questionText = String(raw.questionText || "").trim();

  if (!subject || !lesson || !questionText || !Array.isArray(raw.answers) || raw.answers.length < 2) {
    return null;
  }

  const answers = raw.answers.slice(0, 4).map((a, idx) => ({
    id: idx + 1,
    text: String(a.text || "").trim(),
    isCorrect: !!a.isCorrect,
  }));

  if (answers.some((a) => !a.text)) {
    return null;
  }

  if (!answers.some((a) => a.isCorrect)) {
    answers[0].isCorrect = true;
  }

  const now = new Date().toISOString();

  return {
    id: createId(),
    subject,
    lesson,
    difficulty,
    status,
    questionText,
    answers,
    createdAt: raw.createdAt || now,
    updatedAt: raw.updatedAt || now,
  };
}

function seedData() {
  if (questions.length) {
    const ok = confirm("Kho câu hỏi đang có dữ liệu. Bạn muốn thêm dữ liệu mẫu vào danh sách hiện tại?");
    if (!ok) {
      return;
    }
  }

  const now = new Date().toISOString();
  const seeds = [
    {
      id: createId(),
      subject: "Toán",
      lesson: "Phân số",
      difficulty: "de",
      status: "active",
      questionText: "Phân số nào lớn hơn trong hai phân số 3/5 và 1/2?",
      answers: [
        { id: 1, text: "3/5", isCorrect: true },
        { id: 2, text: "1/2", isCorrect: false },
        { id: 3, text: "Hai phân số bằng nhau", isCorrect: false },
        { id: 4, text: "Không xác định", isCorrect: false },
      ],
      createdAt: now,
      updatedAt: now,
    },
    {
      id: createId(),
      subject: "Tiếng Việt",
      lesson: "Từ đồng nghĩa",
      difficulty: "trungbinh",
      status: "active",
      questionText: "Từ nào đồng nghĩa với từ 'siêng năng'?",
      answers: [
        { id: 1, text: "Lười biếng", isCorrect: false },
        { id: 2, text: "Chăm chỉ", isCorrect: true },
        { id: 3, text: "Mệt mỏi", isCorrect: false },
        { id: 4, text: "Nhanh nhẹn", isCorrect: false },
      ],
      createdAt: now,
      updatedAt: now,
    },
    {
      id: createId(),
      subject: "Khoa học",
      lesson: "Hệ mặt trời",
      difficulty: "kho",
      status: "draft",
      questionText: "Hành tinh nào được gọi là 'hành tinh đỏ'?",
      answers: [
        { id: 1, text: "Sao Mộc", isCorrect: false },
        { id: 2, text: "Sao Kim", isCorrect: false },
        { id: 3, text: "Sao Hỏa", isCorrect: true },
        { id: 4, text: "Sao Thổ", isCorrect: false },
      ],
      createdAt: now,
      updatedAt: now,
    },
  ];

  questions = [...seeds, ...questions];
  saveQuestions();
  renderAll();
}

function buildExamFromFilters() {
  const count = Number(els.examCountInput.value);
  if (!Number.isInteger(count) || count < 1) {
    alert("Số lượng câu hỏi cần lớn hơn 0.");
    return;
  }

  const pool = getFilteredQuestions().filter((q) => q.status === "active");
  if (!pool.length) {
    alert("Không có câu hỏi phù hợp để tạo đề.");
    return;
  }

  const shuffled = [...pool].sort(() => Math.random() - 0.5);
  const selected = shuffled.slice(0, Math.min(count, shuffled.length));

  els.examResult.innerHTML = selected
    .map((q) => {
      const correct = q.answers.find((a) => a.isCorrect);
      return `<li><strong>${escapeHtml(q.subject)} · ${escapeHtml(q.lesson)}</strong><br>${escapeHtml(q.questionText)}<br><em>Đáp án: ${escapeHtml(correct?.text || "")}</em></li>`;
    })
    .join("");
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

function createId() {
  return `q_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 8)}`;
}
