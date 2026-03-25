import os
import re

BASE = r'd:\01_Workspaces\Coding\Websites\boki-frontend'

# 1. Update style.css to fix "sunken" buttons and inputs
css_path = os.path.join(BASE, 'assets', 'css', 'style.css')
with open(css_path, 'r', encoding='utf-8') as f:
    css = f.read()

# Fix inputs
css = re.sub(r'\.clay-input\s*{[^}]*}', '''\.clay-input {
    background: #ffffff; border-radius: 1rem; border: 2px solid transparent; outline: none; padding: 1rem 1.5rem 1rem 3.5rem; width: 100%;
    box-shadow: inset 2px 2px 5px rgba(166, 180, 200, 0.2), inset -2px -2px 5px rgba(255, 255, 255, 0.7), 0 2px 5px rgba(0,0,0,0.02);
    transition: all 0.2s;
}''', css)

css = re.sub(r'\.dark\s*\.clay-input\s*{[^}]*}', '''.dark .clay-input {
    background: #1e293b; color: #f8fafc; border: 2px solid transparent;
    box-shadow: inset 2px 2px 5px rgba(0, 0, 0, 0.4), inset -2px -2px 5px rgba(255, 255, 255, 0.02);
}''', css)

css = re.sub(r'\.clay-input:focus\s*{[^}]*}', '''.clay-input:focus {
    border-color: #3b82f6;
    box-shadow: 0 4px 12px rgba(59, 130, 246, 0.15);
}''', css)

css = re.sub(r'\.dark\s*\.clay-input:focus\s*{[^}]*}', '''.dark .clay-input:focus {
    border-color: #60a5fa;
    box-shadow: 0 4px 12px rgba(96, 165, 250, 0.15);
}''', css)


# Fix buttons
css = re.sub(r'\.clay-btn-primary\s*{[^}]*}', '''.clay-btn-primary {
    background: #3B82F6; color: white; border-radius: 9999px;
    box-shadow: 4px 4px 10px rgba(59, 130, 246, 0.4), -4px -4px 10px rgba(255, 255, 255, 0.9), inset 2px 2px 4px rgba(255, 255, 255, 0.4);
    transition: all 0.2s ease; font-weight: 800; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 0.5rem;
}''', css)

css = re.sub(r'\.dark\s*\.clay-btn-primary\s*{[^}]*}', '''.dark .clay-btn-primary {
    background: #2563EB;
    box-shadow: 4px 4px 10px rgba(0, 0, 0, 0.5), -4px -4px 10px rgba(255, 255, 255, 0.05), inset 2px 2px 4px rgba(255, 255, 255, 0.2);
}''', css)

css = re.sub(r'\.clay-btn-secondary\s*{[^}]*}', '''.clay-btn-secondary {
    background: #ffffff; color: #3B82F6; border-radius: 9999px;
    box-shadow: 4px 4px 10px rgba(166, 180, 200, 0.3), -4px -4px 10px rgba(255, 255, 255, 0.9), inset 2px 2px 4px rgba(255, 255, 255, 0.8);
    transition: all 0.2s ease; font-weight: 800; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 0.5rem;
}''', css)

css = re.sub(r'\.dark\s*\.clay-btn-secondary\s*{[^}]*}', '''.dark .clay-btn-secondary {
    background: #1E293B; color: #60A5FA;
    box-shadow: 4px 4px 10px rgba(0, 0, 0, 0.4), -4px -4px 10px rgba(255, 255, 255, 0.05), inset 2px 2px 4px rgba(255, 255, 255, 0.1);
}''', css)

css = re.sub(r'\.clay-btn-green\s*{[^}]*}', '''.clay-btn-green {
    background: #10B981; color: white; border-radius: 9999px; display: flex; align-items: center; justify-content: center; gap: 0.5rem; padding: 1rem; width: 100%;
    box-shadow: 4px 4px 10px rgba(16, 185, 129, 0.4), -4px -4px 10px rgba(255, 255, 255, 0.9), inset 2px 2px 4px rgba(255, 255, 255, 0.4);
    transition: all 0.2s ease; font-weight: 800; cursor: pointer;
}''', css)

