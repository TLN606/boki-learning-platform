import os
import re

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
    'admin-results.html'
]

TOGGLE_BTN = """    <!-- Floating Dark Mode Toggle -->
    <button id="theme-toggle" class="fixed bottom-6 right-6 z-50 p-4 rounded-full bg-white dark:bg-slate-800 shadow-[0_4px_24px_rgba(0,0,0,0.1)] dark:shadow-[0_4px_24px_rgba(0,0,0,0.5)] border border-slate-200 dark:border-slate-700 hover:scale-105 transition-transform cursor-pointer text-slate-600 dark:text-slate-300 flex items-center justify-center">
        <!-- Sun icon -->
        <svg id="theme-toggle-light-icon" class="w-6 h-6 hidden dark:block" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"></path></svg>
        <!-- Moon icon -->
        <svg id="theme-toggle-dark-icon" class="w-6 h-6 block dark:hidden" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"></path></svg>
    </button>
"""

FOOTER_HTML = """    <!-- Footer -->
    <footer class="bg-white/80 dark:bg-slate-900/80 backdrop-blur-md pt-8 pb-6 border-t border-slate-200 dark:border-slate-800 mt-12 w-full rounded-t-3xl">
        <div class="container mx-auto px-6 flex flex-col items-center text-center">
            <span class="text-xl font-black text-slate-800 dark:text-white tracking-tight mb-2">Boki</span>
            <p class="text-slate-500 dark:text-slate-400 font-medium mb-4 max-w-sm text-sm">Nền tảng học tập vui nhộn, giúp bé phát triển tư duy xuất chúng mỗi ngày.</p>
            <div class="w-full border-t border-slate-100 dark:border-slate-800 pt-4 text-slate-400 dark:text-slate-500 font-bold text-xs mt-2">
                <p>&copy; 2026 Boki Education. Không ngừng sáng tạo.</p>
            </div>
        </div>
    </footer>"""

for f in files:
    path = os.path.join('d:\\01_Workspaces\\Coding\\Websites\\boki-frontend', f)
    if not os.path.exists(path):
        print(f"Skipping {f}, not found.")
        continue
        
    with open(path, 'r', encoding='utf-8') as file:
        content = file.read()
    
    # 1. HTML lang
    content = content.replace('<html lang="vi">', '<html lang="vi" class="light">')
    
    # 2. Extract specific styles and replace with links
    # Replace anything between <style> and </style>
    if '<style>' in content and '</style>' in content:
        content = re.sub(r'<style>.*?</style>', '<link rel="stylesheet" href="assets/css/style.css">\n    <script src="assets/js/theme.js"></script>', content, flags=re.DOTALL)
    
    # 3. Add responsive body & main classes (+ dark mode colors)
    # Most general fix for body
    if 'class="' in content.split('<body')[1].split('>')[0]:
        content = re.sub(r'<body class="([^"]*)">', r'<body class="\1 bg-slate-50 dark:bg-slate-900 text-slate-800 dark:text-slate-100 transition-colors duration-300">', content)
    else:
        content = content.replace('<body>', '<body class="bg-slate-50 dark:bg-slate-900 text-slate-800 dark:text-slate-100 transition-colors duration-300">')
        
    # Replace white backgrounds with dark variants across generic divs/cards
    content = content.replace('bg-white', 'bg-white dark:bg-slate-800')
    content = content.replace('bg-slate-50', 'bg-slate-50 dark:bg-slate-900')
    content = content.replace('border-slate-100', 'border-slate-100 dark:border-slate-700')
    content = content.replace('border-slate-200', 'border-slate-200 dark:border-slate-700')
    content = content.replace('text-slate-800', 'text-slate-800 dark:text-slate-100')
    content = content.replace('text-slate-700', 'text-slate-700 dark:text-slate-200')
    content = content.replace('text-slate-500', 'text-slate-500 dark:text-slate-400')
    
    content = content.replace('bg-slate-50 dark:bg-slate-900 dark:bg-slate-900', 'bg-slate-50 dark:bg-slate-900') # fix doubles
    content = content.replace('bg-white dark:bg-slate-800/80 w-full', 'bg-white/80 dark:bg-slate-800/80 w-full') # header fix

    # 4. Inject floating toggle button right before </body>
    if 'id="theme-toggle"' not in content:
        content = content.replace('</body>', TOGGLE_BTN + '\n</body>')
        
    # 5. Inject footprint inside <main> if it exists
    if '<main' in content and '</main>' in content:
        if 'Boki Education' not in content:
            content = content.replace('</main>', FOOTER_HTML + '\n    </main>')
    else:
        # Fallback to before toggle btn
        if 'Boki Education' not in content:
            content = content.replace('</body>', FOOTER_HTML + '\n</body>')
            
    with open(path, 'w', encoding='utf-8') as file:
        file.write(content)
        
print("Successfully upgraded 10 files!")
