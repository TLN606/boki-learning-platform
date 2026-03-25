import os
import re

BASE = r'd:\01_Workspaces\Coding\Websites\boki-frontend'

# 1. Update style.css for smaller quiz options
css_path = os.path.join(BASE, 'assets', 'css', 'style.css')
with open(css_path, 'r', encoding='utf-8') as f:
    css = f.read()

# Make quiz answer options smaller
css = css.replace(
    'padding: 1.5rem 2rem;',
    'padding: 1rem 1.25rem;'
)
with open(css_path, 'w', encoding='utf-8') as f:
    f.write(css)
print("[+] Updated style.css quiz option sizes")

# Files to update
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

DROPDOWN_MENU = """                <!-- User Menu -->
                <div class="relative group">
                    <button class="flex items-center gap-2 focus:outline-none">
                        <img src="https://api.dicebear.com/7.x/avataaars/svg?seed=Felix&backgroundColor=b6e3f4" alt="Avatar" class="w-10 h-10 rounded-full border-2 border-white shadow-sm hover:scale-105 transition-transform">
                        <span class="font-bold text-slate-700 dark:text-slate-200 hidden sm:inline-block">Minh Hoàng</span>
                        <svg class="w-4 h-4 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                    </button>
                    <!-- Dropdown -->
                    <div class="absolute right-0 mt-2 w-56 bg-white dark:bg-slate-800 rounded-2xl shadow-xl border border-slate-100 dark:border-slate-700 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 z-[100] transform origin-top-right scale-95 group-hover:scale-100">
                        <div class="p-2 space-y-1">
                            <a href="#" class="block px-4 py-2 text-sm font-bold text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700 rounded-xl transition-colors">👤 Thông tin tài khoản</a>
                            <a href="#" class="block px-4 py-2 text-sm font-bold text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700 rounded-xl transition-colors">⚙️ Chỉnh sửa</a>
                            <div class="h-px bg-slate-100 dark:bg-slate-700 my-1"></div>
                            <a href="login.html" class="block px-4 py-2 text-sm font-bold text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-xl transition-colors">🚪 Đăng xuất</a>
                        </div>
                    </div>
                </div>"""

HEADER_USER_SECTION_REGEX = r'<div class="flex items-center gap-2">\s*<img src="https://api.dicebear.com[^"]+"[^>]+>\s*(?:<span[^>]+>Minh Hoàng</span>\s*)?</div>\s*(?:<a href="login.html" class="clay-btn-danger[^"]+">Thoát</a>)?'

for fname in files:
    path = os.path.join(BASE, fname)
    if not os.path.exists(path):
        continue

    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. Flex layout fix for footer
    # Replace body class to include flex and flex-col, ensuring min-h-screen is there
    if '<body class="' in content and 'flex-col' not in content:
        content = re.sub(r'<body class="([^"]+)"', r'<body class="\1 flex flex-col min-h-screen"', content)
    
    # Replace main class to include flex-grow
    if '<main class="' in content and 'flex-grow' not in content:
        content = re.sub(r'<main class="([^"]+)"', r'<main class="\1 flex-grow"', content)
    elif '<main>' in content:
        content = content.replace('<main>', '<main class="flex-grow">')
        
    # Remove pb-20 from body if present, because footer handles it now
    content = content.replace(' pb-20', '')

    # 2. Add Dropdown menu (replace the avatar + "Thoát" button)
    if 'Minh Hoàng' in content or 'dicebear' in content:
        content = re.sub(HEADER_USER_SECTION_REGEX, DROPDOWN_MENU, content)

    # 3. Fix button text on lesson cards (make them flex-col or stack on small screens and use text-center)
    # We will replace 'py-2 flex-1 text-sm' with 'py-2 px-1 flex-1 text-xs sm:text-sm text-center whitespace-nowrap overflow-hidden text-ellipsis flex items-center justify-center'
    # Actually just simple text sizing and flex centering
    content = content.replace('py-2 flex-1 text-sm', 'py-2 px-2 flex-1 text-xs md:text-sm text-center flex items-center justify-center')
    
    # 4. Make quiz texts smaller
    if fname == 'quiz.html':
        content = content.replace('text-2xl font-bold', 'text-lg md:text-xl font-bold')
        content = content.replace('text-3xl md:text-5xl font-black', 'text-2xl md:text-4xl font-black')
        content = content.replace('w-12 h-12', 'w-10 h-10') # Smaller option letter circle

    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)
    
    print(f"[+] Fixed layout & dropdown in {fname}")

print("Done phase 2 refinement")
