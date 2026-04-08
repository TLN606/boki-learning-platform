(() => {
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

  const fallbackQuestions = [
    { id: 1, question: "9 + 14 bằng bao nhiêu?", choices: ["21", "22", "23", "24"], answer: 2 },
    { id: 2, question: "Số nào lớn hơn?", choices: ["35", "53", "45", "43"], answer: 1 },
    { id: 3, question: "15 - 8 = ?", choices: ["5", "6", "7", "8"], answer: 2 },
    { id: 4, question: "3/4 và 2/4, phân số nào lớn hơn?", choices: ["3/4", "2/4", "Bằng nhau", "Không so sánh được"], answer: 0 },
    { id: 5, question: "Số tiếp theo của dãy 4, 8, 12, ... là?", choices: ["14", "15", "16", "18"], answer: 2 },
    { id: 6, question: "Hình nào có 3 cạnh bằng nhau?", choices: ["Tam giác đều", "Tam giác vuông", "Hình vuông", "Hình tròn"], answer: 0 }
  ];

  const app = document.querySelector("#quizApp");
  if (!app) {
    return;
  }

  const questionLabel = app.querySelector("#quizQuestion");
  const optionList = app.querySelector("#quizOptions");
  const currentText = app.querySelector("#quizCurrent");
  const timerText = app.querySelector("#quizTimer");
  const mapWrap = app.querySelector("#quizMap");
  const btnPrev = app.querySelector("#quizPrev");
  const btnNext = app.querySelector("#quizNext");
  const btnSubmit = app.querySelector("#quizSubmit");

  let questions = [];
  let current = 0;
  let remain = 15 * 60;
  let answers = [];

  function formatTime(sec) {
    const m = Math.floor(sec / 60);
    const s = sec % 60;
    return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
  }

  function renderMap() {
    mapWrap.innerHTML = "";
    questions.forEach((_, idx) => {
      const b = document.createElement("button");
      b.textContent = idx + 1;
      if (answers[idx] !== null && answers[idx] !== undefined) {
        b.classList.add("done");
      }
      if (idx === current) {
        b.style.borderColor = "#ab7a38";
      }
      b.addEventListener("click", () => {
        current = idx;
        renderQuestion();
      });
      mapWrap.appendChild(b);
    });
  }

  function renderQuestion() {
    const q = questions[current];
    if (!q) {
      return;
    }
    currentText.textContent = `${current + 1}/${questions.length}`;
    questionLabel.textContent = q.question;
    optionList.innerHTML = "";

    q.choices.forEach((choice, idx) => {
      const li = document.createElement("button");
      li.type = "button";
      li.className = "option";
      li.textContent = `${String.fromCharCode(65 + idx)}. ${choice}`;
      if (answers[current] === idx) {
        li.classList.add("active");
      }
      li.addEventListener("click", () => {
        answers[current] = idx;
        renderQuestion();
      });
      optionList.appendChild(li);
    });

    btnPrev.disabled = current === 0;
    btnNext.disabled = current === questions.length - 1;
    renderMap();
  }

  function scoreQuiz() {
    let correct = 0;
    questions.forEach((q, idx) => {
      if (answers[idx] === q.answer) {
        correct += 1;
      }
    });
    const total = questions.length;
    const score = ((correct / total) * 10).toFixed(1);
    localStorage.setItem("bokiLastQuiz", JSON.stringify({
      correct,
      total,
      score,
      timeLeft: remain,
      at: new Date().toISOString()
    }));
  }

  function submitQuiz() {
    scoreQuiz();
    if (window.BokiUI) {
      window.BokiUI.toast("Đã nộp bài. Đang chuyển đến trang kết quả...");
    }
    setTimeout(() => {
      window.location.href = "student-result.html";
    }, 550);
  }

  btnPrev.addEventListener("click", () => {
    if (current > 0) {
      current -= 1;
      renderQuestion();
    }
  });

  btnNext.addEventListener("click", () => {
    if (current < questions.length - 1) {
      current += 1;
      renderQuestion();
    }
  });

  btnSubmit.addEventListener("click", () => submitQuiz());

  setInterval(() => {
    if (remain <= 0) {
      submitQuiz();
      return;
    }
    remain -= 1;
    timerText.textContent = formatTime(remain);
  }, 1000);

  (async () => {
    questions = await loadJson("assets/data/questions.json", fallbackQuestions);
    answers = new Array(questions.length).fill(null);
    timerText.textContent = formatTime(remain);
    renderQuestion();
  })();
})();