css = re.sub(r'\.dark\s*\.clay-btn-green\s*{[^}]*}', '''.dark .clay-btn-green {
    background: #059669;
    box-shadow: 4px 4px 10px rgba(0, 0, 0, 0.5), -4px -4px 10px rgba(255, 255, 255, 0.05), inset 2px 2px 4px rgba(255, 255, 255, 0.2);
}''', css)

css = re.sub(r'\.clay-btn-danger\s*{[^}]*}', '''.clay-btn-danger {
    background: #EF4444; color: white; border-radius: 9999px; display: inline-flex; align-items: center; justify-content: center; gap: 0.5rem;
    box-shadow: 4px 4px 10px rgba(239, 68, 68, 0.4), -4px -4px 10px rgba(255, 255, 255, 0.9), inset 2px 2px 4px rgba(255, 255, 255, 0.4);
    transition: all 0.2s ease; font-weight: 800; cursor: pointer;
}''', css)

css = re.sub(r'\.dark\s*\.clay-btn-danger\s*{[^}]*}', '''.dark .clay-btn-danger {
    background: #DC2626;
    box-shadow: 4px 4px 10px rgba(0, 0, 0, 0.5), -4px -4px 10px rgba(255, 255, 255, 0.05), inset 2px 2px 4px rgba(255, 255, 255, 0.2);
}''', css)

# Answer options
css = re.sub(r'\.answer-option\s*{[^}]*}', '''.answer-option {
    background: #fff; border-radius: 1.5rem; padding: 1rem 1.25rem; width: 100%;
    box-shadow: 4px 4px 10px rgba(166, 180, 200, 0.3), -4px -4px 10px rgba(255, 255, 255, 0.8);
    border: 3px solid transparent; cursor: pointer; transition: all 0.2s ease; text-align: left;
}''', css)

css = re.sub(r'\.dark\s*\.answer-option\s*{[^}]*}', '''.dark .answer-option {
    background: #1e293b;
    box-shadow: 4px 4px 10px rgba(0,0,0,0.4), -4px -4px 10px rgba(255,255,255,0.03);
}''', css)

with open(css_path, 'w', encoding='utf-8') as f:
    f.write(css)
print("[+] Updated style.css inputs and buttons")

# 2. Fix the overflow issue on landing pages and others with floating-bg-shape
html_files = [f for f in os.listdir(BASE) if f.endswith('.html')]
for fname in html_files:
    path = os.path.join(BASE, fname)
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    # To fix the bottom space caused by negative bottoms on absolute items
    # we can wrap the the floating backgrounds in an overflow-hidden fixed wrapper
    # But an easier way is to add `overflow-clip` to body or html.
    # Tailwind has `overflow-clip`: 'overflow-clip'
    # Wait, 'overflow-x-hidden' is already on body. Let's make sure 'overflow-clip' or 'overflow-hidden' is on body, NO, that kills scrolling.
    
    # Let's change `bottom-[-200px]` to `top-[90%]` or something, OR wrap them.
    # Wrapper approach: replace `<!-- Background Ornaments -->\n    <div class="floating-bg-shape...`
    # Replace all .floating-bg-shape blocks with a wrapper.
    
    wrapper_start = '<div class="absolute inset-0 z-[-1] overflow-hidden">\n'
    wrapper_end = '\n    </div>'
    
    if '<div class="bounding-wrapper' not in content and 'floating-bg-shape' in content:
        # Just wrap them manually
        content = re.sub(r'(<!-- Background Ornaments -->\s*)(<div class="floating-bg-shape.*</div>\s*)*', 
                        lambda m: m.group(1) + '<div class="absolute inset-x-0 inset-y-0 overflow-hidden z-[-1] pointer-events-none">\n' + m.group(0).replace('<!-- Background Ornaments -->\n', '').replace('<!-- Background Ornaments -->\r\n', '') + '\n</div>',
                        content, flags=re.MULTILINE)
        
        # Alternative simple approach: find any `bottom-[-...]` on floating shapes and turn them into appropriate relative values or just keep them in the wrapper.
    
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

print("[+] Fixed HTML Background Wrappers")
