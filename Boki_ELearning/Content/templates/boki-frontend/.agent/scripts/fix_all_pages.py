"""
Fix all Boki HTML pages:
1. Inject tailwind.config with darkMode:'class' (the ROOT CAUSE of dark mode not working)
2. Fix footer placement (move outside <main>, place before </body>)
3. Add answer-option CSS class to style.css
"""
import os
import re

BASE = r'd:\01_Workspaces\Coding\Websites\boki-frontend'

TAILWIND_CONFIG = """    <script>
        tailwind.config = {
            darkMode: 'class',
            theme: {
                extend: {
                    fontFamily: { sans: ['Nunito', 'sans-serif'] },
                    colors: {
                        brand: { 50: '#eff6ff', 100: '#dbeafe', 500: '#3b82f6', 600: '#2563eb', 900: '#1e3a8a' }
                    }
                }
            }
        }
    </script>"""

FOOTER_STUDENT = """
    <!-- Footer -->
    <footer class="bg-white/80 dark:bg-slate-800/80 backdrop-blur-md pt-8 pb-6 border-t border-slate-200 dark:border-slate-700 mt-12 w-full">
        <div class="container mx-auto px-6 flex flex-col items-center text-center">
            <span class="text-xl font-black text-slate-800 dark:text-white tracking-tight mb-2">Boki</span>
            <p class="text-slate-500 dark:text-slate-400 font-medium mb-4 max-w-sm text-sm">Nền tảng học tập vui nhộn, giúp bé phát triển tư duy xuất chúng mỗi ngày.</p>
            <div class="w-full border-t border-slate-100 dark:border-slate-700 pt-4 text-slate-400 dark:text-slate-500 font-bold text-xs mt-2">
                <p>&copy; 2026 Boki Education. Không ngừng sáng tạo.</p>
            </div>
        </div>
    </footer>"""

TOGGLE_BTN = """    <!-- Floating Dark Mode Toggle -->
    <button id="theme-toggle" class="fixed bottom-6 right-6 z-50 p-3 rounded-full bg-white dark:bg-slate-800 shadow-lg border border-slate-200 dark:border-slate-700 hover:scale-110 active:scale-95 transition-all cursor-pointer text-slate-600 dark:text-slate-300">
        <svg class="w-5 h-5 hidden dark:block" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"></path></svg>
        <svg class="w-5 h-5 block dark:hidden" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"></path></svg>
    </button>"""

# Files to process (excluding landing - it already works, and login/register - already fixed)
files = [
    'student-dashboard.html',
    'lesson-list.html',
    'lesson-detail.html',
    'quiz.html',
    'result.html',
    'admin-dashboard.html',
    'admin-students.html',
    'admin-lessons.html',
    'admin-questions.html',
    'admin-results.html',
]

for fname in files:
    path = os.path.join(BASE, fname)
    if not os.path.exists(path):
        print(f"SKIP: {fname} not found")
        continue

    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    original = content

    # ==========================================
    # FIX 1: Inject tailwind.config with darkMode:'class'
    # This is the ROOT CAUSE - without this, dark: classes are completely ignored
    # ==========================================
    if "tailwind.config" not in content:
        # Insert right after the tailwindcss script tag
        content = content.replace(
            '<script src="https://cdn.tailwindcss.com"></script>',
            '<script src="https://cdn.tailwindcss.com"></script>\n' + TAILWIND_CONFIG
        )
        print(f"  [+] {fname}: Injected tailwind.config darkMode:'class'")
    
    # ==========================================
    # FIX 2: Remove all existing broken footer + toggle button blocks
    # They were injected inside <main> which breaks layout
    # ==========================================
    # Remove old footer blocks (inside main or anywhere)
    content = re.sub(r'\s*<!-- Footer -->\s*<footer[^>]*>.*?</footer>', '', content, flags=re.DOTALL)
    # Remove old floating toggle buttons
    content = re.sub(r'\s*<!-- Floating Dark Mode Toggle -->\s*<button[^>]*id="theme-toggle"[^>]*>.*?</button>', '', content, flags=re.DOTALL)
    
    # ==========================================
    # FIX 3: Fix double dark classes (from previous script)
    # ==========================================
    content = content.replace('dark:text-slate-100 dark:text-slate-100', 'dark:text-slate-100')
    content = content.replace('dark:bg-slate-900 dark:bg-slate-900', 'dark:bg-slate-900')
    
    # ==========================================
    # FIX 4: Re-inject footer + toggle in the CORRECT position (before </body>)
    # ==========================================
    is_admin = fname.startswith('admin-')
    
    # For admin pages, don't add footer (they have sidebar layout)
    if is_admin:
        inject = f"\n{TOGGLE_BTN}\n"
    else:
        inject = f"\n{FOOTER_STUDENT}\n\n{TOGGLE_BTN}\n"
    
    content = content.replace('</body>', inject + '\n</body>')

    if content != original:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"  [✓] {fname}: Fixed successfully")
    else:
        print(f"  [-] {fname}: No changes needed")

# ==========================================
# FIX 5: Add answer-option + admin styles to style.css
# ==========================================
css_path = os.path.join(BASE, 'assets', 'css', 'style.css')
with open(css_path, 'r', encoding='utf-8') as f:
    css = f.read()

extra_css = """

/* Quiz answer options */
.answer-option {
    background: #fff; border-radius: 1.5rem; padding: 1.5rem 2rem; width: 100%;
    box-shadow: 6px 6px 12px rgba(166, 180, 200, 0.3), -6px -6px 12px rgba(255, 255, 255, 0.8), inset 2px 2px 4px rgba(255, 255, 255, 0.5), inset -2px -2px 4px rgba(166, 180, 200, 0.1);
    border: 3px solid transparent; cursor: pointer; transition: all 0.2s ease; text-align: left;
}
.dark .answer-option {
    background: #1e293b;
    box-shadow: 6px 6px 12px rgba(0,0,0,0.4), -6px -6px 12px rgba(255,255,255,0.03), inset 2px 2px 4px rgba(255,255,255,0.08), inset -2px -2px 4px rgba(0,0,0,0.4);
}
.answer-option:hover { border-color: #93c5fd; transform: translateY(-2px); }
.dark .answer-option:hover { border-color: #3b82f6; }
.answer-option.selected { border-color: #3B82F6; background: #EFF6FF; }
.dark .answer-option.selected { border-color: #60a5fa; background: #1e3a5f; }

/* Admin card */
.admin-card {
    background: #fff; border-radius: 1.5rem;
    box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05), 0 2px 4px -1px rgba(0,0,0,0.03);
    border: 1px solid #F1F5F9; transition: all 0.3s;
}
.dark .admin-card {
    background: #1e293b; border-color: rgba(255,255,255,0.05);
    box-shadow: 0 4px 6px -1px rgba(0,0,0,0.3), 0 2px 4px -1px rgba(0,0,0,0.2);
}

/* Admin sidebar */
.sidebar-link {
    padding: 0.75rem 1rem; border-radius: 1rem; display: flex; align-items: center; gap: 0.75rem;
    font-weight: 700; color: #64748B; transition: all 0.2s;
}
.dark .sidebar-link { color: #94a3b8; }
.sidebar-link:hover { background-color: #F1F5F9; color: #3B82F6; }
.dark .sidebar-link:hover { background-color: #334155; color: #60a5fa; }
.sidebar-link.active { background-color: #E0F2FE; color: #2563EB; box-shadow: inset 2px 2px 4px rgba(255,255,255,0.5), inset -2px -2px 4px rgba(186,230,253,0.5); }
.dark .sidebar-link.active { background-color: #1e3a5f; color: #60a5fa; }

.stat-icon-wrapper {
    width: 3rem; height: 3rem; border-radius: 1rem; display: flex; align-items: center; justify-content: center; font-size: 1.5rem;
}

.grade-tab.active { background-color: #3B82F6; color: white; box-shadow: inset 2px 2px 4px rgba(29,78,216,0.5); }

/* Danger button */
.clay-btn-danger {
    background: #EF4444; color: white; border-radius: 9999px; display: inline-block; text-align: center;
    box-shadow: 6px 6px 12px rgba(239,68,68,0.4), -4px -4px 10px rgba(255,255,255,0.9), inset 2px 2px 6px rgba(255,255,255,0.3), inset -2px -2px 6px rgba(185,28,28,0.5);
    transition: all 0.2s ease; font-weight: 800; cursor: pointer;
}
.dark .clay-btn-danger {
    box-shadow: 6px 6px 12px rgba(0,0,0,0.5), -4px -4px 10px rgba(255,255,255,0.05), inset 2px 2px 6px rgba(255,255,255,0.2), inset -2px -2px 6px rgba(127,29,29,0.6);
}
.clay-btn-danger:active { box-shadow: inset 4px 4px 8px rgba(185,28,28,0.6), inset -4px -4px 8px rgba(255,255,255,0.2); transform: scale(0.96); }
"""

if '.answer-option' not in css:
    with open(css_path, 'a', encoding='utf-8') as f:
        f.write(extra_css)
    print("\n[✓] style.css: Added answer-option, admin-card, sidebar-link, grade-tab, clay-btn-danger styles")

# ==========================================
# FIX 6: Fix landing.html footer links for mobile responsive
# ==========================================
landing_path = os.path.join(BASE, 'landing.html')
with open(landing_path, 'r', encoding='utf-8') as f:
    landing = f.read()

# Make footer links column on mobile
landing = landing.replace(
    '<div class="flex gap-6 mb-10 font-bold text-slate-500 dark:text-slate-400">',
    '<div class="flex flex-col sm:flex-row gap-4 sm:gap-6 mb-10 font-bold text-slate-500 dark:text-slate-400">'
)
with open(landing_path, 'w', encoding='utf-8') as f:
    f.write(landing)
print("\n[✓] landing.html: Fixed footer links to stack on mobile")

print("\n=== ALL FIXES COMPLETE ===")
